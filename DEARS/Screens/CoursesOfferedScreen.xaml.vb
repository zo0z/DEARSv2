Imports System.ComponentModel

Public Class CoursesOfferedScreen
    Implements IBaseScreen
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Private CoursesViewSource As CollectionViewSource
    Private OfferedCoursesViewSource As CollectionViewSource

    Private OfferedCoursesObservableCollection As ObservableEntityCollection(Of OfferedCourse)

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        LoadData("")
        QueryParamnsBox.DataContext = SharedState.GetSingleInstance
    End Sub
    Private Sub UserControl_UnLoaded(sender As Object, e As RoutedEventArgs)
        QueryParamnsBox.DataContext = Nothing
    End Sub

    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID

        If Not (PropertyName = "GradeID") Then
            Dim q_grades = From bt In DBContext.SemesterBatches.Include("Grades")
                       Where bt.SemesterId = SemesterID And bt.YearId = YearID
                       Select bt.Grade Distinct

            GradesViewSource.Source = q_grades.ToList()

            If q_grades.Count > 0 AndAlso Not (q_grades.ToList().Any(Function(gr) gr.Id = GradeID)) Then
                SharedState.GetSingleInstance.GradeID = q_grades.First().Id
                GradeID = SharedState.GetSingleInstance().GradeID
            End If
        End If

        Dim q_courses = From cr In DBContext.Courses
                        Select cr
        CoursesViewSource = CType(Me.FindResource("CoursesViewSource"), CollectionViewSource)
        CoursesViewSource.Source = New ObservableEntityCollection(Of Course)(DBContext, q_courses)


        OfferedCoursesViewSource = CType(Me.FindResource("OfferedCoursesViewSource"), CollectionViewSource)
        Dim q_offeredcourse = From oc In DBContext.OfferedCourses
                              Where oc.GradeId = GradeID And oc.YearId = YearID And oc.SemesterId = SemesterID
                              Select oc

        OfferedCoursesViewSource.Source = New ObservableEntityCollection(Of OfferedCourse)(DBContext, q_offeredcourse)
    End Sub
End Class