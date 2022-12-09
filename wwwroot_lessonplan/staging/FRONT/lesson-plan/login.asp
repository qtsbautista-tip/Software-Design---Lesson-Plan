<!--#include file="includes/easyasp/easp.asp"-->
<!--#include file="includes/global_variables.asp"-->
<!--#include file="includes/helper.asp"-->
<%  
Dim errmsg
errmsg = "status=error&msg=Invalid Username Or Password!"
  
If Request.Form("email") <> "" AND Request.Form("password") <> "" Then
  Dim islogged

  Dim tmp
  Dim obj	
	Dim objHttp, params
	params = "login=true&module=LESSONPLAN&action=EMAIL_PASSWORD&email=" & Request.Form("email") & "&password="& Request.Form("password") & "&callback=none"
	
	
	Set objHttp = Server.CreateObject("WinHTTP.WinHTTPRequest.5.1")
	objHttp.open "POST", system_api_url & "/AccessPermission.ashx", False
	objHttp.setRequestHeader "Content-type", "application/x-www-form-urlencoded"
	objHttp.Send(params)
	tmp = objHttp.responseText 
	Set objHttp = Nothing 

  'Test'
	'Response.Write(tmp)
	'Response.End()
	
  If tmp <> "[]" Then
     Set obj = Easp.Decode(tmp)
     islogged = True
  Else
    islogged = False
  End If
  
  If islogged = True Then
      Response.Cookies(cookie_name).Expires = DateAdd("d", 1, Now())      ' 1 day Expiration of Cookie
      Response.Cookies(cookie_name)("expdate") =  DateAdd("d", 1, Now())  
      Response.Cookies(cookie_name)("email") = Request.Form("email")
      Response.Cookies(cookie_name)("mode") = "light"
      Response.Cookies(cookie_name)("user_trandate") = Now()
      Response.Cookies(cookie_name)("user_loggedin") = "true"
      Response.Redirect("dashboard.asp")
      Response.End() 
  Else 
      'Response.Write(errmsg)
      Response.Redirect("index.asp?status=error")
      Response.End() 
  End If 
Else 
  'Response.Write("invalid") 
  Response.Redirect("index.asp?status=invalid")
  Response.End() 
End If 

'[DEBUG]  
'Write(Request.Cookies(cookie_name)("shift"))
'Write(Request.Cookies(cookie_name)("user_name"))
'Write(Request.Cookies(cookie_name)("user_level"))
%>