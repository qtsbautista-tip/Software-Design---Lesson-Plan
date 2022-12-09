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

Public Class Sysconfig
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        If context.Request.QueryString("action") = Nothing Then
            context.Response.Write("missing_parameter")
            context.Response.End()
        End If

        Dim action As String = context.Request.QueryString("action")

        If action = "HasRecord" Then
            HasRecord(context)
        ElseIf action = "ALL" Then
            SearchALL(context)

        ElseIf action = "Save" Then
            SaveData(context)
        ElseIf action = "Delete" Then
            TruncateData(context)
        End If

    End Sub

    Public Sub HasRecord(ByVal context As HttpContext)
        Using con As New SqlConnection(clsModule.MainDSN)
            con.Open()
            Using cmd As New SqlCommand("SELECT count(*) FROM Sysconfig", con)
                cmd.CommandType = CommandType.Text
                Dim cnt As Int32 = Convert.ToInt32(cmd.ExecuteScalar)
                If cnt > 0 Then
                    context.Response.Write("yes")
                Else
                    context.Response.Write("no")
                End If
            End Using
        End Using

        context.Response.End()
    End Sub

    Public Sub SearchALL(ByVal context As HttpContext)
        Dim dt As New DataTable
        Using con As New SqlConnection(clsModule.MainDSN)
            con.Open()
            Using cmd As New SqlCommand("SELECT * FROM Sysconfig", con)
                cmd.CommandType = CommandType.Text

                dt.Load(cmd.ExecuteReader())

                For i = 0 To (dt.Columns.Count - 1)
                    dt.Columns(i).ColumnName = dt.Columns(i).ColumnName.ToUpper()
                Next
            End Using
        End Using
        context.Response.Write(clsGlobal.DataTable2JSON(dt))
        context.Response.End()
    End Sub

    Public Sub SaveData(ByVal context As HttpContext)
        Using con As New SqlConnection(clsModule.MainDSN)
            con.Open()
            Using cmd As New SqlCommand("[usp_sysconfig_create]", con)
                cmd.CommandType = CommandType.StoredProcedure
                'params
                cmd.Parameters.AddWithValue("@Company_Name", "")
                cmd.Parameters.AddWithValue("@Company_Address", "")
                cmd.Parameters.AddWithValue("@Company_Address1", "")
                cmd.Parameters.AddWithValue("@Company_Tel1", "")
                cmd.Parameters.AddWithValue("@EmailAdd", "")
                cmd.Parameters.AddWithValue("@Company_Fax", "")
                cmd.Parameters.AddWithValue("@Photo", "")
                cmd.ExecuteNonQuery()

                context.Response.Write("saved")
                context.Response.End()
            End Using
        End Using
    End Sub

    Public Sub TruncateData(ByVal context As HttpContext)
        Using con As New SqlConnection(clsModule.MainDSN)
            con.Open()
            Using cmd As New SqlCommand("TRUNCATE TABLE Sysconfig", con)
                cmd.CommandType = CommandType.Text
                cmd.ExecuteNonQuery()
                context.Response.Write("truncated")
                context.Response.End()
            End Using
        End Using
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class