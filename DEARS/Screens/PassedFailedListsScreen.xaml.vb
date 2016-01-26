Public Class PassedFailedListsScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property


    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData

    End Sub
End Class
