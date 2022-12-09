Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports System
Imports System.Data
Imports System.Data.SqlClient

Public Class UploadFile
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Try
            'Upload files
            Dim postedFile As HttpPostedFile = context.Request.Files("files")
            If IsNothing(postedFile) Then
                context.Response.ContentType = "application/json"
                context.Response.Write("{ error: true }")
                context.Response.End()
            End If
            Dim module_code As String = context.Request.Form("module_code")
            Dim refno As String = context.Request.Form("refno")

            Dim savepath As String = My.Computer.FileSystem.ReadAllText(context.Server.MapPath("StorageLocation.config")) + module_code + "\" + refno + "\"
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

            'After files uploaded, we will record it now to the Database "ResourceFiles" table
            Using con As New SqlConnection(clsModule.MainDSN)
                con.Open()
                Using cmd = New SqlCommand("[usp_resourcefiles_create]", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@module_code", module_code)
                    cmd.Parameters.AddWithValue("@refno", refno)
                    cmd.Parameters.AddWithValue("@physical_path", actual_file)
                    cmd.Parameters.AddWithValue("@file_name", filename)
                    cmd.Parameters.AddWithValue("@file_ext", file_ext)
                    cmd.Parameters.AddWithValue("@upload_date", DateTime.Now)

                    cmd.ExecuteNonQuery()
                End Using
            End Using

            'Return empty json response {}, so that Kendo UI know that it was uploaded
            context.Response.ContentType = "application/json"
            context.Response.Write("{}")
        Catch ex As Exception
            context.Response.ContentType = "application/json"
            context.Response.Write(ex.ToString())
        End Try
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class