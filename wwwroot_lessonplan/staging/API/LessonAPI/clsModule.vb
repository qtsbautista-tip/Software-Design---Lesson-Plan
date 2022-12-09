Public Class clsModule

    Public Sub New()
    End Sub

    Public Shared Function ApplicationName() As String
        Dim s As String = ConfigurationManager.AppSettings("AppName").ToString()
        Return s
    End Function

    Public Shared Function MainDSN() As String
        Dim s As String = ConfigurationManager.ConnectionStrings("Main_Connectionstring").ToString()
        Return s
    End Function
    Public Shared Function Warehousesql() As String
        Dim s As String = ConfigurationManager.ConnectionStrings("Warehouse_Connectionstring").ToString()
        Return s
    End Function

    Public Shared Function ConnString(Optional ByVal s As String = "None") As String
        If s = "None" Then
            Throw New Exception("No database string specified")
        End If
        Return ConfigurationManager.ConnectionStrings(s & "_Connectionstring").ToString()
    End Function

End Class
