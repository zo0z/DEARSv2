Imports DetailedResultsImporter

Public Class MeetingResultsScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Private GPAViewSource As CollectionViewSource
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
        Dim q_gpas = From gpw In SharedState.DBContext.GPAwRecomms.Include("Student")
                     Where gpw.YearId = YearID And gpw.GradeId = GradeID And
                     gpw.BatchEnrollment.SemesterBatchEnrollments.Any(Function(s) s.DisciplineId = DisciplineID)
                     Select gpw

        ' Students with no Records
        Dim q_benr = From benr In SharedState.DBContext.BatchEnrollments.Include("Student")
                     Where benr.YearId = YearID And benr.GradeId = GradeID And
                     benr.SemesterBatchEnrollments.Any(Function(s) s.DisciplineId = DisciplineID) And benr.GPAwRecomm Is Nothing
                     Select benr

        Dim StudentsCollection As New ObservableEntityCollection(Of GPAwRecomm)(DBContext, q_gpas.ToList())

        For Each cenr In q_benr.ToList()
            StudentsCollection.Add(New GPAwRecomm() With {.YearId = YearID, .GradeId = GradeID, .StudentId = cenr.StudentId,
                                                          .Student = cenr.Student})
        Next

        GPAViewSource.Source = StudentsCollection
    End Sub

    Private Sub ImportButton_Click(sender As Object, e As RoutedEventArgs)
        ' We need to start the import process
        Dim importerDialog As New ImporterDialog()
        importerDialog.ShowDialog()

    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        GPAViewSource = CType(Me.FindResource("GPAViewSource"), CollectionViewSource)
        LoadData("")
        QueryParamsBox.DataContext = SharedState.GetSingleInstance
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamsBox.DataContext = Nothing
    End Sub

    Private Sub GenerateButton_Click(sender As Object, e As RoutedEventArgs)
        Dim flbrwsr As New Forms.SaveFileDialog()
        flbrwsr.Filter = "Excel OpenXML Document (*.xlsx) |*.xlsx"
        If flbrwsr.ShowDialog() = Forms.DialogResult.OK Then
            Dim resIssy As New ResultsIssue(flbrwsr.FileName, SharedState.GetSingleInstance.YearID, SharedState.GetSingleInstance.SemesterID = 1, SharedState.GetSingleInstance.GradeID)
            Dim YearID As Integer = SharedState.GetSingleInstance().YearID
            Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
            Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID
            Dim DisciplineID As Integer = SharedState.GetSingleInstance.DisciplineID

            Dim bD = SharedState.GetSingleInstance.AllDisciplines
            Dim discs = (From d In DBContext.OfferedDisciplines
                         Where d.YearId = YearID And d.GradeId = GradeID And d.SemesterId = SemesterID And (bD Or (d.DisciplineId = DisciplineID))
                         Select d.Discipline).ToList()
            For Each disc In discs
                resIssy.AddDisciplineResults(disc.Id, False)
            Next
            resIssy.Save()
            MsgBox("Done!")
        Else
            Return
        End If
    End Sub
End Class
