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

#if !(PORTABLE40 || NET20 || NET35)
using System;
using System.Diagnostics;
using System.Reflection;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.Serialization;

namespace Newtonsoft.Json.Tests.Utilities
{
  [TestFixture]
  public class ExpressionReflectionDelegateFactoryTests : TestFixtureBase
  {
    [Test]
    public void DefaultConstructor()
    {
      Func<object> create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Movie));

      Movie m = (Movie)create();
      Assert.IsNotNull(m);
    }

    [Test]
    public void DefaultConstructor_Struct()
    {
      Func<object> create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(StructTest));

      StructTest m = (StructTest)create();
      Assert.IsNotNull(m);
    }

    [Test]
    public void DefaultConstructor_Abstract()
    {
      ExceptionAssert.Throws<Exception>("Cannot create an abstract class.",
      () =>
      {
        Func<object> create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Type));

        create();
      }); 
    }

    [Test]
    public void CreatePropertySetter()
    {
      Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

      Movie m = new Movie();

      setter(m, "OH HAI!");

      Assert.AreEqual("OH HAI!", m.Name);
    }

    [Test]
    public void CreatePropertyGetter()
    {
      Func<object, object> getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

      Movie m = new Movie();
      m.Name = "OH HAI!";

      object value = getter(m);

      Assert.AreEqual("OH HAI!", value);
    }

    [Test]
    public void CreateMethodCall()
    {
      MethodCall<object, object> method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetMethod("ToString"));

      Movie m = new Movie();
      object result = method(m);
      Assert.AreEqual("Newtonsoft.Json.Tests.TestObjects.Movie", result);

      method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetMethod("Equals"));

      result = method(m, m);
      Assert.AreEqual(true, result);
    }

    [Test]
    public void CreateMethodCall_Constructor()
    {
      MethodCall<object, object> method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetConstructor(new Type[0]));

      object result = method(null);

      Assert.IsTrue(result is Movie);
    }

    public static class StaticTestClass
    {
      public static string StringField;
      public static string StringProperty { get; set; }
    }
    
    [Test]
    public void GetStatic()
    {
      StaticTestClass.StringField = "Field!";
      StaticTestClass.StringProperty = "Property!";

      Func<object, object> getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(StaticTestClass).GetProperty("StringProperty"));

      object v = getter(null);
      Assert.AreEqual(StaticTestClass.StringProperty, v);

      getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(StaticTestClass).GetField("StringField"));

      v = getter(null);
      Assert.AreEqual(StaticTestClass.StringField, v);
    }

    [Test]
    public void SetStatic()
    {
      Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StaticTestClass).GetProperty("StringProperty"));

      setter(null, "New property!");
      Assert.AreEqual("New property!", StaticTestClass.StringProperty);

      setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StaticTestClass).GetField("StringField"));

      setter(null, "New field!");
      Assert.AreEqual("New field!", StaticTestClass.StringField);
    }

    public class FieldsTestClass
    {
      public string StringField;
      public bool BoolField;

      public readonly int IntReadOnlyField = int.MaxValue;
    }

    [Test]
    public void CreateGetField()
    {
      FieldsTestClass c = new FieldsTestClass
      {
        BoolField = true,
        StringField = "String!"
      };

      Func<object, object> getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(FieldsTestClass).GetField("StringField"));

      object value = getter(c);
      Assert.AreEqual("String!", value);

      getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(FieldsTestClass).GetField("BoolField"));

      value = getter(c);
      Assert.AreEqual(true, value);
    }

    [Test]
    public void CreateSetField_ReadOnly()
    {
      FieldsTestClass c = new FieldsTestClass();

      Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(FieldsTestClass).GetField("IntReadOnlyField"));

      setter(c, int.MinValue);
      Assert.AreEqual(int.MinValue, c.IntReadOnlyField);
    }

    [Test]
    public void CreateSetField()
    {
      FieldsTestClass c = new FieldsTestClass();

      Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(FieldsTestClass).GetField("StringField"));

      setter(c, "String!");
      Assert.AreEqual("String!", c.StringField);

      setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(FieldsTestClass).GetField("BoolField"));

      setter(c, true);
      Assert.AreEqual(true, c.BoolField);
    }

    [Test]
    public void SetOnStruct()
    {
      object structTest = new StructTest();

      Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetProperty("StringProperty"));

      setter(structTest, "Hi1");
      Assert.AreEqual("Hi1", ((StructTest)structTest).StringProperty);

      setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetField("StringField"));

      setter(structTest, "Hi2");
      Assert.AreEqual("Hi2", ((StructTest)structTest).StringField);
    }

    [Test]
    public void CreateGetWithBadObjectTarget()
    {
      ExceptionAssert.Throws<InvalidCastException>("Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.",
      () =>
      {
        Person p = new Person();
        p.Name = "Hi";

        Func<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

        setter(p);
      });
    }

    [Test]
    public void CreateSetWithBadObjectTarget()
    {
      ExceptionAssert.Throws<InvalidCastException>("Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.",
      () =>
      {
        Person p = new Person();
        Movie m = new Movie();

        Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

        setter(m, "Hi");

        Assert.AreEqual(m.Name, "Hi");

        setter(p, "Hi");

        Assert.AreEqual(p.Name, "Hi");
      });
    }

    [Test]
    public void CreateSetWithBadObjectValue()
    {
      ExceptionAssert.Throws<InvalidCastException>("Unable to cast object of type 'System.Version' to type 'System.String'.",
      () =>
      {
        Movie m = new Movie();

        Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

        setter(m, new Version("1.1.1.1"));
      });
    }

    [Test]
    public void CreateStaticMethodCall()
    {
      MethodInfo castMethodInfo = typeof(JsonSerializerTest.DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

      Assert.IsNotNull(castMethodInfo);

      MethodCall<object, object> call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

      object result = call(null, "First!");
      Assert.IsNotNull(result);

      JsonSerializerTest.DictionaryKey key = (JsonSerializerTest.DictionaryKey) result;
      Assert.AreEqual("First!", key.Value);
    }
  }
}
#endif