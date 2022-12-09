<%
' File: global_variables.asp


' Declaration
Dim lang
Dim lang_asp
Dim FooterIndicator
FooterIndicator = True

Dim lesson_api_url
Dim lesson_rpt_url
Dim system_api_url
Dim files_upload_url
Dim images_upload_url

Dim setting_logo_url
Dim setting_favicon_logo
Dim program_name
Dim program_version
Dim program_provider
Dim setting_defaultbg_url

Dim program_nav_icon
Dim program_owner
Dim program_owner_address
Dim program_owner_telno
Dim program_owner_website
Dim program_owner_email

Dim program_registeredto
Dim program_registeredto_address
Dim program_registeredto_telno
Dim program_registeredto_website
dim program_registeredto_email


Dim nocache_time
Dim main_url
Dim main_program_folder

' Declaration for User Session
Dim email
Dim user_isloggedin
Dim user_trandate
Dim cookie_name
Dim active_user_limit
Dim program_mouseactivitylimit
Dim mode
Dim expdate

' Initialization
program_name     = "Lesson Plan"
program_version  = "06.10.2021"
program_provider = "Taylor Inc."
setting_defaultbg_url        = "resources/calculator-385506_1920_v2.jpg"
active_user_limit = 3   'Set here the Allowable Active User ... Configure number of users to logged-in'
program_mouseactivitylimit = 600      ' 60 secs*2 * 5mins

main_url                  = GetDomain() + ":91"
main_program_folder       = ""

setting_favicon_logo      = "resources/logo.jpg"

program_nav_icon             = "resources/logo.jpg"
'program_nav_icon             = ""
program_owner                = "Taylor Inc."
program_owner_address        = ""
program_owner_telno          = ""
program_owner_website        = ""
program_owner_email          = ""

setting_logo_url             = "resources/logo.jpg"
setting_defaultbg_url        = "resources/calculator-385506_1920_v2.jpg"
program_registeredto         = "Tella"
program_registeredto_address = "51 T. Santiago, Canumay West. Valenzuela City"
program_registeredto_telno   = "(045) 000-0000"
program_registeredto_website = "primecanadian.ph"
program_registeredto_email   = "renzver30@gmail.com"

  ' Cookie parent name
cookie_name      = LCASE(program_name)
cookie_name      = REPLACE(cookie_name, " ", "_")



If Request.Cookies(cookie_name) = "" Then
  email        = ""
  user_trandate    = ""
  user_isloggedin  = ""
  mode  = ""
  expdate  = ""
Else
  email        = Request.Cookies(cookie_name)("email")
  user_trandate    = Request.Cookies(cookie_name)("user_trandate")
  user_isloggedin  = Request.Cookies(cookie_name)("user_loggedin")
  mode  = Request.Cookies(cookie_name)("mode")
  expdate  = Request.Cookies(cookie_name)("expdate")
  Response.Cookies(cookie_name).Expires = expdate
End If


'API'
system_api_url   = main_url & "/systemapi"
lesson_api_url    = main_url & "/lessonapi"
'REPORT'
lesson_rpt_url    = main_url & "/lessonrpt"
	
'RESOURCES (Files/Images)'
files_upload_url     = main_url & "/client_uploads/files/"
images_upload_url     = main_url & "/client_uploads/images/"
  
' Embed this to resources (css/js) that you dont want to cache by the browser
Randomize
nocache_time     = Rnd(100)
%>