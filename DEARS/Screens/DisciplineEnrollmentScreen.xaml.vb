Public Class DisciplineEnrollmentScreen
    Implements IBaseScreen

    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        DisciplineEnrollmentsViewSource = CType(Me.FindResource("DisciplineEnrollmentsViewSource"), CollectionViewSource)
        LoadData("")
        QueryParamsBox.DataContext = SharedState.GetSingleInstance()
    End Sub
    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamsBox.DataContext = Nothing
    End Sub
    Private GradesViewSource As CollectionViewSource
    Private DisciplineEnrollmentsViewSource As CollectionViewSource

    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID

        If Not (PropertyName = "GradeID" Or PropertyName = "DisciplineID") Then
            Dim q_grades = From bt In DBContext.SemesterBatches.Include("Grade").Include("OfferedDisciplines")
                       Where bt.SemesterId = SemesterID And bt.YearId = YearID
                       Select bt


            GradesViewSource.Source = q_grades.ToList()

            If q_grades.Count > 0 AndAlso Not (q_grades.ToList().Any(Function(gr) gr.GradeId = GradeID)) Then
                SharedState.GetSingleInstance.GradeID = q_grades.First().GradeId
                GradeID = SharedState.GetSingleInstance().GradeID
            End If
        End If

        GradeID = SharedState.GetSingleInstance().GradeID

        Dim q_discipenr = From denr In DBContext.SemesterBatchEnrollments.Include("Student")
                          Where denr.YearId = YearID And denr.SemesterId = SemesterID And denr.GradeId = GradeID
                          Select denr


        Dim q_nodiscp = From senr In DBContext.BatchEnrollments.Include("Student")
                        Where senr.YearId = YearID And senr.GradeId = GradeID And Not (senr.SemesterBatchEnrollments.Any(Function(s) s.SemesterId = SemesterID))
                        Select senr

        Dim DiscpEnrCollection As New ObservableEntityCollection(Of SemesterBatchEnrollment)(DBContext, q_discipenr)

        For Each benr In q_nodiscp.ToList()
            DiscpEnrCollection.Add(New SemesterBatchEnrollment() With {.Student = benr.Student, .YearId = YearID, .GradeId = GradeID, .SemesterId = SemesterID})
        Next

        DisciplineEnrollmentsViewSource.Source = DiscpEnrCollection
    End Sub
End Class
