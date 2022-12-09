Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports System
Imports System.Data
Imports System.Data.SqlClient

Public Class DeleteFile
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Try
            'Upload files
            Dim module_code As String = context.Request.QueryString("module_code")
            Dim refno As String = context.Request.QueryString("refno")
            Dim file_name As String = context.Request.QueryString("file_name")
            Dim storage_location As String = My.Computer.FileSystem.ReadAllText(context.Server.MapPath("StorageLocation.config"))

            'Actual saving of files
            Dim actual_file As String = storage_location
            Dim rec_id As Integer = 0

            Dim fi As New IO.FileInfo(actual_file)
            Dim file_ext As String = fi.Extension
            Dim base64image As String = ""
            Dim dt As New DataTable

            'After files uploaded, we will record it now to the Database "ResourceFiles" table
            Using con As New SqlConnection(clsModule.MainDSN)
                con.Open()
                Using cmd = New SqlCommand("[usp_resourcefiles]", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@module_code", module_code)
                    cmd.Parameters.AddWithValue("@refno", refno)
                    cmd.Parameters.AddWithValue("@file_name", file_name)
                    cmd.Parameters.AddWithValue("@action", "selected")
                    dt.Load(cmd.ExecuteReader)
                End Using

                ' actual deletion
                If dt.Rows.Count > 0 Then
                    actual_file = dt.Rows(0).Item("physical_path")
                    rec_id = Convert.ToInt32(dt.Rows(0).Item("id"))
                    If File.Exists(actual_file) Then
                        'delete actual file
                        File.Delete(actual_file)
                    End If
                    'delete on the database
                    Using cmd1 = New SqlCommand("[usp_resourcefiles_delete]", con)
                        cmd1.CommandType = CommandType.StoredProcedure
                        cmd1.Parameters.AddWithValue("@id", rec_id)
                        cmd1.ExecuteNonQuery()
                    End Using
                    context.Response.ContentType = "application/json"
                    context.Response.Write("deleted")
                End If
            End Using

        Catch ex As Exception
            context.Response.ContentType = "application/json"
            context.Response.Write("error_getimage")
            'context.Response.Write(ex.ToString())
        End Try

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class