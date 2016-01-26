Public Class AcademicClassesScreen
    Implements IBaseScreen
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)

        Dim grades = From gr In DBContext.Grades
                     Select gr

        GradesViewSource.Source = New ObservableEntityCollection(Of Grade)(DBContext, grades)
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)

        LoadData("")

    End Sub

End Class
