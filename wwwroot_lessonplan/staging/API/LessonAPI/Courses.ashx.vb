Imports System.Web
Imports System.Web.Services
Imports System.Text
Imports System.IO
Imports System.Collections.Specialized
Imports System.Data.SqlClient
Imports System.Data
Imports Newtonsoft.Json
Imports System.Web.Script.Serialization

Public Class Courses
    Implements System.Web.IHttpHandler

    Dim jss As JavaScriptSerializer = New JavaScriptSerializer()
    Dim output As New clsResultData()

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        ' NOTE: Always read the filename of this code
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
            Case "BY_ID"
                dt = SearchByID(context)
            Case "SAVE"
                SaveRecord(context)
            Case "UPDATE"
                UpdateRecord(context)
            Case "DELETE"
                DeleteRecord(context)
        End Select

        ' Output to browser
        context.Response.ContentType = "application/json"
        context.Response.StatusCode = 200
        context.Response.Write(clsGlobal.DataTable2JSON(dt))
    End Sub
    Function SearchByID(ByVal context As HttpContext) As DataTable
        Dim dt As New DataTable
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_courses]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", "BY_ID")
                cmd.Parameters.AddWithValue("@criteria", context.Request.QueryString("criteria"))

                ' Store to Temporary memory
                dt.Load(cmd.ExecuteReader())

            End Using ' command
        End Using ' connection
        Return dt
    End Function
    Function SearchAll(ByVal context As HttpContext) As DataTable
        Dim r As HttpRequest = context.Request
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_courses]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
                cmd.Parameters.AddWithValue("@criteria", context.Request.QueryString("criteria"))

                ' Store to Temporary memory
                Dim dt As New DataTable
                dt.Load(cmd.ExecuteReader())

                Return dt
            End Using ' command
        End Using ' connection
    End Function

    Sub SaveRecord(ByVal context As HttpContext)
        Dim id As String = ""
        Try
            Using conn As New SqlConnection(clsModule.MainDSN)
                conn.Open()
                Using cmd As New SqlCommand("[usp_courses_create]", conn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ASSIGNTO", context.Request.Form("ASSIGNTO"))
                    cmd.Parameters.AddWithValue("@STATUS", context.Request.Form("STATUS"))
                    cmd.Parameters.AddWithValue("@ASSIGNBY", context.Request.QueryString("USER"))
                    cmd.Parameters.AddWithValue("@REMARKS", context.Request.Form("REMARKS"))
                    cmd.Parameters.AddWithValue("@TASKS", context.Request.Form("TASKS"))
                    cmd.Parameters.AddWithValue("@PRIORITY", context.Request.Form("PRIORITY"))
                    cmd.Parameters.AddWithValue("@TRANDATE", context.Request.Form("TRANDATE"))
                    cmd.Parameters.AddWithValue("@DUEDATE", context.Request.Form("DUEDATE"))
                    cmd.Parameters.AddWithValue("@PROGRESS", context.Request.Form("PROGRESS"))
                    Dim rdr As SqlDataReader = cmd.ExecuteReader
                    If rdr.HasRows Then
                        rdr.Read()
                        'Forward generated PONO when saving details
                        id = rdr("ID").ToString
                    End If
                    rdr.Close()
                End Using ' command
            End Using ' connection

            'TRANSACTION LOG
            Dim username As String = context.Request.QueryString("user")
            Dim transactionmade As String = "NEW RECORD"
            Dim programdesc As String = "TASK FILE"
            Dim particular As String = "ID: " + id + ", TASK: " + context.Request.Form("TASKS") + ", ASSIGNTO: " + context.Request.Form("ASSIGNTO")
            clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)

            context.Response.Write("saved" + id)
            context.Response.End()
        Catch ex As SqlException
            context.Response.Write("error: " & ex.Message)
            context.Response.End()
        End Try

    End Sub
    Sub UpdateRecord(ByVal context As HttpContext)
        Try
            Using conn As New SqlConnection(clsModule.MainDSN)
                conn.Open()
                Using cmd As New SqlCommand("[usp_courses_update]", conn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ASSIGNTO", context.Request.Form("ASSIGNTO"))
                    cmd.Parameters.AddWithValue("@STATUS", context.Request.Form("STATUS"))
                    cmd.Parameters.AddWithValue("@ASSIGNBY", context.Request.QueryString("USER"))
                    cmd.Parameters.AddWithValue("@REMARKS", context.Request.Form("REMARKS"))
                    cmd.Parameters.AddWithValue("@TASKS", context.Request.Form("TASKS"))
                    cmd.Parameters.AddWithValue("@PRIORITY", context.Request.Form("PRIORITY"))
                    cmd.Parameters.AddWithValue("@TRANDATE", context.Request.Form("TRANDATE"))
                    cmd.Parameters.AddWithValue("@DUEDATE", context.Request.Form("DUEDATE"))
                    cmd.Parameters.AddWithValue("@PROGRESS", context.Request.Form("PROGRESS"))
                    cmd.Parameters.AddWithValue("@ID", context.Request.Form("ID"))
                    cmd.ExecuteNonQuery()
                End Using ' command
            End Using ' connection
            'TRANSACTION LOG
            Dim username As String = context.Request.QueryString("user")
            Dim transactionmade As String = "NEW RECORD"
            Dim programdesc As String = "TASK FILE"
            Dim particular As String = "ID: " + context.Request.Form("ID") + ", TASK: " + context.Request.Form("TASKS") + ", ASSIGNTO: " + context.Request.Form("ASSIGNTO")
            clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)
            context.Response.Write("updated")
            context.Response.End()
        Catch ex As SqlException
            context.Response.Write("error: " & ex.Message)
            context.Response.End()
        End Try
    End Sub

    Sub DeleteRecord(ByVal context As HttpContext)
        Try
            Using conn As New SqlConnection(clsModule.MainDSN)
                conn.Open()
                Using cmd As New SqlCommand("[usp_courses_delete]", conn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ID", context.Request.QueryString("id"))
                    cmd.ExecuteNonQuery()
                End Using ' command

                'TRANSACTION LOG
                Dim username As String = context.Request.QueryString("user")
                Dim transactionmade As String = "DELETE RECORD"
                Dim programdesc As String = "TASK FILE"
                Dim particular As String = "ID: " + context.Request.QueryString("id")
                clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)

                context.Response.Write("deleted")
                context.Response.End()
            End Using ' connection
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
