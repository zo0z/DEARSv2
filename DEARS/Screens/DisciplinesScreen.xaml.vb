Public Class DisciplinesScreen
    Implements IBaseScreen

    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private DisciplinesViewSource As CollectionViewSource

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        DisciplinesViewSource = CType(Me.FindResource("DisciplinesViewSource"), CollectionViewSource)

        Dim grades = (From gr In DBContext.Disciplines
                     Select gr)

        DisciplinesViewSource.Source = New ObservableEntityCollection(Of Discipline)(DBContext, grades)
    End Sub

End Class
