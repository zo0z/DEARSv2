Module Module1

    Sub Main()
        Console.ReadKey()
    End Sub

End Module
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
Public Enum RecommTypeEnum
    I = 1
    II
    II1
    II2
    III
    Passed
    Repeat
    Failed
    Subs
    Supp
    SubsSupp
    Resit
    WGPA
    SpecialCase
    Suspend
    Dismiss
    SubYear
End Enum
Public Module ResultsProcessUtils
    Class RecommData
        Public GPA? As Decimal
        Public YearRecomm As RecommTypeEnum
        Public CGPA? As Decimal
        Public CumulativeRecomm As RecommTypeEnum
        Public Comment As String
    End Class
    Function SecondSemesterProcessing(YearId As Integer, GradeId As Integer, DisciplineId As Integer, ExamType As ExamTypeEnum) As List(Of RecommData)
        Dim RecommList As New List(Of RecommData)
        Dim StudList As List(Of BatchEnrollment) = GetStudentsEnrollmentList(YearId, GradeId, DisciplineId, ExamType)
        For Each stud In StudList
            RecommList.Add(SecondSemesterStudProcess(stud))
        Next
        Return RecommList
    End Function

    Private Function GetStudentsEnrollmentList(YearId As Integer, GradeId As Integer, DisciplineId As Integer, ExamType As ExamTypeEnum) As List(Of BatchEnrollment)
        Throw New NotImplementedException
    End Function

    Private Function SecondSemesterStudProcess(studEnr As BatchEnrollment) As RecommData
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
    End Function

    Private Function SecondSemesterProcessRegularStudent(studEnr As BatchEnrollment) As RecommData
        Throw New NotImplementedException
    End Function


End Module