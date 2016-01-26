Public Module ResultsProcessingUtilities
    Public Enum ExamTypeEnum
        FirstSemester
        SecondSemester
        SubSuppExams
    End Enum
    Public Enum EnrollmentTypeEnum
        Regular = 1
        Transfer
        External
        Repeat
        Resit
    End Enum

    Sub SecondSemesterProcessing(YearId As Integer, GradeId As Integer, DisciplineId As Integer, ExamType As ExamTypeEnum)
        Dim StudList As List(Of BatchEnrollment) = GetStudentEnrollmentList(YearId, GradeId, DisciplineId, ExamType)
        Dim RecommsList As List(Of GPAwRecomm) = New List(Of GPAwRecomm)(StudList.Count)
        For StudInd As Integer = 0 To StudList.Count - 1
            RecommsList.Add(SecondSemesterStudProcess(StudList(StudInd)))
        Next
    End Sub

    Private Function GetStudentEnrollmentList(YearId As Integer, GradeId As Integer, DisciplineId As Integer, ExamType As ExamTypeEnum) As List(Of BatchEnrollment)
        Throw New NotImplementedException
    End Function

    Private Function SecondSemesterStudProcess(studEnr As BatchEnrollment) As GPAwRecomm
        Select Case studEnr.EnrollmentTypeId
            Case EnrollmentTypeEnum.Regular
                Return SecondSemesterProcessRegularStudent(studEnr)
            Case EnrollmentTypeEnum.Transfer
            Case EnrollmentTypeEnum.External
            Case EnrollmentTypeEnum.Repeat
            Case EnrollmentTypeEnum.Resit
            Case Else
                Throw New ArgumentException("Student Enrollment Typeis not Known")
        End Select
        Throw New NotImplementedException
    End Function
    Class GradeTotal
        Public Mark As MarksExamCW
        Public Grade As String
        Public Total As Decimal
    End Class
    Private Function SecondSemesterProcessRegularStudent(studEnr As BatchEnrollment) As GPAwRecomm
        Dim Marks As List(Of MarksExamCW) = GetStudentMarks(studEnr, ExamTypeEnum.SecondSemester)
        Dim GradesTotalList As List(Of GradeTotal) = Marks.ConvertAll(Of GradeTotal)(Function(s) AssignGrade(s))
        Dim Recomm As New GPAwRecomm()
        Dim PreviousCGPA As Decimal? = GetPreviousCGPA(studEnr)

        Dim GPA As Decimal = EvaluateGPA(GradesTotalList)
        Dim CGPA As Decimal = EvaluateCGPA(Recomm.GPA, PreviousCGPA, studEnr.GradeId)

        Recomm.GPA = GPA
        Recomm.CGPA = CGPA

        Dim PrjCourseID As Integer = GetProjectCourseID()

        If studEnr.GradeId = 5 And GradesTotalList.Any(Function(s) s.Mark.CourseId = PrjCourseID And (s.Grade = "F" Or s.Grade = "D")) Then

        End If

        'Recomm.RecommendationType = NiceRecomm(GPA) 'Applies 5.11, 8.2
        'Recomm.CumulativeRecommendationType = GNiceRecomm(CGPA, PreviousCGPA, Recomm.RecommendationType) 'Applies 5.12

        Dim CountFs As Integer = GradesTotalList.Where(Function(s) s.Grade = "F").Count()
        Dim CountDs As Integer = GradesTotalList.Where(Function(s) s.Grade = "D").Count()
        Dim CountABs As Integer = GradesTotalList.Where(Function(s) s.Grade = "AB" Or s.Grade = "AB*").Count()
        Dim CountSubjects As Integer = GradesTotalList.Count
        Dim CountNonExcused As Integer = Marks.Where(Function(mk) Not mk.Present And Not mk.Excuse).Count()

        If CountNonExcused > (CountSubjects / 3) Then

        ElseIf CountABs > 0 Then
            'Recomms.YearRecomm = Sub ( CountABs )
            '// Note that GPA should be NaN since some subjects will have exam mark set as NaN
            '//Recomms.CumulativeRecomm = Null
        ElseIf CountFs = 0 And CountDs = 0 And GPA >= 4.5 Then
            '// Hurrah! Go for vacation!
            'Return // Applies 8.1
        ElseIf (4.3 <= CGPA) And (CGPA < 4.5) And CountFs = 0 And CountDs = 0 Then
            'Return // 8.2 Already Applied
        ElseIf GPA >= 4.5 And CountFs = 0 And CountDs = 1 Then
            'Return // 8.3 Already Applied
        ElseIf GPA >= 4.3 And (CountFs + CountDs) < Math.Ceiling(CountSubjects / 3) Then
            'Recomms.YearRecomm.Append( Supp ( CountFs + CountDs ) ) // Applies 9.1
            'ElseIf IsNaN(GPA) And (CountFs + CountDs) < Ceil(CountSubjects/3) // Cautiously applies 9.1(A,B)
            'Recomms.YearRecomm.Append(Supp(CountFs + CountDs))
        ElseIf GPA >= 4.5 And (CountDs + CountFs) < Math.Ceiling(CountSubjects / 3) + 1 And (CountDs >= 1) Then
            'Recomms.YearRecomm .Append(‘Special Case (FG 9.2) ⇒ Supp ( CountFs + CountDs - 1))’)
            '// Applies 9.2
            '// Skip 9.3 applies to Supp
        ElseIf 3.5 <= GPA And GPA < 4.3 And CountFs = 0 And CountDs = 0 Then
            'Recomms.YearRecomm = “Special Case (FG 10.1) ⇒ Repeat” //Applies 10.1
        ElseIf GPA >= 3.5 And (CountFs + CountDs) > Math.Ceiling(CountSubjects / 3) Then
            'Recomms.YearRecomm = “Special Case (FG 10.2) ⇒ Repeat” //Applies 10.2
        ElseIf 4.3 <= PreviousCGPA And PreviousCGPA < 4.5 And CGPA < 4.5 Then
            'Return //Already Applied 10.3
            '// Skip 10.4 applies to Supp
            '// 10.5 Applied above (Project)
            '// 10.6 Below (Double Repeats)
        ElseIf GPA < 3.5 Then
            'Recomms.YearRecomm = “Dismiss (FG 11.a)” // Applies 11.a
        Else

        End If

        Return Recomm

    End Function

    Private Function GetStudentMarks(studEnr As BatchEnrollment, examTypeEnum As ExamTypeEnum) As List(Of MarksExamCW)
        Throw New NotImplementedException
    End Function

    Private Function AssignGrade(s As MarksExamCW) As GradeTotal
        Throw New NotImplementedException
    End Function

    Private Function EvaluateGPA(GradesTotalList As List(Of GradeTotal)) As Decimal
        Throw New NotImplementedException
    End Function

    Private Function GetPreviousCGPA(studEnr As BatchEnrollment) As Decimal?
        Throw New NotImplementedException
    End Function

    Private Function EvaluateCGPA(p1 As Decimal, PreviousCGPA As Decimal?, p3 As Integer) As Decimal?
        Throw New NotImplementedException
    End Function

    Private Function GetProjectCourseID() As Integer
        Throw New NotImplementedException
    End Function

    Private Function NiceRecomm(GPA As Decimal) As RecommendationType
        Throw New NotImplementedException
    End Function

    Private Function GNiceRecomm(CGPA As Decimal, PreviousCGPA As Decimal?, recommendationType As RecommendationType) As RecommendationType
        Throw New NotImplementedException
    End Function



End Module
