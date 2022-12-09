On Error Resume Next
Function readFromRegistry (strRegistryKey, strDefault)
    Dim WSHShell, value



    On Error Resume Next
    Set WSHShell = CreateObject ("WScript.Shell")
    value = WSHShell.RegRead (strRegistryKey)

    if err.number <> 0 then
        readFromRegistry= strDefault
    else
        readFromRegistry=value
    end if

    set WSHShell = nothing
End Function

Function OpenWithChrome(strURL)
    Dim strChrome
    Dim WShellChrome



    strChrome = readFromRegistry ( "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe\Path", "") 
    if (strChrome = "") then
        strChrome = "chrome.exe"
    else
        strChrome = strChrome & "\chrome.exe"
    end if
    Set WShellChrome = CreateObject("WScript.Shell")
    strChrome = """" & strChrome & """" & " " & strURL
    WShellChrome.Run strChrome, 1, false
End Function

Dim strComputer
Dim mddSerialNum
Dim hddSerialNum


strComputer = "."


Set objWMIService = GetObject("winmgmts:\\" & strComputer & "\root\cimv2")
Set colItems = objWMIService.ExecQuery("Select * from Win32_BaseBoard",,48)
For Each objItem in colItems
    mddSerialNum = objItem.SerialNumber
Next

Set colItems = objWMIService.ExecQuery("select * from Win32_DiskDrive",,48)
For Each objItem in colItems
    hddSerialNum = objItem.SerialNumber
Next

OpenWithChrome "http://ehotel.syscomsystems.com/accounting/weblicense.asp?keys=" & mddSerialNum & "-" & hddSerialNum
