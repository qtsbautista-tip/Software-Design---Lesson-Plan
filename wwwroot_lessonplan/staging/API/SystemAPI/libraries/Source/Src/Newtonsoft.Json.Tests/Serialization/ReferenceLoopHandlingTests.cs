#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
#if !(NET20 || NET35)
using System.Dynamic;
#endif
using System.Runtime.Serialization;
using Newtonsoft.Json.Tests.Linq;

namespace Newtonsoft.Json.Tests.Serialization
{
  [TestFixture]
  public class ReferenceLoopHandlingTests : TestFixtureBase
  {
    [Test]
    public void ReferenceLoopHandlingTest()
    {
      JsonPropertyAttribute attribute = new JsonPropertyAttribute();
      Assert.AreEqual(null, attribute._defaultValueHandling);
      Assert.AreEqual(ReferenceLoopHandling.Error, attribute.ReferenceLoopHandling);

      attribute.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
      Assert.AreEqual(ReferenceLoopHandling.Ignore, attribute._referenceLoopHandling);
      Assert.AreEqual(ReferenceLoopHandling.Ignore, attribute.ReferenceLoopHandling);
    }

    [Test]
    public void IgnoreObjectReferenceLoop()
    {
      ReferenceLoopHandlingObjectContainerAttribute o = new ReferenceLoopHandlingObjectContainerAttribute();
      o.Value = o;

      string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        });
      Assert.AreEqual("{}", json);
    }

    [Test]
    public void IgnoreObjectReferenceLoopWithPropertyOverride()
    {
      ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride o = new ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride();
      o.Value = o;

      string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
      });
      Assert.AreEqual(@"{
  ""Value"": {
    ""Value"": {
      ""Value"": {
        ""Value"": {
          ""Value"": {
            ""Value"": null
          }
        }
      }
    }
  }
}", json);
    }

    [Test]
    public void IgnoreArrayReferenceLoop()
    {
      ReferenceLoopHandlingList a = new ReferenceLoopHandlingList();
      a.Add(a);

      string json = JsonConvert.SerializeObject(a, Formatting.Indented, new JsonSerializerSettings
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
      });
      Assert.AreEqual("[]", json);
    }

    [Test]
    public void IgnoreDictionaryReferenceLoop()
    {
      ReferenceLoopHandlingDictionary d = new ReferenceLoopHandlingDictionary();
      d.Add("First", d);

      string json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
      });
      Assert.AreEqual("{}", json);
    }

    [Test]
    public void SerializePropertyItemReferenceLoopHandling()
    {
      PropertyItemReferenceLoopHandling c = new PropertyItemReferenceLoopHandling();
      c.Text = "Text!";
      c.SetData(new List<PropertyItemReferenceLoopHandling> { c });

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Text"": ""Text!"",
  ""Data"": [
    {
      ""Text"": ""Text!"",
      ""Data"": [
        {
          ""Text"": ""Text!"",
          ""Data"": [
            {
              ""Text"": ""Text!"",
              ""Data"": null
            }
          ]
        }
      ]
    }
  ]
}", json);
    }

#if !(PORTABLE || SILVERLIGHT || NETFX_CORE || PORTABLE40)
    public class MainClass : ISerializable
    {
      public ChildClass Child { get; set; }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        info.AddValue("Child", Child);
      }
    }

    public class ChildClass : ISerializable
    {
      public string Name { get; set; }
      public MainClass Parent { get; set; }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        info.AddValue("Parent", Parent);
        info.AddValue("Name", Name);
      }
    }

    [Test]
    public void ErrorISerializableCyclicReferenceLoop()
    {
      var main = new MainClass();
      var child = new ChildClass();

      child.Name = "Child1";
      child.Parent = main; // Obvious Circular Reference

      main.Child = child;

      var settings =
          new JsonSerializerSettings();

      ExceptionAssert.Throws<JsonSerializationException>(
        "Self referencing loop detected with type 'Newtonsoft.Json.Tests.Serialization.ReferenceLoopHandlingTests+MainClass'. Path 'Child'.",
        () => JsonConvert.SerializeObject(main, settings));
    }

    [Test]
    public void IgnoreISerializableCyclicReferenceLoop()
    {
      var main = new MainClass();
      var child = new ChildClass();

      child.Name = "Child1";
      child.Parent = main; // Obvious Circular Reference

      main.Child = child;

      var settings =
          new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

      var c = JsonConvert.SerializeObject(main, settings);
      Assert.AreEqual(@"{""Child"":{""Name"":""Child1""}}", c);
    }
#endif

#if !(NET20 || NET35 || PORTABLE40)
    public class DictionaryDynamicObject : DynamicObject
    {
      public IDictionary<string, object> Values { get; private set; }

      public DictionaryDynamicObject()
      {
        Values = new Dictionary<string, object>();
      }

      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
        Values[binder.Name] = value;
        return true;
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
        return Values.TryGetValue(binder.Name, out result);
      }

      public override IEnumerable<string> GetDynamicMemberNames()
      {
        return Values.Keys;
      }
    }

    [Test]
    public void ErrorDynamicCyclicReferenceLoop()
    {
      dynamic parent = new DictionaryDynamicObject();
      dynamic child = new DictionaryDynamicObject();
      parent.child = child;
      child.parent = parent;

      var settings = new JsonSerializerSettings();

      ExceptionAssert.Throws<JsonSerializationException>(
        "Self referencing loop detected with type 'Newtonsoft.Json.Tests.Serialization.ReferenceLoopHandlingTests+DictionaryDynamicObject'. Path 'child'.",
        () => JsonConvert.SerializeObject(parent, settings));
    }

    [Test]
    public void IgnoreDynamicCyclicReferenceLoop()
    {
      dynamic parent = new DictionaryDynamicObject();
      dynamic child = new DictionaryDynamicObject();
      parent.child = child;
      parent.name = "parent";
      child.parent = parent;
      child.name = "child";

      var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

      var c = JsonConvert.SerializeObject(parent, settings);
      Assert.AreEqual(@"{""child"":{""name"":""child""},""name"":""parent""}", c);
    }
#endif
  }

  public class PropertyItemReferenceLoopHandling
  {
    private IList<PropertyItemReferenceLoopHandling> _data;
    private int _accessCount;

    public string Text { get; set; }

    [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public IList<PropertyItemReferenceLoopHandling> Data
    {
      get
      {
        if (_accessCount >= 3)
          return null;

        _accessCount++;
        return new List<PropertyItemReferenceLoopHandling>(_data);
      }
    }

    public void SetData(IList<PropertyItemReferenceLoopHandling> data)
    {
      _data = data;
    }
  }

  [JsonArray(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingList : List<ReferenceLoopHandlingList>
  {
  }

  [JsonDictionary(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingDictionary : Dictionary<string, ReferenceLoopHandlingDictionary>
  {
  }

  [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingObjectContainerAttribute
  {
    public ReferenceLoopHandlingObjectContainerAttribute Value { get; set; }
  }

  [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride
  {
    private ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride _value;
    private int _getCount;

    [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride Value
    {
      get
      {
        if (_getCount < 5)
        {
          _getCount++;
          return _value;
        }
        return null;
      }
      set { _value = value; }
    }
  }
}
