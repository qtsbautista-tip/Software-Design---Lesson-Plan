Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports Newtonsoft.Json
Imports System.Web.Script.Serialization
Imports System.Net

Public Class ImportData
    Implements System.Web.IHttpHandler

    Private savepath As String = Path.GetTempPath() & "\ImportDataTemp\"

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Try
            If context.Request.QueryString("upload") <> Nothing Then
                UploadExcelData(context)
            ElseIf context.Request.QueryString("validate") <> Nothing Then
                ValidateExcelData(context)
            ElseIf context.Request.QueryString("process") <> Nothing Then
                ProcessExcelData(context)
            End If

        Catch ex As Exception
            context.Response.ContentType = "application/json"
            context.Response.Write("error_uploadfile")
            'context.Response.Write(ex.ToString())
        End Try

    End Sub

    Public Sub ProcessExcelData(ByVal context As HttpContext)
        If context.Request.Form("json_excel") <> Nothing And context.Request.Form("uploaded_filename") <> Nothing Then
            Try
                Dim json_excel As String = context.Request.Form("json_excel")
                Dim uploaded_filename As String = context.Request.Form("uploaded_filename")
                Dim uploaded_temp As String = savepath & uploaded_filename
                If File.Exists(uploaded_temp) Then
                    Dim dts As New DataSet
                    Using con As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & uploaded_temp & ";Extended Properties=""Excel 12.0 Xml;HDR=YES"";")
                        Dim cmd As New OleDbDataAdapter
                        cmd = New OleDbDataAdapter("select * from [Template$]", con)
                        cmd.Fill(dts)
                    End Using

                    'Get the correct format for that Master file
                    Dim webClient As New WebClient
                    Dim JSONString As String = webClient.DownloadString(json_excel)

                    Dim texcel As Object = New JavaScriptSerializer().Deserialize(Of Object)(JSONString)
                    Dim sheets As Object = texcel("sheets")

                    Dim sTargetTable As String = sheets(0)("target") 'target table where will the import data will be inserted/appended
                    Dim sValue As String = ""
                    Dim sFld As String = ""


                    Dim fldCount As Integer = dts.Tables(0).Columns.Count - 1
                    Dim lstValues As New List(Of String)
                    Dim lstSQL As New List(Of String)

                    'Compose target fields
                    Dim colNames(fldCount) As String
                    Dim i As Integer = 0
                    For Each column As Object In sheets(0)("rows")(0)("cells")
                        sValue = column("value")
                        sFld = column("fld")
                        colNames(i) = sFld
                        i += 1
                    Next
                    Dim strFields As String = String.Join(",", colNames)

                    'Compose values
                    Dim colValues(fldCount) As String
                    For Each row As DataRow In dts.Tables(0).Rows
                        ReDim colValues(fldCount)
                        For i = 0 To fldCount
                            If Not row.Item(i).GetType() Is GetType(DBNull) Then
                                colValues(i) = "'" + CStr(row.Item(i)) + "'"
                            Else
                                colValues(i) = "''"
                            End If
                        Next
                        lstValues.Add(String.Join(",", colValues))
                    Next row

                    Dim strValues As String = ""
                    Dim strSQL As String = ""
                    If lstValues.Count > 0 Then
                        For i = 0 To lstValues.Count - 1
                            strValues = lstValues(i)
                            'Compose SQL Statement for Insertion
                            strSQL = String.Format("INSERT INTO {0}({1}) VALUES ({2})", sTargetTable, strFields, strValues)
                            lstSQL.Add(strSQL)
                        Next
                    End If

                    Dim execute_destination As String = ""


                    'Execute statement
                    If lstSQL.Count > 0 Then
                        Using con As New SqlConnection(clsModule.ConnString("Acctg"))
                            con.Open()

                            'check the method on how server will treat the import request
                            If context.Request.Form("import_method") <> Nothing Then
                                'import_method:
                                '   append - means will append the import data to the existing table records
                                '   delete - means will delete first all existing table records, then import new data
                                Dim import_method As String = context.Request.Form("import_method")
                                If import_method = "delete" Then
                                    Using cmddelete As New SqlCommand(String.Format("TRUNCATE TABLE {0}", sTargetTable), con)
                                        cmddelete.ExecuteNonQuery()
                                    End Using
                                End If
                            End If

                            For i = 0 To lstSQL.Count - 1
                                '[debug]
                                'context.Response.Write(lstSQL(i) & vbCrLf)
                                strSQL = lstSQL(i)
                                Using cmd As New SqlCommand(strSQL, con)
                                    cmd.CommandType = CommandType.Text
                                    cmd.ExecuteNonQuery()
                                End Using
                            Next
                        End Using
                        context.Response.Write("Success_Import")
                    Else
                        context.Response.Write("No_Data_To_Import")
                    End If

                End If
            Catch ex As Exception
                context.Response.Write(ex.Message)
            End Try
        End If
    End Sub

    Public Sub ValidateExcelData(ByVal context As HttpContext)
        If context.Request.Form("json_excel") <> Nothing And context.Request.Form("uploaded_filename") <> Nothing Then
            Try
                Dim json_excel As String = context.Request.Form("json_excel")
                Dim uploaded_filename As String = context.Request.Form("uploaded_filename")
                Dim uploaded_temp As String = savepath & uploaded_filename
                If File.Exists(uploaded_temp) Then
                    Dim dts As New DataSet
                    Using con As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & uploaded_temp & ";Extended Properties=""Excel 12.0 Xml;HDR=YES"";")
                        Dim cmd As New OleDbDataAdapter
                        cmd = New OleDbDataAdapter("select * from [Template$]", con)
                        cmd.Fill(dts)
                    End Using

                    'Get the correct format for that Master file
                    Dim webClient As New WebClient
                    Dim JSONString As String = webClient.DownloadString(json_excel)

                    Dim texcel As Object = New JavaScriptSerializer().Deserialize(Of Object)(JSONString)
                    Dim sheets As Object = texcel("sheets")

                    'context.Response.Write(clsGlobal.VarDump(sheets(0)("rows")(0)("cells")))
                    'context.Response.StatusCode = 200
                    'context.Response.StatusDescription = "Success"
                    'context.Response.End()


                    'Check header if it match with JSON import structure
                    Dim colNames(dts.Tables(0).Columns.Count) As String
                    Dim i As Integer = 0
                    For Each column As DataColumn In (dts.Tables(0).Columns)
                        colNames(i) = column.ColumnName
                        i += 1
                    Next

                    Dim isValidFormat As Boolean = True
                    Dim sValue As String = ""
                    Dim sFld As String = ""
                    For Each row As Object In sheets(0)("rows")(0)("cells")
                        'context.Response.Write(clsGlobal.VarDump(row))
                        sValue = row("value")
                        sFld = row("fld")

                        'Check each field by looping it
                        For i = 0 To (colNames.Length - 1)
                            If UCase(sValue) = UCase(colNames(i)) Then
                                isValidFormat = True
                                Exit For
                            Else
                                isValidFormat = False
                            End If
                        Next

                        If isValidFormat = False Then
                            Exit For
                        End If
                    Next


                    If isValidFormat Then
                        'Good format!
                        context.Response.Write("Valid_Format")
                    Else
                        'Wrong format!
                        context.Response.Write("Invalid_Format")
                    End If
                End If
            Catch ex As Exception
                context.Response.Write(ex.Message)
            End Try
        End If
    End Sub

    Public Sub UploadExcelData(ByVal context As HttpContext)
        'Upload files
        Dim postedFile As HttpPostedFile = context.Request.Files("files")
        If IsNothing(postedFile) Then
            context.Response.ContentType = "application/json"
            context.Response.Write("{ error: true }")
            context.Response.End()
        End If
        Dim module_code As String = context.Request.Form("module_code")
        Dim program_code As String = context.Request.Form("program_code")

        Dim filename As String = postedFile.FileName
        If Not Directory.Exists(savepath) Then Directory.CreateDirectory(savepath)

        'Actual saving of files
        Dim actual_file As String = savepath & filename
        If File.Exists(actual_file) Then
            File.Delete(actual_file)
        End If
        postedFile.SaveAs(actual_file)

        Dim fi As New IO.FileInfo(actual_file)
        Dim file_ext As String = fi.Extension


        'Return empty json response {}, so that Kendo UI know that it was uploaded
        context.Response.ContentType = "application/json"
        context.Response.Write("{}")
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class