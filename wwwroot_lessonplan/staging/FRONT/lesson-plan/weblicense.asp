<!--#include file="includes/easyasp/easp.asp"-->
<%
	' Turn off error Handling
	'On Error Resume Next


  Dim licensePath
	Dim licenseKey
	
	licensePath = Server.MapPath("\accounting\weblicense.config")
	
  If (Easp.Fso.isExists(licensePath) = False) Then
	  Response.Write("Server has no license key yet. Please follow-up to Server Administrator")
		Response.End
	End If

  If Request.QueryString("keys") = "" Then
		Response.Write("No keys present")
		Response.End
	End If
	
	Dim reqKeys, str
	Dim scriptVersion
	
	scriptVersion = "version=1.0"
	reqKeys = Request.QueryString("keys")
	licenseKey = Easp.Fso.Read(licensePath)
		
	str = ""
	If licenseKey = reqKeys Then
	  str = str & "<html><body>"
		str = str & "<script src='libraries/jquery/jquery-2.2.4.min.js'></script>"
		str = str & "<script src='libraries/store.min.js'></script>"
		str = str & "<script>var weblicense = '"& reqKeys &"';</script>"
		str = str & "<script src='storelicense.js?"& scriptVersion &"'></script>"
		str = str & "</body></html>"
	Else
	  str = str & "<html><body>"
		str = str & "<script src='errorlicense.js?"& scriptVersion &"'></script>"
		str = str & "</body></html>"
	End If
	
	Response.Write(str)
	Response.End
  
		
	



	' Error Handler
	If Err.Number <> 0 Then
		 ' Error Occurred - Trap it
		 On Error Goto 0 ' Turn error handling back on for errors in your handling block
		 ' Code to cope with the error here
	End If
	On Error Goto 0 ' Reset error handling.
		
%>