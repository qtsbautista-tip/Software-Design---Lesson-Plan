Imports System.Web
Imports System.Web.Services
Imports System.Text
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Collections.Specialized
Imports System.Data.SqlClient

Public Class UserTable
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim dt As New DataTable

        ' Validate
        If context.Request.QueryString("action") = Nothing Then
            context.Response.Write("action is required")
            context.Response.End()
        End If

        ' Execute application commands
        Select Case context.Request.QueryString("action")
            Case "ALL"
                dt = SearchAll(context)

            Case "CHANGE_PASSWORD"
                ChangePassword(context)

            Case "SAVE"
                SaveUser(context)

        End Select

        ' Output to browser
        context.Response.ContentType = "application/json"
        context.Response.StatusCode = 200
        context.Response.Write(clsGlobal.DataTable2JSON(dt))

    End Sub
    Function SearchAll(ByVal context As HttpContext) As DataTable
        Dim r As HttpRequest = context.Request
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_usertable]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", "ALL")

                ' Store to Temporary memory
                Dim dt As New DataTable
                dt.Load(cmd.ExecuteReader())

                Return dt
            End Using ' command
        End Using ' connection
    End Function

    Private Sub ChangePassword(ByVal context As HttpContext)
        Dim email As String = context.Request.Form("email")
        Dim password As String = context.Request.Form("password")
        Dim old_pass As String = context.Request.Form("old_password")
        Dim branch As String = "MAIN"

        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()

            Using cmd As New SqlCommand("[usp_access_permission]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", "EMAIL_PASSWORD")
                cmd.Parameters.AddWithValue("@EMAIL", email)
                cmd.Parameters.AddWithValue("@Password", old_pass)
                Dim rdr As SqlDataReader = cmd.ExecuteReader()
                If rdr.HasRows = True Then
                    rdr.Close()
                    Using cmd1 As New SqlCommand("UPDATE USERTABLE SET PASSWORD=@PASSWORD WHERE EMAIL=@EMAIL AND PASSWORD=@OLD_PASS", conn)
                        cmd.CommandType = CommandType.Text
                        cmd1.Parameters.AddWithValue("@EMAIL", email)
                        cmd1.Parameters.AddWithValue("@old_pass", old_pass)
                        cmd1.Parameters.AddWithValue("@password", password)
                        cmd1.ExecuteNonQuery()
                        context.Response.Write("saved") 'empty JSON mean success

                    End Using ' command
                Else
                    context.Response.Write("Access denied!") 'empty JSON mean success
                End If
            End Using
        End Using ' connection

        context.Response.ContentType = "application/json"
        context.Response.StatusCode = 200
        context.Response.End()

    End Sub
    Private Sub SaveUser(ByVal context As HttpContext)
        Try
            Using conn As New SqlConnection(clsModule.MainDSN)
                conn.Open()
                Using cmd As New SqlCommand("[usp_usertable_create]", conn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@EMAIL", context.Request.Form("EMAIL"))
                    cmd.Parameters.AddWithValue("@NUMBER", context.Request.Form("NUMBER"))
                    cmd.Parameters.AddWithValue("@PASSWORD", context.Request.Form("PASSWORD"))
                    cmd.Parameters.AddWithValue("@TERMS", context.Request.Form("TERMS"))
                    cmd.ExecuteNonQuery()

                End Using ' command
            End Using ' connection
            context.Response.Write("success")
            context.Response.End()
        Catch ex As SqlException
            context.Response.Write("error: " & ex.Message)
            context.Response.End()
        End Try
    End Sub
    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class