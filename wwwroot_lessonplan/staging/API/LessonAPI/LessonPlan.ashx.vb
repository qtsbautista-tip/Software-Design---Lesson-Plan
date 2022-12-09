Imports System.Web
Imports System.Web.Services
Imports System.Text
Imports System.IO
Imports System.Collections.Specialized
Imports System.Data.SqlClient
Imports System.Data
Imports Newtonsoft.Json
Imports System.Web.Script.Serialization

Public Class LessonPlan
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
            Case "BY_ID", "BY_DATE", "BY_STATUS"
                dt = SearchBy(context)
            Case "ILO"
                dt = SearchILO(context)
            Case "SAVE"
                SaveRecord(context)
            Case "UPDATE"
                UpdateRecord(context)
            Case "DELETE"
                DeleteRecord(context)
            Case "UPDATE_STATUS"
                UpdateStatus(context)
        End Select

        ' Output to browser
        context.Response.ContentType = "application/json"
        context.Response.StatusCode = 200
        context.Response.Write(clsGlobal.DataTable2JSON(dt))
    End Sub
    Sub UpdateStatus(ByVal context As HttpContext)
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Dim transaction As SqlTransaction
            ' Start a local transaction
            transaction = conn.BeginTransaction("transaction")
            Try
                Using cmd As New SqlCommand("UPDATE LESSONPLANHEAD SET STATUS=@STATUS WHERE PLAN_CODE=@PLAN_CODE", conn, transaction)
                    cmd.CommandType = CommandType.Text
                    cmd.Parameters.AddWithValue("@PLAN_CODE", context.Request.QueryString("PLAN_CODE"))
                    cmd.Parameters.AddWithValue("@STATUS", context.Request.QueryString("STATUS"))
                    cmd.ExecuteNonQuery()
                End Using ' command
                ' Commit finally to sqlserver database
                transaction.Commit()
                context.Response.Write("success")
                context.Response.End()
            Catch ex As Exception
                ' Rollback any changes
                transaction.Rollback()
                context.Response.Write(ex.Message.ToString())
                context.Response.End()
            End Try
        End Using
    End Sub
    Function SearchILO(ByVal context As HttpContext) As DataTable
        Dim dt As New DataTable
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_lessonplandetail]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", "BY_ID")
                cmd.Parameters.AddWithValue("@criteria", context.Request.QueryString("criteria"))

                ' Store to Temporary memory
                dt.Load(cmd.ExecuteReader())

            End Using ' command
        End Using ' connection
        Return dt
    End Function
    Function SearchBy(ByVal context As HttpContext) As DataTable
        Dim dt As New DataTable
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_lessonplanhead]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
                cmd.Parameters.AddWithValue("@criteria", context.Request.QueryString("criteria"))
                cmd.Parameters.AddWithValue("@status", context.Request.QueryString("status"))

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
            Using cmd As New SqlCommand("[usp_lessonplanhead]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
                cmd.Parameters.AddWithValue("@criteria", context.Request.QueryString("criteria"))
                cmd.Parameters.AddWithValue("@status", context.Request.QueryString("status"))

                ' Store to Temporary memory
                Dim dt As New DataTable
                dt.Load(cmd.ExecuteReader())

                Return dt
            End Using ' command
        End Using ' connection
    End Function

    Sub SaveRecord(ByVal context As HttpContext)
        Dim id As String = ""
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Dim transaction As SqlTransaction

            ' Start a local transaction
            transaction = conn.BeginTransaction("transaction")
            Try
                Dim plan_code = ""
                Using cmd As New SqlCommand("[usp_lessonplanhead_create]", conn, transaction)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@PLAN_CODE", context.Request.QueryString("PLAN_CODE"))
                    cmd.Parameters.AddWithValue("@COURSE_CODE", context.Request.QueryString("COURSE_CODE"))
                    cmd.Parameters.AddWithValue("@TITLE", context.Request.QueryString("TITLE"))
                    cmd.Parameters.AddWithValue("@DATE", context.Request.QueryString("DATE"))
                    cmd.Parameters.AddWithValue("@TIMEIN", context.Request.QueryString("TIMEIN"))
                    cmd.Parameters.AddWithValue("@TIMEOUT", context.Request.QueryString("TIMEOUT"))
                    cmd.Parameters.AddWithValue("@STATUS", "Pending")
                    cmd.Parameters.AddWithValue("@TYPE", context.Request.QueryString("TYPE"))
                    Dim rdr As SqlDataReader = cmd.ExecuteReader
                    If rdr.HasRows Then
                        rdr.Read()
                        plan_code = rdr("PLAN_CODE").ToString
                    End If
                    rdr.Close()
                End Using ' command

                SaveDetail(context, plan_code, conn, transaction)
                context.Response.Write("success")
                ' Commit finally to sqlserver database
                transaction.Commit()
                context.Response.End()
            Catch ex As Exception
                ' Rollback any changes
                transaction.Rollback()
                context.Response.Write(ex.Message.ToString())
                context.Response.End()
            End Try
        End Using

    End Sub
    Sub SaveDetail(ByVal context As HttpContext, ByVal plan_code As String, ByVal conn As SqlConnection, ByVal transaction As SqlTransaction)
        Dim details As String = context.Request.Form("details")
        Dim details_recoord As Object = New JavaScriptSerializer().Deserialize(Of List(Of Object))(details)
        For Each item As Object In details_recoord
            If item("TOPICS") <> "" Then
                Using cmd As New SqlCommand("[usp_lessonplandetail_create]", conn, transaction)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@STATUS", item("STATUS"))
                    cmd.Parameters.AddWithValue("@TOPICS", item("TOPICS"))
                    cmd.Parameters.AddWithValue("@PLAN_CODE", plan_code)
                    cmd.Parameters.AddWithValue("@COURSE_CODE", context.Request.QueryString("COURSE_CODE"))
                    cmd.ExecuteNonQuery()
                End Using ' command
            End If
        Next

    End Sub
    Sub EditRecord(ByVal context As HttpContext, ByVal conn As SqlConnection, ByVal transaction As SqlTransaction)
        Using cmd As New SqlCommand("[usp_lessonplanhead_update]", conn, transaction)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@PLAN_CODE", context.Request.QueryString("PLAN_CODE"))
            cmd.Parameters.AddWithValue("@COURSE_CODE", context.Request.QueryString("COURSE_CODE"))
            cmd.Parameters.AddWithValue("@TITLE", context.Request.QueryString("TITLE"))
            cmd.Parameters.AddWithValue("@DATE", context.Request.QueryString("DATE"))
            cmd.Parameters.AddWithValue("@TIMEIN", context.Request.QueryString("TIMEIN"))
            cmd.Parameters.AddWithValue("@TIMEOUT", context.Request.QueryString("TIMEOUT"))
            cmd.Parameters.AddWithValue("@TYPE", context.Request.QueryString("TYPE"))
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Sub UpdateRecord(ByVal context As HttpContext)
        Dim plan_code As String = context.Request.QueryString("plan_code")
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Dim transaction As SqlTransaction

            ' Start a local transaction
            transaction = conn.BeginTransaction("transaction")
            Try
                Using cmd1 As New SqlCommand("[usp_lessonplandetail_delete]", conn, transaction)
                    cmd1.CommandType = CommandType.StoredProcedure
                    cmd1.Parameters.AddWithValue("@plan_code", plan_code)
                    cmd1.ExecuteNonQuery()
                End Using ' commandand
                EditRecord(context, conn, transaction)
                SaveDetail(context, plan_code, conn, transaction)
                ' Commit finally to sqlserver database
                transaction.Commit()
                context.Response.Write("success")
                context.Response.End()
            Catch ex As Exception
                ' Rollback any changes
                transaction.Rollback()
                context.Response.Write(ex.Message.ToString())
                context.Response.End()
            End Try
        End Using
    End Sub
    Sub DeleteRecord(ByVal context As HttpContext)
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Dim transaction As SqlTransaction
            ' Start a local transaction
            transaction = conn.BeginTransaction("transaction")
            Try
                Using cmd1 As New SqlCommand("[usp_lessonplandetail_delete]", conn, transaction)
                    cmd1.CommandType = CommandType.StoredProcedure
                    cmd1.Parameters.AddWithValue("@plan_code", context.Request.QueryString("plan_code"))
                    cmd1.ExecuteNonQuery()
                End Using ' command
                Using cmd1 As New SqlCommand("[usp_lessonplanhead_delete]", conn, transaction)
                    cmd1.CommandType = CommandType.StoredProcedure
                    cmd1.Parameters.AddWithValue("@plan_code", context.Request.QueryString("plan_code"))
                    cmd1.ExecuteNonQuery()
                End Using ' command

                ' Commit finally to sqlserver database
                transaction.Commit()
                context.Response.Write("deleted")
                context.Response.End()
            Catch ex As Exception
                ' Rollback any changes
                transaction.Rollback()
                context.Response.Write(ex.Message.ToString())
                context.Response.End()
            End Try
        End Using
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
