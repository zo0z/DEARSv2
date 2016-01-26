Imports DocumentFormat.OpenXml.Spreadsheet
Imports DEARS.ExcelSimplified
Public Enum Disciplines
    Dept = 1
    Elec
    Soft
    Tele
    Cont
    Pow
End Enum

Public Class ResultsIssue
    Private _TYear As Integer
    Private _FirstOnly As Boolean
    Private _Grade As Integer
    Private wb As CWorkbook
    Private db As AcademicResultsDBEntities
    Public Sub New(ByVal FileName As String, ByVal TYear As Integer, ByVal FirstOnly As Boolean, ByVal Grade As Integer)
        Me._TYear = TYear
        Me._FirstOnly = FirstOnly
        Me._Grade = Grade
        wb = New CWorkbook(FileName, True)
        db = SharedState.DBContext

    End Sub
    Function GradedDisciplines(ByVal Grade As Integer, ByVal discp As Disciplines) As Disciplines
        Select Case Grade
            Case 3
                If discp = Disciplines.Soft Then
                    Return Disciplines.Dept
                End If
                Return discp
            Case 4
                If discp = Disciplines.Cont Or discp = Disciplines.Pow Then
                    Return Disciplines.Dept
                End If
                Return discp
            Case Else
                Return discp
        End Select
    End Function

    Sub AddDisciplineResults(ByVal Discipline As Disciplines, Optional ByVal SubResult As Boolean = False)

        Dim ws As CWorksheet = wb.CreateNewWorksheet(Discipline.ToString(), True)

        Dim disc = GradedDisciplines(_Grade, Discipline)

        Dim CoursesInDiscipline_x = (From ofc In SharedState.DBContext.OfferedCourses.Include("CourseDisciplines")
                                   Where ((ofc.YearId = Me._TYear And ofc.GradeId = Me._Grade And ofc.SemesterId = 1) _
                                          Or ((Not Me._FirstOnly) And ofc.YearId = Me._TYear And ofc.GradeId = Me._Grade And ofc.SemesterId = 2))).ToList()

        Dim CoursesInDiscipline = (From x In CoursesInDiscipline_x
                                  Where ((x.SemesterId = 1 And x.CourseDisciplines.Any(Function(s) s.DisciplineId = disc)) _
                                         Or (x.SemesterId = 2 And x.CourseDisciplines.Any(Function(s) s.DisciplineId = Discipline)))).ToList()

        'Dim CoursesInDiscipline = db.GetDisciplineCoursesList(_TYear, GradedDisciplines(_Grade, Discipline), sem - 1).ToList()

        'CoursesInDiscipline.AddRange(db.GetDisciplineCoursesList(_TYear, Discipline, sem))


        Dim orderCourses = CoursesInDiscipline.OrderBy(Of Integer)(Function(s) CourseOrder(s.Course.CourseCode)).ToList()
        Dim numberOfCourses As Integer = orderCourses.Count()

        Dim TotalCreditHours10 As Integer = CoursesInDiscipline.Sum(Function(ct) ct.CreditHours) * 10

        'Dim StudentsInDiscipline = db.GetDisciplineStudentsList(_TYear, sem, Discipline, True).ToList()
        Dim StudentsInDiscipline = (From st In db.SemesterBatchEnrollments.Include("CourseEnrollments").Include("CourseEnrollments.MarksExamCW").Include("Student")
                                   Where (st.YearId = Me._TYear And st.GradeId = Me._Grade) And ((Not Me._FirstOnly And st.DisciplineId = Discipline And st.SemesterId = 2) Or (Me._FirstOnly And st.DisciplineId = disc And st.SemesterId = 1))
                                   Select st.Student Distinct).ToList()

        Dim col As Integer
        ws.SetWidths(1, 1, 15)
        ws.SetWidths(2, 2, 15)
        ws.SetWidths(3, 3, 25)

        ws.SetWidths(4, 4 + CoursesInDiscipline.Count * 2, 3.8)

        ws.CreateNewRange(1, 3, "Index", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
        ws.CreateNewRange(2, 3, "Univ No", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
        ws.CreateNewRange(3, 3, "Name", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
        col = 4
        For Each Course In CoursesInDiscipline.OrderBy(Of Integer)(Function(cr) CourseOrder(cr.Course.CourseCode))
            ws.CreateNewRange(col, 3, Course.Course.CourseCode, CellValues.SharedString, 1, 2, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
            ws.CreateNewRange(col, 4, Course.CreditHours.ToString, CellValues.Number, 1, 2, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
            ws.CreateNewRange(col, 5, Course.CourseWorkFraction.ToString, CellValues.Number, 1, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
            ws.CreateNewRange(col + 1, 5, Course.ExamFraction.ToString, CellValues.Number, 1, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin)
            col += 2
        Next

        ws.SetWidths(col, col + 1, 4)
        ws.SetWidths(col + 2, col + 2, 6)
        ws.CreateNewRange(col, 3, "ABSENT", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, Alignment:=90)
        ws.CreateNewRange(col + 1, 3, "FAIL", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, Alignment:=90)
        ws.CreateNewRange(col + 2, 3, "GPA", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, Alignment:=90)
        ws.CreateNewRange(col + 3, 3, "Recommendation", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, Alignment:=90)
        ws.CreateNewRange(col + 4, 3, "CGPA", CellValues.SharedString, 3, 1, BGColor:="C0C0C0", Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, Alignment:=90)

        ws.CreateNewRange(1, 1, "TITLE AREA", CellValues.SharedString, 1, col + 4, Font:="Cambria")

        Dim stR As Integer = 6

        For Each stud In StudentsInDiscipline
            col = 0
            ws.CreateNewRange(1, stR, stud.Index, CellValues.Number, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center,
               Font:="Traditional arabic")
            If stud.UnivNo IsNot Nothing Then
                ws.CreateNewRange(2, stR, stud.UnivNo, CellValues.InlineString, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin,
                  HAlign:=HorizontalAlignmentValues.Center)
            Else
                ws.CreateNewRange(2, stR, "N/A", CellValues.InlineString, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin,
               HAlign:=HorizontalAlignmentValues.Center)
            End If
            ws.CreateNewRange(3, stR, stud.NameArabic, CellValues.InlineString, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, Font:="Traditional arabic",
               HAlign:=HorizontalAlignmentValues.Right)


            'Dim mks = db.GetMarksForStudent(_TYear, sem - 1, stud.SIndex)
            'mks.AddRange(db.GetMarksForStudent(_TYear, sem, stud.SIndex))

            Dim mks = (From crs In stud.MarksExamCWs
                       Where ((crs.YearId = Me._TYear And crs.GradeId = Me._Grade And crs.SemesterId = 1) Or (Not Me._FirstOnly And crs.YearId = Me._TYear And crs.GradeId = Me._Grade And crs.SemesterId = 2))
                       Select crs).ToList()

            col = 4
            If mks.Count = numberOfCourses Then
                For Each mk In mks.OrderBy(Of Integer)(Function(mky) CourseOrder(mky.Course.CourseCode))
                    ws.CreateNewRange(col, stR, mk.CWMark.ToString, CellValues.Number, 1, 1, Border:=CBorders.Left Or CBorders.Top, BorderStyle:=BorderStyleValues.Thin)
                    ws.CreateNewRange(col + 1, stR, mk.ExamMark.ToString, CellValues.Number, 1, 1, Border:=CBorders.Right Or CBorders.Top, BorderStyle:=BorderStyleValues.Thin)

                    Dim TotalMarkFormula As String = String.Format("IF({0}="""",""--"",SUM({1},{0}))", GetColumnName(col + 1) & stR, GetColumnName(col) & stR)
                    ws.CreateNewFormulaRange(col, stR + 1, TotalMarkFormula, 1, 1, Border:=CBorders.Left Or CBorders.Bottom, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center)
                    ws.CreateNewFormulaRange(col + 1, stR + 1, GenerateGradingFormula(col + 1, stR + 1, mk.CWMark.HasValue), 1, 1, Border:=CBorders.Right Or CBorders.Bottom, BorderStyle:=BorderStyleValues.Thin,
                     HAlign:=HorizontalAlignmentValues.Center)
                    col += 2
                Next
            Else
                For Each mk In mks.OrderBy(Of Integer)(Function(mky) CourseOrder(mky.Course.CourseCode))
                    ws.CreateNewRange(col, stR, mk.CWMark.ToString, CellValues.Number, 1, 1, Border:=CBorders.Left Or CBorders.Top, BorderStyle:=BorderStyleValues.Thin)
                    ws.CreateNewRange(col + 1, stR, mk.ExamMark.ToString, CellValues.Number, 1, 1, Border:=CBorders.Right Or CBorders.Top, BorderStyle:=BorderStyleValues.Thin)

                    Dim TotalMarkFormula As String = String.Format("IF({0}="""",""--"",SUM({1},{0}))", GetColumnName(col + 1) & stR, GetColumnName(col) & stR)
                    ws.CreateNewFormulaRange(col, stR + 1, TotalMarkFormula, 1, 1, Border:=CBorders.Left Or CBorders.Bottom, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center)
                    ws.CreateNewFormulaRange(col + 1, stR + 1, GenerateGradingFormula(col + 1, stR + 1, mk.CWMark.HasValue), 1, 1, Border:=CBorders.Right Or CBorders.Bottom, BorderStyle:=BorderStyleValues.Thin,
                     HAlign:=HorizontalAlignmentValues.Center)
                    col += 2
                Next
                col = 4 + 2 * numberOfCourses
            End If




            Dim gradesRange As String = "D" & (stR + 1) & ":" & GetColumnName(col - 1) & (stR + 1)

            Dim frmABtemp = "COUNTIF({0},""AB"")"
            Dim frmTemp = "COUNTIF({0},""F*"")+COUNTIF({0},""D*"")+COUNTIF({0},""AB*"")-COUNTIF({0},""AB"")"
            Dim frm As String = String.Format(frmTemp, gradesRange)
            Dim frmAB As String = String.Format(frmABtemp, gradesRange)
            ws.CreateNewFormulaRange(col, stR, frmAB, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center)
            ws.CreateNewFormulaRange(col + 1, stR, frm, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center)
            ws.CreateNewFormulaRange(col + 2, stR, GenerateGPAFormula(4, stR, TotalCreditHours10, orderCourses), 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center)

            If Me._FirstOnly Then

            Else
                Dim rc = (From gp In stud.GPAwRecomms
                         Where gp.YearId = Me._TYear And Me._Grade = gp.GradeId).SingleOrDefault()
                If rc IsNot Nothing Then
                    '   ws.CreateNewRange(col + 3, stR, rc.RecommendationType.ShortNameEnglish, CellValues.InlineString, 2, 1, Border:=CBorders.All, BorderStyle:=BorderStyleValues.Thin,
                    'HAlign:=HorizontalAlignmentValues.Center)
                End If
            End If

            stR += 2
            'If SubResult Then
            '    col = 4
            '    mks = db.GetMarksForStudent(_TYear, sem - 1, stud.SIndex, False)
            '    For Each mk In mks.OrderBy(Of Integer)(Function(mky) CourseOrder(mky.CourseCode))
            '        ws.CreateNewRange(col, stR, mk.CWMark.ToString, CellValues.Number, 1, 1, Border:=CBorders.Left Or CBorders.Top, BorderStyle:=BorderStyleValues.Thin)
            '        ws.CreateNewRange(col + 1, stR, mk.ExamMark.ToString, CellValues.Number, 1, 1, Border:=CBorders.Right Or CBorders.Top, BorderStyle:=BorderStyleValues.Thin)

            '        Dim TotalMarkFormula As String = String.Format("IF({0}="""",""--"",SUM({1},{0}))", GetColumnName(col + 1) & stR, GetColumnName(col) & stR)
            '        ws.CreateNewFormulaRange(col, stR + 1, TotalMarkFormula, 1, 1, Border:=CBorders.Left Or CBorders.Bottom, BorderStyle:=BorderStyleValues.Thin, HAlign:=HorizontalAlignmentValues.Center)
            '        ws.CreateNewFormulaRange(col + 1, stR + 1, GenerateGradingFormula(col + 1, stR + 1, mk.CWMark.HasValue), 1, 1, Border:=CBorders.Right Or CBorders.Bottom, BorderStyle:=BorderStyleValues.Thin,
            '         HAlign:=HorizontalAlignmentValues.Center)
            '        col += 2
            '    Next
            '    stR += 2
            'End If
        Next



    End Sub
    Public Sub Save()
        wb.Save()
    End Sub

    Function GenerateGradingFormula(ByVal Col As Integer, ByVal Row As Integer, ByVal CWHasValue As Boolean) As String
        Dim ref1 = GetColumnName(Col - 1) & (Row - 1)
        Dim ref2 = GetColumnName(Col) & (Row - 1)
        Dim ref3 = GetColumnName(Col - 1) & (Row)
        Dim ref4 = GetColumnName(Col) & (Row)
        Dim refCW = GetColumnName(Col - 1) & "$5"
        Dim refEX = GetColumnName(Col) & "$5"

        Dim tempFor As String = Nothing
        If CWHasValue Then
            '"IF(AND(E9=""--"",E8>=E$6*0.4),""AB"",IF(AND(E9=""--"",E8<E$6*0.4),""AB*"",IF(E8<E$6*0.4,IF(OR(F8<F$6*0.3,E8<E$6*0.3),""F*"",""D*""),IF(F8<F$6*0.3,""F"",IF(F8<F$6*0.4,""D"",IF(E9<70,IF(E9<50,""C"",IF(E9<60,""B"",""B+"")),IF(E9<80,""A-"",IF(E9<90,""A"",""A+""))))))))"
            tempFor = "IF(AND({2}=""--"",{0}>={4}*0.4),""AB"",IF(AND({2}=""--"",{0}<{4}*0.4),""AB*"",IF({0}<{4}*0.4,IF(OR({1}<{5}*0.3,{0}<{4}*0.3),""F*"",""D*""),IF({1}<{5}*0.3,""F"",IF({1}<{5}*0.4,""D"",IF({2}<70,IF({2}<50,""C"",IF({2}<60,""B"",""B+"")),IF({2}<80,""A-"",IF({2}<90,""A"",""A+""))))))))"
        Else
            tempFor = "IF({2}=""--"",""AB"",IF({2}<40,IF({2}<30,""F"",""D""),IF({2}<50,""C"",IF({2}<60,""B"",IF({2}<70,""B+"",IF({2}<80,""A-"",IF({2}<90,""A"",""A+"")))))))"
        End If
        Dim formula As New System.Text.StringBuilder(tempFor)
        formula.Replace("{0}", ref1)
        formula.Replace("{1}", ref2)
        formula.Replace("{2}", ref3)
        formula.Replace("{3}", ref4)
        formula.Replace("{4}", refCW)
        formula.Replace("{5}", refEX)
        Return formula.ToString()
    End Function
    Function GenerateGPAFormula(ByVal StartCol As Integer, ByVal rInd As Integer, ByVal TotalCreditHours10 As Integer, ByVal CoursesList As List(Of OfferedCourse)) As String

        Dim fml As String = "("
        For i As Integer = 0 To CoursesList.Count - 2
            fml &= CoursesList(i).CreditHours & "*(" & (GetColumnName(StartCol) & rInd) & "+" & (GetColumnName(StartCol + 1) & rInd) & ")+"
            StartCol += 2
        Next
        fml &= CoursesList.Last.CreditHours & "*(" & (GetColumnName(StartCol) & rInd) & "+" & (GetColumnName(StartCol + 1) & rInd) & ")" & ")/(" & (TotalCreditHours10) & ")"
        Return "IF(" & GetColumnName(StartCol + 2) & rInd & "=0," & fml & ", ""--"")"
    End Function
    Function CourseOrder(ByVal CourseCode As String) As Integer
        If CourseCode = "PR5202" Then
            Return 1000000
        End If
        Return (Integer.Parse(CourseCode.Substring(2)) * 100 + Asc(CourseCode(0)) * 100 + Asc(CourseCode(1)) * 10)
    End Function
End Class

