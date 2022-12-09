Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports System
Imports System.Data
Imports System.Data.SqlClient

Public Class GetImage
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Try
            'Upload files
            Dim module_code As String = context.Request.QueryString("module_code")
            Dim refno As String = context.Request.QueryString("refno")
            Dim action As String = "base64"
            Dim storage_location As String = My.Computer.FileSystem.ReadAllText(context.Server.MapPath("ImageLocation.config"))
            If context.Request.QueryString("action") = Nothing Then
                action = context.Request.QueryString("action")
            End If

            'Actual saving of files
            Dim actual_file As String = storage_location

            Dim fi As New IO.FileInfo(actual_file)
            Dim file_ext As String = fi.Extension
            Dim base64image As String = ""
            Dim dt As New DataTable

            'After files uploaded, we will record it now to the Database "ResourceFiles" table
            Using con As New SqlConnection(clsModule.MainDSN)
                con.Open()
                Using cmd = New SqlCommand("[usp_resourceimages]", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@module_code", module_code)
                    cmd.Parameters.AddWithValue("@refno", refno)
                    cmd.Parameters.AddWithValue("@action", action)
                    dt.Load(cmd.ExecuteReader)
                        If dt.Rows.Count > 0 Then
                            actual_file = dt.Rows(0).Item(0)
                            If File.Exists(actual_file) Then
                                ImagefileToStream(context, actual_file)
                            Else
                                ImagefileToStream(context, context.Server.MapPath("no-image.png"))
                            End If
                        Else
                            ImagefileToStream(context, context.Server.MapPath("no-image.png"))
                        End If
                End Using
            End Using

        Catch ex As Exception
            context.Response.ContentType = "application/json"
            context.Response.Write("error_getimage")
            'context.Response.Write(ex.ToString())
        End Try

    End Sub
    Public Sub ImagefileToStream(ByVal context As HttpContext, ByVal actual_file As String)
        Dim contentType As String = String.Empty
        Dim extension As String = actual_file.Substring((actual_file.Length - 4), 4).ToLower()

        Select Case extension
            Case ".png"
                contentType = "image/png"
            Case Else
        End Select

        context.Response.ContentType = contentType
        context.Response.TransmitFile(actual_file)
        'context.Response.Clear()
        'context.Response.ClearContent()
        'Using reader As New FileStream(actual_file, FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
        '    Dim buffer As Byte() = New Byte(reader.Length - 1) {}
        '    reader.Read(buffer, 0, CInt(reader.Length))
        '    context.Response.Write(Convert.ToBase64String(buffer))
        'End Using
        'context.Response.Flush()
        'context.Response.End()
    End Sub

    Public Function Base64ToImage(ByVal base64string As String) As System.Drawing.Image
        'Setup image and get data stream together
        Dim img As System.Drawing.Image
        Dim MS As System.IO.MemoryStream = New System.IO.MemoryStream
        Dim b64 As String = base64string.Replace(" ", "+")
        Dim b() As Byte

        'Converts the base64 encoded msg to image data
        b = Convert.FromBase64String(b64)
        MS = New System.IO.MemoryStream(b)

        'creates image
        img = System.Drawing.Image.FromStream(MS)

        Return img
    End Function
    Public Function ConvertFileToBase64(ByVal fileName As String) As String
        Return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName))
    End Function
    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class