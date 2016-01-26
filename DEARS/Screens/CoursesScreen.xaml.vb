Public Class CoursesScreen
    Implements IBaseScreen
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private CoursesViewSource As CollectionViewSource

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        CoursesViewSource = CType(Me.FindResource("CoursesViewSource"), CollectionViewSource)

        Dim q_courses = (From gr In DBContext.Courses
                     Select gr)

        CoursesViewSource.Source = New ObservableEntityCollection(Of Course)(DBContext, q_courses)
    End Sub
End Class
