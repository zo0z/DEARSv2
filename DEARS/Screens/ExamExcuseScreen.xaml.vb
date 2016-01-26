Public Class ExamExcuseScreen
    Implements IBaseScreen

    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Private StudentsViewSource As CollectionViewSource

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID


        If Not (PropertyName = "GradeID" Or PropertyName = "CourseID" Or PropertyName = "DisciplineID") Then
            Dim q_grades = From bt In DBContext.SemesterBatches.Include("Grade").Include("OfferedCourses")
                       Where bt.SemesterId = SemesterID And bt.YearId = YearID
                       Select bt


            GradesViewSource.Source = New ObservableEntityCollection(Of SemesterBatch)(DBContext, q_grades)

            If q_grades.Count > 0 AndAlso Not (q_grades.ToList().Any(Function(gr) gr.GradeId = GradeID)) Then
                SharedState.GetSingleInstance.GradeID = q_grades.First().GradeId
                GradeID = SharedState.GetSingleInstance().GradeID
            End If
        End If

        Dim CourseID As Integer = SharedState.GetSingleInstance().CourseID
        Dim DisciplineID As Integer = SharedState.GetSingleInstance().DisciplineID

        ' Students with records already
        Dim q_cwmarks = From cwm In DBContext.MarksExamCWs
                        Where cwm.YearId = YearID And cwm.GradeId = GradeID And cwm.CourseId = CourseID And cwm.SemesterId = SemesterID And (cwm.Present = False)
                        Select cwm

       
        Dim StudentsCollection As New ObservableEntityCollection(Of MarksExamCW)(DBContext, q_cwmarks)

        StudentsViewSource.Source = StudentsCollection
        StudentsViewSource.View.Filter = AddressOf DisciplineFilterFunction
    End Sub
    Private Sub AllDisciplinescheckBox_Checked(sender As Object, e As RoutedEventArgs)
        StudentsViewSource.View.Filter = AddressOf DisciplineFilterFunction
    End Sub

    Private Sub AllDisciplinescheckBox_Unchecked(sender As Object, e As RoutedEventArgs)
        StudentsViewSource.View.Filter = AddressOf DisciplineFilterFunction
    End Sub

    Private Function DisciplineFilterFunction(s As Object) As Boolean
        Dim item = CType(s, MarksExamCW)
        If AllDisciplinescheckBox.IsChecked Then
            Return True
        Else
            Return (SharedState.GetSingleInstance.DisciplineID = item.CourseEnrollment.SemesterBatchEnrollment.DisciplineId)
        End If
    End Function

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        StudentsViewSource = CType(Me.FindResource("StudentsViewSource"), CollectionViewSource)
        LoadData("")
        QueryParamsBox.DataContext = SharedState.GetSingleInstance
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamsBox.DataContext = Nothing
    End Sub
End Class
