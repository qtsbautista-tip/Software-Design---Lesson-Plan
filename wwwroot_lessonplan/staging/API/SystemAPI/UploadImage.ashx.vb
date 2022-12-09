Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports System
Imports System.Data
Imports System.Data.SqlClient

Public Class UploadImage
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Try
            'Upload files
            Dim module_code As String = context.Request.Form("module_code")
            Dim refno As String = context.Request.Form("refno")
            Dim postedFile As HttpPostedFile = context.Request.Files("files")
            Dim savepath As String = My.Computer.FileSystem.ReadAllText(context.Server.MapPath("ImageLocation.config")) + module_code + "\" + refno + "\"
            Dim filename As String = postedFile.FileName
            Dim type As New IO.FileInfo(filename)
            filename = refno & type.Extension
            If Not Directory.Exists(savepath) Then Directory.CreateDirectory(savepath)

            'Actual saving of files
            Dim actual_file As String = savepath & filename
            If File.Exists(actual_file) Then
                File.Delete(actual_file)
            End If
            postedFile.SaveAs(actual_file)

            Dim fi As New IO.FileInfo(actual_file)
            Dim file_ext As String = fi.Extension
            Dim base64image As String = ConvertFileToBase64(actual_file)

            'Delete existing filename on the Database
            Using con As New SqlConnection(clsModule.MainDSN)
                con.Open()
                Using cmd = New SqlCommand("DELETE FROM ResourceImages WHERE module_code = @module_code AND refno = @refno", con)
                    cmd.CommandType = CommandType.Text
                    cmd.Parameters.AddWithValue("@module_code", module_code)
                    cmd.Parameters.AddWithValue("@refno", refno)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            'After files uploaded, we will record it now to the Database "ResourceImages" table
            Using con As New SqlConnection(clsModule.MainDSN)
                con.Open()
                Using cmd = New SqlCommand("[usp_resourceimages_create]", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@module_code", module_code)
                    cmd.Parameters.AddWithValue("@refno", refno)
                    cmd.Parameters.AddWithValue("@physical_path", actual_file)
                    cmd.Parameters.AddWithValue("@base64image", base64image)
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
            context.Response.Write("error_uploadimage : " + ex.ToString())
            'context.Response.Write(ex.ToString())
        End Try
    End Sub

    Public Function ConvertFileToBase64(ByVal fileName As String) As String
        Return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName))
    End Function

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class