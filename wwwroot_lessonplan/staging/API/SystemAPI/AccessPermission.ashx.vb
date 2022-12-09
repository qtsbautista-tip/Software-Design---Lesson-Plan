Imports System.Web
Imports System.Web.Services
Imports System.Collections.Specialized
Imports System.Data.SqlClient
Imports System.Net
Imports System.Globalization
Imports System.Device.Location
Public Class AccessPermission
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Call UserStatusCount(context)

        Call ChangeUserStatus(context)

        Call GetUserInfo(context)

        Call ValidateRequest(context)



        Dim myWebClient As New WebClient()
        Dim myNameValueCollection As New NameValueCollection()
        Dim uriString As String = "https://semaphore.co/api/v4/messages"
        Dim message = ""
        Dim action As String = context.Request.Form("action")

        ' NOTE: Always read the filename of this code
        Using conn As New SqlConnection(clsModule.MainDSN)
            conn.Open()
            Using cmd As New SqlCommand("[usp_access_permission]", conn)
                cmd.CommandType = CommandType.StoredProcedure

                ' Set Parameters
                cmd.Parameters.AddWithValue("@action", context.Request.Form("action"))

                Dim _invalid As Boolean = False
                If action.StartsWith("EMAIL_PASSWORD") Then
                    If context.Request.Form("email") = Nothing And context.Request.Form("password") = Nothing Then
                        _invalid = True
                    Else
                        cmd.Parameters.AddWithValue("@email", context.Request.Form("email"))
                        cmd.Parameters.AddWithValue("@password", context.Request.Form("password"))
                    End If
                End If
                If action.StartsWith("PASSWORD") Then
                    If context.Request.Form("password") = Nothing Then
                        _invalid = True
                    Else
                        cmd.Parameters.AddWithValue("@password", context.Request.Form("password"))
                    End If
                End If


                If _invalid Then
                    context.Response.StatusCode = 400
                    context.Response.Write("missing parameter")
                    context.Response.End()
                End If

                ' Store to Temporary memory
                Dim dt As New DataTable
                dt.Load(cmd.ExecuteReader())

                If context.Request.Form("login") = "true" And dt.Rows.Count > 0 Then
                    'TRANSACTION LOG
                    Dim username As String = dt.Rows(0)("EMAIL").ToString()
                    Dim transactionmade As String = "LOGIN in " + context.Request.Form("module")
                    Dim programdesc As String = "LOGIN"
                    Dim particular As String = ""
                    clsGlobal.KeyINLOG(username, transactionmade, programdesc, particular)

                End If

                ' Output to browser
                context.Response.ContentType = "application/json"
                context.Response.StatusCode = 200
                context.Response.Write(clsGlobal.DataTable2JSON(dt))
            End Using ' command
        End Using ' connection

    End Sub



    ' Standard Validation of QueryString
    Sub ValidateRequest(ByVal context As HttpContext)
        Dim invalid As Boolean = False

				
        'If context.Request.Form("callback") = Nothing Then
        '    invalid = True
        'End If
        'If context.Request.Form("action") = Nothing Then
        '    invalid = True
        'End If

        ' Output to browser
        If invalid Then
            'context.Response.ContentType = "application/json"
            'context.Response.StatusCode = 400
            context.Response.Write("invalid or missing parameter")
            context.Response.End()
        End If
    End Sub

		Sub GetUserInfo(ByVal context As HttpContext)
			If context.Request.QueryString("action") = "GETUSERINFO" Then
							Using conn As New SqlConnection(clsModule.MainDSN)
								conn.Open()
								Using cmd As New SqlCommand("[usp_access_permission]", conn)
									cmd.CommandType = CommandType.StoredProcedure
									cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
									cmd.Parameters.AddWithValue("@userid", context.Request.QueryString("userid"))
									Dim dt As New DataTable
									dt.Load(cmd.ExecuteReader())
										' Output to browser
									context.Response.ContentType = "application/json"
									context.Response.StatusCode = 200
									context.Response.Write(clsGlobal.DataTable2JSON(dt))
								End Using
							End Using
							context.Response.End()
			End If
		End Sub

		Sub ChangeUserStatus(ByVal context As HttpContext)
			If context.Request.QueryString("action") = "CHANGESTATUS" Then
							Using conn As New SqlConnection(clsModule.MainDSN)
								conn.Open()
								Using cmd As New SqlCommand("UPDATE usertable SET [status] = @status WHERE userid = @userid", conn)
									cmd.CommandType = CommandType.Text
									cmd.Parameters.AddWithValue("@status", context.Request.QueryString("status"))
									cmd.Parameters.AddWithValue("@userid", context.Request.QueryString("userid"))
									cmd.ExecuteNonQuery()
									' Output to browser
									context.Response.StatusCode = 200
									context.Response.Write("changed")
								End Using
							End Using
							context.Response.End()
			End If
		End Sub

		Sub UserStatusCount(ByVal context As HttpContext)
			If context.Request.QueryString("action") = "USERSTATUSCOUNT" Then
            Using conn As New SqlConnection(clsModule.MainDSN)
                conn.Open()
                Using cmd As New SqlCommand("SELECT USERID,CASHIER,ACCOUNTING FROM usertable WHERE status = @status", conn)
                    cmd.CommandType = CommandType.Text
                    cmd.Parameters.AddWithValue("@status", context.Request.QueryString("status"))
                    Dim dt As New DataTable
                    dt.Load(cmd.ExecuteReader())
                    ' Output to browser
                    context.Response.ContentType = "application/json"
                    context.Response.StatusCode = 200
                    context.Response.Write(clsGlobal.DataTable2JSON(dt))
                End Using
            End Using
							context.Response.End()
			End If
		End Sub



    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class