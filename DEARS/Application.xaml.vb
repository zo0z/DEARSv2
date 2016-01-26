Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As Windows.Threading.DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        MsgBox(FlattenOutException(e.Exception))
        e.Handled = True
    End Sub

    Public Shared Function FlattenOutException(ex As Exception) As String
        Dim msg As String = ex.Message
        While ex.InnerException IsNot Nothing
            msg = msg + vbCrLf + vbTab + ex.InnerException.Message
            ex = ex.InnerException
        End While
        Return msg
    End Function
End Class
