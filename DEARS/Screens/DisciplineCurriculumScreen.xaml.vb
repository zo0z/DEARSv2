Imports System.ComponentModel

Public Class DisciplineCurriculumScreen
    Implements IBaseScreen
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Private CoursesViewSource As CollectionViewSource
    Private DisciplineCurriculumViewSource As CollectionViewSource

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        CoursesViewSource = CType(Me.FindResource("CoursesViewSource"), CollectionViewSource)
        DisciplineCurriculumViewSource = CType(Me.FindResource("DisciplineCurriculumViewSource"), CollectionViewSource)

        LoadData("")
        QueryParamnsBox.DataContext = SharedState.GetSingleInstance
    End Sub

    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID
        Dim DisciplineID As Integer = SharedState.GetSingleInstance().DisciplineID

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

        If Not (PropertyName = "DisciplineID") Then
            Dim q_courses = From cr In DBContext.OfferedCourses.Include("Course")
                            Where cr.YearId = YearID And cr.GradeId = GradeID And cr.SemesterId = SemesterID
                            Select cr.Course

            CoursesViewSource.Source = q_courses.ToList()
        End If


        Dim q_disciplinecurr = From dc In DBContext.CourseDisciplines
                               Where dc.YearId = YearID And dc.GradeId = GradeID And dc.SemesterId = SemesterID And dc.DisciplineId = DisciplineID
                               Select dc


        DisciplineCurriculumViewSource.Source = New ObservableEntityCollection(Of CourseDiscipline)(DBContext, q_disciplinecurr)
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamnsBox.DataContext = Nothing
    End Sub
End Class