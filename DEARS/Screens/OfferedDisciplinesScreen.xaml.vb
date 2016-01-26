Public Class OfferedDisciplinesScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property


    Private GradesViewSource As CollectionViewSource
    Private DisciplinesViewSource As CollectionViewSource
    Private OfferedDisciplinesViewSource As CollectionViewSource


    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance.YearID
        Dim SemesterID As Integer = SharedState.GetSingleInstance.SemesterID

        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        DisciplinesViewSource = CType(Me.FindResource("DisciplinesViewSource"), CollectionViewSource)
        OfferedDisciplinesViewSource = CType(Me.FindResource("OfferedDisciplinesViewSource"), CollectionViewSource)

        GradesViewSource.Source = New ObservableEntityCollection(Of Grade)(DBContext, DBContext.Grades)
        DisciplinesViewSource.Source = New ObservableEntityCollection(Of Discipline)(DBContext, DBContext.Disciplines)

        Dim q_offereddisciplines = From od In DBContext.OfferedDisciplines
                                   Where od.YearId = YearID And od.SemesterId = SemesterID
                                   Select od

        OfferedDisciplinesViewSource.Source = New ObservableEntityCollection(Of OfferedDiscipline)(DBContext, q_offereddisciplines)
    End Sub
End Class
