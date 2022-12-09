Imports System.Web
Imports System.Web.Services
Imports System.Text
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Collections.Specialized
Imports System.Data.SqlClient

Public Class AuditLog
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        If context.Request.QueryString("action") = Nothing Then
            context.Response.Write("required param missing")
            context.Response.End()
        End If

        Dim action As String = context.Request.QueryString("action")
        action = action.ToUpper()
        If action = "ALL" Then
            RetreiveRecord(context)
        ElseIf action = "BY_DATE" Then
            RetreiveRecord(context)
            ElseIf action = "DELETE" Then
                DeleteRecord(context)
            End If

    End Sub

    
    Private Sub DeleteRecord(ByVal context As HttpContext)
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_logfle_delete]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@id", context.Request.QueryString("ID"))
                cmd.ExecuteNonQuery()

            End Using ' command
        End Using ' connection

        context.Response.ContentType = "application/json"
        context.Response.StatusCode = 200
        context.Response.Write("{}") 'empty JSON mean success
    End Sub

    Private Sub RetreiveRecord(ByVal context As HttpContext)
        Dim dt As New DataTable
        Dim _connstring As String = clsModule.ConnString("Hotel")
        If context.Request.QueryString("mainprogram") = Nothing Then
            context.Response.Write("No mainprogram selected")
            Exit Sub
        End If

        Select Case context.Request.QueryString("mainprogram")
            Case "ACCOUNTING"
                _connstring = clsModule.ConnString("Acctg")
            Case "FRONTDESK"
                _connstring = clsModule.ConnString("Hotel")
            Case "RESTAURANT"
                _connstring = clsModule.ConnString("Hotel")
        End Select

        Using conn As New SqlConnection(_connstring)
            conn.Open()
            Using cmd As New SqlCommand("[usp_logfle]", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
                cmd.Parameters.AddWithValue("@criteria", context.Request.QueryString("criteria"))
                cmd.Parameters.AddWithValue("@fromdate", context.Request.QueryString("fromdate"))
                cmd.Parameters.AddWithValue("@todate", context.Request.QueryString("todate"))
                dt.Load(cmd.ExecuteReader())
                If dt.Rows.Count > 0 Then
                    For Each col As DataColumn In dt.Columns
                        col.ColumnName = col.ColumnName.ToString().ToUpper()
                    Next
                End If
                context.Response.ContentType = "application/json"
                context.Response.StatusCode = 200
                context.Response.Write(clsGlobal.DataTable2JSON(dt))

            End Using ' command
        End Using ' connection
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class