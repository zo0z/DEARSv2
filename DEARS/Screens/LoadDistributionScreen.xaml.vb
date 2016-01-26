Imports System.ComponentModel

Public Class LoadDistributionScreen
    Implements IBaseScreen
    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Private TeachersViewSource As CollectionViewSource

    Private CourseTeachersViewSource As CollectionViewSource
    Private TuitionTypesViewSource As CollectionViewSource

    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID

        If Not (PropertyName = "GradeID" Or PropertyName = "CourseID") Then
            Dim q_grades = From bt In DBContext.SemesterBatches.Include("Grades")
                       Where bt.SemesterId = SemesterID And bt.YearId = YearID
                       Select bt.Grade Distinct

            GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
            GradesViewSource.Source = New ObservableEntityCollection(Of Grade)(DBContext, q_grades)

            If q_grades.Count > 0 AndAlso Not (q_grades.ToList().Any(Function(gr) gr.Id = GradeID)) Then
                SharedState.GetSingleInstance.GradeID = q_grades.First().Id
                GradeID = SharedState.GetSingleInstance().GradeID
            End If
        End If

        TeachersViewSource = CType(Me.FindResource("TeachersViewSource"), CollectionViewSource)
        TeachersViewSource.Source = New ObservableEntityCollection(Of Teacher)(DBContext, DBContext.Teachers)
        TuitionTypesViewSource = CType(Me.FindResource("TuitionTypesViewSource"), CollectionViewSource)
        TuitionTypesViewSource.Source = DBContext.TuitionTypes.ToList()

        Dim q_courseteachers = From ct In DBContext.CourseTeachers
                               Where ct.YearId = YearID And ct.SemesterId = SemesterID
                               Select ct

        CourseTeachersViewSource = CType(Me.FindResource("CourseTeachersViewSource"), CollectionViewSource)
        CourseTeachersViewSource.Source = New ObservableEntityCollection(Of CourseTeacher)(DBContext, q_courseteachers)
        CourseTeachersViewSource.View.Filter = AddressOf GradeCourseFilter

    End Sub
    Sub QueryParamChangeHandler(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = "YearID" Or e.PropertyName = "SemesterID" Then
            LoadData("")
        End If
    End Sub
    Function GradeCourseFilter(s As Object) As Boolean
        Dim item As CourseTeacher = CType(s, CourseTeacher)
        Return (item.GradeId = SharedState.GetSingleInstance().GradeID) AndAlso (item.CourseId = SharedState.GetSingleInstance().CourseID)
    End Function

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
        QueryParamsBox.DataContext = SharedState.GetSingleInstance
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamsBox.DataContext = Nothing
    End Sub
End Class
