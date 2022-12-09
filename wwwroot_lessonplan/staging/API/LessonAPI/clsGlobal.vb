Imports System.Web.Script.Serialization
Imports Newtonsoft.Json

Public Class clsGlobal

    Public Shared Sub LoggedInUser(ByVal userid As String, ByVal username As String, ByVal userlevel As String)
        HttpContext.Current.Session.Add("UserLoggedIn", True)
        HttpContext.Current.Session.Add("UserID", userid)
        HttpContext.Current.Session.Add("UserName", username)
        HttpContext.Current.Session.Add("UserLevel", userlevel)
        KeyINLOG(username, "LOGIN", "Logged-In")

        Dim str As String = clsModule.MainDSN()
        Using con As New System.Data.SqlClient.SqlConnection(str)
            Using cmd As New System.Data.SqlClient.SqlCommand("SELECT top 1 * FROM logs WHERE username = @username", con)
                cmd.CommandType = CommandType.Text
                cmd.Parameters.AddWithValue("@username", username)
                con.Open()
                Dim dr As System.Data.SqlClient.SqlDataReader = cmd.ExecuteReader()
                If dr.HasRows Then
                    Do While dr.Read()
                        Dim a As Date = IIf(IsDBNull(dr("logdatetime")), DateTime.Now(), Convert.ToDateTime(dr("logdatetime")))
                        HttpContext.Current.Session.Add("UserLastLogin", a)
                    Loop
                End If
                dr.Close()
            End Using
        End Using
    End Sub

    Public Shared Sub LogoutUser()
        KeyINLOG(HttpContext.Current.Session.Item("UserName"), "LOGOUT", "Logged-Out")
        HttpContext.Current.Session.Remove("UserLoggedIn")
        HttpContext.Current.Session.Remove("UserID")
        HttpContext.Current.Session.Remove("UserName")
        HttpContext.Current.Session.Remove("UserLevel")
        HttpContext.Current.Session.Clear()
    End Sub

    Public Shared Sub KeyINLOG(ByVal username As String, ByVal transactionmade As String, ByVal programdesc As String, Optional ByVal particular As String = "-")
        Dim str As String = clsModule.MainDSN
        Using con As New System.Data.SqlClient.SqlConnection(str)
            con.Open()
            Using cmd As New System.Data.SqlClient.SqlCommand("usp_logfle_create", con)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@logdate", DateTime.Now())
                cmd.Parameters.AddWithValue("@logtime", DateTime.Now())
                cmd.Parameters.AddWithValue("@username", username)
                cmd.Parameters.AddWithValue("@transactionmade", transactionmade)
                cmd.Parameters.AddWithValue("@programdesc", programdesc)
                cmd.Parameters.AddWithValue("@particular", particular)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Function IsLoggedIn() As Boolean
        If (HttpContext.Current.Session IsNot Nothing) Then
            Dim item As Object = HttpContext.Current.Session("UserLoggedIn")
            Return (item IsNot Nothing)
        End If
        Return False
    End Function
    Public Shared Function DataTable2JSON(ByVal dt As DataTable) As String
        Dim json As New JavaScriptSerializer()
        json.MaxJsonLength = Int32.MaxValue
        Return json.Serialize(From dr As DataRow In dt.Rows Select dt.Columns.Cast(Of DataColumn)().ToDictionary(Function(col) col.ColumnName, Function(col) dr(col)))
    End Function
    Public Shared Function DataSet2JSON(ByVal dts As DataSet) As String
        Return JsonConvert.SerializeObject(dts, Formatting.Indented)
    End Function
    Public Shared Sub MsgAlert(ByVal p As System.Web.UI.Page, ByVal msg As String)
        System.Web.UI.ScriptManager.RegisterClientScriptBlock(p, p.GetType(), "MsgAlert", "alert('" & msg & "');", True)
    End Sub

    Public Shared Sub CallJavaScript(ByVal p As System.Web.UI.Page, ByVal js As String)
        System.Web.UI.ScriptManager.RegisterClientScriptBlock(p, p.GetType(), "CallJavaScript", js, True)
    End Sub

End Class
