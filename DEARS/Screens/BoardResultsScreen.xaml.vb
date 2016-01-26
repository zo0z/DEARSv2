Public Class BoardResultsScreen
    Implements IBaseScreen
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property
    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData

    End Sub
End Class
