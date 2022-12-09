Imports System.Web
Imports System.Web.Services
Imports System.Text
Imports System.IO
Imports System.Collections.Specialized
Imports System.Data.SqlClient
Imports System.Data
Imports Newtonsoft.Json
Imports System.Web.Script.Serialization

Public Class AccessFile
    Implements System.Web.IHttpHandler

    Dim jss As JavaScriptSerializer = New JavaScriptSerializer()

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
            Case "FILTER", "ALL", "RESTAURANT", "FRONTDESK", "ACCOUNTING", "HOUSEKEEPING", "BY_ACCESS"
                dt = SearchByAccess(context)
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

    Function SearchByAccess(ByVal context As HttpContext) As DataTable
        Dim dt As New DataTable
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_accessfile]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
                cmd.Parameters.AddWithValue("@accessname", context.Request.QueryString("accessname"))
                cmd.Parameters.AddWithValue("@userid", context.Request.QueryString("userid"))
                cmd.Parameters.AddWithValue("@mainprogram", context.Request.QueryString("mainprogram"))

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
            Using cmd As New SqlCommand("[usp_accessfile]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", "ALL")

                ' Store to Temporary memory
                Dim dt As New DataTable
                dt.Load(cmd.ExecuteReader())

                Return dt
            End Using ' command
        End Using ' connection
    End Function

    Sub SaveRecord(ByVal context As HttpContext)
        Try
            Dim json As String = context.Request.QueryString("models")
            Dim objs As Object = New JavaScriptSerializer().Deserialize(Of List(Of Object))(json)
            For Each item As Object In objs
                Using conn As New SqlConnection(clsModule.MainDSN)
                    conn.Open()
                    Using cmd As New SqlCommand("[usp_accessfile_create]", conn)
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.AddWithValue("@USERID", item("USERID"))
                        cmd.Parameters.AddWithValue("@ACCESSCODE", item("ACCESSNAME"))
                        cmd.Parameters.AddWithValue("@ACCESSNAME", item("ACCESSNAME"))
                        cmd.Parameters.AddWithValue("@AMOUNT", item("AMOUNT"))
                        cmd.Parameters.AddWithValue("@MAINPROGRAM", context.Request.QueryString("mainprogram"))
                        cmd.ExecuteNonQuery()
                    End Using ' command
                End Using ' connection
                'TRANSACTION LOG
                Dim username As String = context.Request.QueryString("username")
                Dim transactionmade As String = "SAVE ACCESS"
                Dim programdesc As String = "ACCESSFILE"
                Dim particular As String = "USERID: " + item("USERID").ToString() + ", ACCESSNAME: " + item("ACCESSNAME").ToString() + ", AMOUNT: " + item("AMOUNT").ToString()
                clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)

            Next

            context.Response.Write("saved")
        Catch ex As SqlException
            context.Response.Write("error: " & ex.Message)
        End Try
        context.Response.End()

    End Sub

    Sub UpdateRecord(ByVal context As HttpContext)
        Try
            Dim json As String = context.Request.QueryString("models")
            Dim objs As Object = New JavaScriptSerializer().Deserialize(Of List(Of Object))(json)
            For Each item As Object In objs
                Using conn As New SqlConnection(clsModule.MainDSN)
                    conn.Open()
                    Using cmd As New SqlCommand("[usp_accessfile_update]", conn)
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.AddWithValue("@USERID", item("USERID"))
                        cmd.Parameters.AddWithValue("@ACCESSCODE", item("ACCESSCODE"))
                        cmd.Parameters.AddWithValue("@ACCESSNAME", item("ACCESSNAME"))
                        cmd.Parameters.AddWithValue("@AMOUNT", item("AMOUNT"))
                        cmd.Parameters.AddWithValue("@MAINPROGRAM", context.Request.QueryString("mainprogram"))
                        cmd.Parameters.AddWithValue("@ID", item("ID"))
                        cmd.ExecuteNonQuery()
                    End Using ' command
                End Using ' connection
                'TRANSACTION LOG
                Dim username As String = context.Request.QueryString("username")
                Dim transactionmade As String = "UPDATE ACCESS"
                Dim programdesc As String = "ACCESSFILE"
                Dim particular As String = "USERID: " + item("USERID").ToString() + ", ACCESSNAME: " + item("ACCESSNAME").ToString() + ", AMOUNT: " + item("AMOUNT").ToString()
                clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)
            Next
            context.Response.Write("updated")
            context.Response.End()
        Catch ex As SqlException
            context.Response.Write("error: " & ex.Message)
            context.Response.End()
        End Try
    End Sub

    Sub DeleteRecord(ByVal context As HttpContext)
        Try
            Dim json As String = context.Request.QueryString("models")
            Dim objs As Object = New JavaScriptSerializer().Deserialize(Of List(Of Object))(json)
            For Each item As Object In objs
                Using conn As New SqlConnection(clsModule.MainDSN)
                    conn.Open()
                    Using cmd As New SqlCommand("[usp_accessfile_delete]", conn)
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.AddWithValue("@criteria", item("ID"))
                        cmd.ExecuteNonQuery()
                    End Using ' command
                    'TRANSACTION LOG
                    Dim username As String = context.Request.QueryString("username")
                    Dim transactionmade As String = "DELETE ACCESS"
                    Dim programdesc As String = "ACCESSFILE"
                    Dim particular As String = "USERID: " + item("USERID").ToString() + ", ACCESSNAME: " + item("ACCESSNAME").ToString() + ", AMOUNT: " + item("AMOUNT").ToString()
                    clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)

                    context.Response.Write("deleted")
                    context.Response.End()
                End Using ' connection
            Next
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
