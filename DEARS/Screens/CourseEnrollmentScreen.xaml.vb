Public Class CourseEnrollmentScreen
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


        'We have to load three types of data. 
        'Students already resgistered in the course.
        'Students registered in disciplines with the course as compulsory
        'Allow registration of students with disciplines where the course is recorded as optional
        Dim q_cenrstud = From cenr In DBContext.CourseEnrollments.Include("Student")
                         Where cenr.YearId = YearID And cenr.GradeId = GradeID And cenr.CourseId = CourseID And SemesterID = cenr.SemesterId
                         Select cenr

        Dim q_comp_disc = From cd In DBContext.CourseDisciplines
                          Where cd.YearId = YearID And cd.GradeId = GradeID And cd.CourseId = CourseID And SemesterID = cd.SemesterId And cd.Optional = False
                          Select cd.Discipline.Id Distinct

        'Students not enrolled in course but enrolled in discipline with the course compulsory
        Dim q_comp = From denr In DBContext.SemesterBatchEnrollments.Include("Student")
                     Where q_comp_disc.Any(Function(s) s = denr.DisciplineId) And denr.YearId = YearID And denr.GradeId = GradeID And SemesterID = denr.SemesterId And Not denr.CourseEnrollments.Any(Function(s) s.CourseId = CourseID)
                     Select denr

        Dim StudentsCollection As New ObservableEntityCollection(Of CourseEnrollment)(DBContext, q_cenrstud)

        For Each sbe In q_comp.ToList()
            StudentsCollection.Add(New CourseEnrollment With {.YearId = YearID, .SemesterId = SemesterID, .CourseId = CourseID, .GradeId = GradeID, .StudentId = sbe.StudentId,
                                                              .SemesterBatchEnrollment = sbe, .Student = sbe.Student})
        Next
        StudentsViewSource.Source = StudentsCollection
        DBContext.ChangeTracker.DetectChanges()
        StudentsViewSource.View.Filter = AddressOf DisciplineFilterFunction

    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        StudentsViewSource = CType(Me.FindResource("StudentsViewSource"), CollectionViewSource)
        LoadData("")
        QueryParamsBox.DataContext = SharedState.GetSingleInstance
    End Sub

    Private Function DisciplineFilterFunction(s As Object) As Boolean
        Dim item = CType(s, CourseEnrollment)
        If AllDisciplinescheckBox.IsChecked Then
            Return True
        Else
            Return (SharedState.GetSingleInstance.DisciplineID = item.SemesterBatchEnrollment.DisciplineId)
        End If
    End Function

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamsBox.DataContext = Nothing
    End Sub

    Private Sub AllDisciplinescheckBox_Checked(sender As Object, e As RoutedEventArgs)
        StudentsViewSource.View.Filter = AddressOf DisciplineFilterFunction
    End Sub

    Private Sub AllDisciplinescheckBox_Unchecked(sender As Object, e As RoutedEventArgs)
        StudentsViewSource.View.Filter = AddressOf DisciplineFilterFunction
    End Sub
End Class

Public Class NonOptionalEnrollment
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Dim cenr = CType(value, CourseEnrollment)
        Dim DiscId = cenr.SemesterBatchEnrollment.DisciplineId
        Return cenr.OfferedCourse.CourseDisciplines.Any(Function(s) s.DisciplineId = DiscId And s.Optional = False)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException("Conversion back is not logical")
    End Function
End Class

Public Class NonOptionalEnrollmentHeaderConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Dim cenr = CType(value, CourseEnrollment)
        Dim DiscId = cenr.SemesterBatchEnrollment.DisciplineId
        If cenr.OfferedCourse.CourseDisciplines.Any(Function(s) s.DisciplineId = DiscId And s.Optional = False) Then
            Return "C"
        Else
            Return ""
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException("Conversion back is not logical")
    End Function
End Class