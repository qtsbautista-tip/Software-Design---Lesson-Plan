Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports System
Imports System.Data
Imports System.Data.SqlClient

Public Class GetAllFiles
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Try
            'Upload files
            Dim module_code As String = context.Request.QueryString("module_code")
            Dim refno As String = context.Request.QueryString("refno")
            Dim action As String = "allfiles"
            Dim storage_location As String = My.Computer.FileSystem.ReadAllText(context.Server.MapPath("StorageLocation.config"))

            'Actual saving of files
            Dim actual_file As String = storage_location

            Dim fi As New IO.FileInfo(actual_file)
            Dim file_ext As String = fi.Extension
            Dim base64image As String = ""
            Dim dt As New DataTable
            'After files uploaded, we will record it now to the Database "ResourceFiles" table
            Using con As New SqlConnection(clsModule.MainDSN)
                con.Open()
                Using cmd = New SqlCommand("[usp_resourcefiles]", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    If module_code = "APP_RESOURCES" Then
                        cmd.Parameters.AddWithValue("@action", context.Request.QueryString("action"))
                    End If
                    cmd.Parameters.AddWithValue("@module_code", module_code)
                    cmd.Parameters.AddWithValue("@refno", refno)
                    dt.Load(cmd.ExecuteReader)
                End Using
            End Using

            ' Output to browser
            context.Response.ContentType = "application/json"
            context.Response.StatusCode = 200
            context.Response.Write(clsGlobal.DataTable2JSON(dt))

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