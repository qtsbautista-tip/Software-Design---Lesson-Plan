Imports System.Web
Imports System.Web.Services
Imports System.Net.Mail
Imports System.Net


Public Class SendViaSMTP
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        If context.Request.QueryString("action") = Nothing Then
            context.Response.Write("missing_parameter")
            context.Response.End()
        End If

        Dim action As String = context.Request.QueryString("action")
        If action = "send" Then
            SendEmail(context)
        End If
    End Sub

    Private Function GetEmailDetail() As String
        Return ""
    End Function

    Private Sub SendEmail(ByVal context As HttpContext)
        Dim isvalid As Boolean = True
        Dim strFrom As String = ""
        Dim strTo As String = ""
        Dim strSubject As String = ""
        Dim strBody As String = ""

        'Validation
        If context.Request.Form("from") = Nothing Then
            isvalid = False
        ElseIf context.Request.Form("to") = Nothing Then
            isvalid = False
        ElseIf context.Request.Form("subject") = Nothing Then
            isvalid = False
        ElseIf context.Request.Form("body") = Nothing Then
            isvalid = False
        End If
        If isvalid = False Then
            context.Response.Write("required_parameters_missing")
            context.Response.End()
        End If

        strFrom = context.Request.Form("from")
        strTo = context.Request.Form("to")
        strSubject = context.Request.Form("subject")
        strBody = context.Request.Form("body")

        'Actual sending
        Dim o As MailMessage = New MailMessage(strFrom, strTo, strSubject, strSubject)
        Dim creds As NetworkCredential = New NetworkCredential("Sender Email", "Sender Password")
        Dim smtpobj As SmtpClient = New SmtpClient("smtp.live.com", 587)
        smtpobj.EnableSsl = True
        smtpobj.Credentials = creds
        smtpobj.Send(o)
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class