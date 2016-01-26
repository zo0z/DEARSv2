Imports System.Text.RegularExpressions
Public Enum Disciplines
    Dept = 1
    Elec = 2
    Soft = 3
    Tele = 4
    Cont = 5
    Pow = 6
End Enum

Public Class CResultsDataExtractor
    Shared sheetNames() As String = {"Dept", "Elec", "Soft", "Tele", "Cont", "Pow"}
    Shared CourseTitleRegex As New Regex("[A-Z][A-Z]\d{4}")
    Shared Guesser As New SemesterGuessHelper()
    Public Shared Translator As RecommendationsTraslator
    Public Shared UseTranslator As Boolean = True
    Public Shared Recomms As New Dictionary(Of String, Integer)
    'Shared HeadersList As New List(Of List(Of String))
    Structure Student
        Dim SIndex As Integer
        Dim SUNumber As String
        Dim Name As String
        Dim SpecialEnroll As Integer
        Dim AssociatedRecords As Integer
        Dim Discipline1 As Disciplines
        Dim Discipline2 As Disciplines
    End Structure
    Structure Course
        Dim CourseCode As String
        Dim CreditHours As Integer
        Dim ExamFraction As Integer
        Dim CWFraction As Integer
        Dim Semester As Integer
        Dim TDisciplines As List(Of Disciplines)
    End Structure
    Structure Mark
        Dim CourseRef As Integer
        Dim ExamMark As Nullable(Of Decimal)
        Dim CWMark As Nullable(Of Decimal)
        Dim ExType As Integer
    End Structure
    Class Summary
        Public Absent As Integer
        Public Fail As Integer
        Public GPA As Nullable(Of Decimal)
        Public Recomm As String
        Public CGPA As Nullable(Of Decimal)
        Public FinalRecomm As String
    End Class
    Public StudentsData As List(Of Student)
    Public CoursesData As List(Of Course)
    Public MarksData As List(Of List(Of Mark))
    Public SummaryData As List(Of Summary)

    Public TYear As Integer
    Public ExamType As Integer
    Public FirstSemesterOnly As Boolean

    Public Grade As Integer

    'Currenlty not used
    Public DisciplinesInc As New List(Of Disciplines)

    Public DataFieldsIncluded As List(Of String)

    Private OrderedDisciplines As Boolean = True

    Public Sub New(ByRef ws As CWorksheet, ByRef Segmenter As CResultsSegmeter, ByVal TYear As Integer, ByVal Grade As Integer, ByVal ExamType As Integer)
        Trace.Indent()
        Dim SheetName As String = ws.GetWorksheetName()
        Me.TYear = TYear
        Me.Grade = Grade
        Me.FirstSemesterOnly = (ExamType = 0)
        Me.ExamType = ExamType
        Dim Multidisciplinary As Boolean = False

        If SheetName.Contains("+") Then
            Multidisciplinary = True
            Dim DeptNames = SheetName.Split("+")
            For Each d In DeptNames
                DisciplinesInc.Add(Array.IndexOf(sheetNames, d) + 1)
            Next
        Else
            DisciplinesInc.Add(Array.IndexOf(sheetNames, SheetName) + 1)
        End If

        'Extract sheet names


        'This method should extract the data found in the sheet into the variables above
        'Start with headers
        'Trace.WriteLine(Segmenter.BigHeaderRegions.Count)
        'If TargetFile = "C:\Users\Mohanad\Desktop\ResultsBag\DEEEC\2005\Final4.xlsx" And SheetName = "Pow" Then
        '	Beep()
        '	SheetName = SheetName.Trim()
        'End If

        If Segmenter.BigHeaderRegions.Count = 1 Then
            'Bad 
            Trace.WriteLine("Un merged header probably on rigth side")
            Throw New Exception("Un merged header probably on rigth side")
        End If
        If Segmenter.BigHeaderRegions.Count > 2 Then
            'Choose the one with the smallest width
            Dim minWidth = Segmenter.BigHeaderRegions.Min(Function(s) CRange.RangeWidth(s))
            Trace.WriteLine("Big Header regions not as expected")
            Dim misplaced = Segmenter.BigHeaderRegions.Where(Function(s) CRange.RangeWidth(s) = minWidth).Single()
            Segmenter.BigHeaderRegions.RemoveAll(Function(s) CRange.RangeWidth(s) = minWidth)
            'For a new range refwith onlyy single row
            'Dim startCol As Integer = RangeColumn(misplaced)
            'Dim endCol As Integer = startCol + RangeWidth(misplaced) - 1
            'Dim startRow As Integer = RangeRow(misplaced)
            'Dim ref As String = GetColumnName(startCol) & startRow & ":" & GetColumnName(endCol) & startRow
            Segmenter.CoursesRegions.Add(misplaced)
            'Segmenter.Debug_ShowRegions(TargetFile, SheetName)
        End If
        'Extract courses data

        Dim CoursesCount As Integer = 0
        Dim CourseCodes As List(Of String) = Nothing
        Dim DataAreaStartColumn As Integer = 0
        Dim msg As String = ""

        'Dim S1 As String = Nothing
        'If SingleHeader Then
        '	For Each SubRange In Segmenter.CoursesRegions
        '		Dim ctitles = ExtractDataFromSubrange(ws, SubRange)
        '		If ctitles.All(Function(t) CourseTitleRegex.IsMatch(t)) Then
        '			S1 = SubRange
        '			Exit For
        '		End If
        '	Next
        'End If

        'If SingleHeader And S1 IsNot Nothing Then
        '	Segmenter.BigHeaderRegions.AddRange(Segmenter.CoursesRegions.Where(Function(s) (RangeRow(s) = RangeRow(S1)) And s <> S1))
        '	Segmenter.CoursesRegions.RemoveAll(Function(s) (RangeRow(s) = RangeRow(S1)) And (s <> S1))
        'End If


        Dim rowSets As List(Of Integer) = Segmenter.CoursesRegions.ConvertAll(Of Integer)(Function(s) CRange.RangeRow(s)).Distinct().ToList()
        For Each El In rowSets
            Dim X = El
            'Trace.WriteLine(El & "---------")
            Dim SubRangesToBeConsolidated = Segmenter.CoursesRegions.Where(Function(s) CRange.RangeRow(s) = X)
            Dim ctitles = ws.ExtractDataFromSubrange(ConsolidateSubRanges(SubRangesToBeConsolidated))
            'msg = ""
            'For Each cl In ctitles
            '	msg &= cl & vbCrLf
            'Next
            'Trace.WriteLine(msg)
            If ctitles.All(Function(t) CourseTitleRegex.IsMatch(t)) Then
                'This is the course codes
                CoursesCount = ctitles.Count
                CourseCodes = ctitles
                DataAreaStartColumn = CRange.RangeColumn(ConsolidateSubRanges(SubRangesToBeConsolidated))
                rowSets.Remove(X)
                Exit For
            End If
            'Trace.WriteLine(ctitles.Count)
            'Trace.WriteLine(ctitles.All(Function(t) CourseTitleRegex.IsMatch(t)))
            'Trace.WriteLine("----------------------------------------------------------------------------========")
        Next

        'Corrective measure
        Dim CorrectiveCondition As Boolean = True
        Dim AnyCourseSchema As Integer = 0
        Dim AllCourseSchema As Integer = 0
        If CoursesCount = 0 Then
            'We could't find a consolidated region in which all elements are course codes. Search through sub ranges
            'non consolidated and find a region which is all course codes. Make sure all the rest have non conforming to schema
            'Move all the other cells to BigHeaders group
            Dim CourseSub As String = ""
            For Each SubRange In Segmenter.CoursesRegions
                Dim ctitles = ws.ExtractDataFromSubrange(SubRange)
                If ctitles.All(Function(t) CourseTitleRegex.IsMatch(t)) Then
                    AllCourseSchema += 1
                    If AllCourseSchema = 1 Then
                        CourseSub = SubRange
                        CourseCodes = ctitles
                    ElseIf AllCourseSchema > 1 Then
                        Exit For
                    End If
                ElseIf ctitles.Any(Function(t) CourseTitleRegex.IsMatch(t)) Then
                    AnyCourseSchema += 1
                End If
            Next
            If AllCourseSchema = 1 And AnyCourseSchema = 0 Then
                CoursesCount = CourseCodes.Count
                rowSets.Remove(CRange.RangeRow(CourseSub))
                DataAreaStartColumn = CRange.RangeColumn(CourseSub)
                Trace.WriteLine("SPECIAL RULE INVOKE: CORRECTION OF UNMERGED BIG HEADERS (NOT BIG)")
            Else
                Throw New Exception("Correction of BigHeaders and Courses misplacement failed")
            End If
        End If

        CoursesData = New List(Of Course)(CoursesCount)

        Dim CreditHours As New List(Of Integer)(CoursesCount)
        Dim CWEX_Fractions As New List(Of Integer)
        For Each El In rowSets
            Dim X = El
            Dim SubRangesToBeConsolidated = Segmenter.CoursesRegions.Where(Function(s) CRange.RangeRow(s) = X)
            Dim courseDataElems = ws.ExtractDataFromSubrange(ConsolidateSubRanges(SubRangesToBeConsolidated))
            'msg = ""
            'For Each cl In courseDataElems
            '	msg &= cl & "  "
            'Next
            'Trace.WriteLine(msg)
            'Check if they are integers
            Dim cnt = courseDataElems.Where(Function(s) Integer.TryParse(s, Nothing)).Count()
            'Either Fractions, Pass marks, Credit Hours
            If cnt > 0 Then
                Dim courseDataElemsValues As List(Of Integer)
                If courseDataElems.Count = (CoursesCount) Then
                    courseDataElemsValues = courseDataElems.ConvertAll(Function(s) Integer.Parse(s)).ToList()
                ElseIf courseDataElems.Count > (CoursesCount * 2) Then
                    courseDataElemsValues = courseDataElems.GetRange(0, CoursesCount * 2).ConvertAll(Function(s) Integer.Parse(s)).ToList()
                Else
                    courseDataElemsValues = courseDataElems.GetRange(0, cnt).ConvertAll(Function(s) Integer.Parse(s)).ToList()
                End If
                If courseDataElemsValues.All(Function(num) num < 10) Then
                    'All numbers are less than 10 ==> Credit Hours
                    'The number of Credit hours must be the same as number of courses
                    ', "Courses count and credit hours count not equal")
                    If Not (courseDataElemsValues.Count = CoursesCount) Then
                        Return
                    End If

                    CreditHours.AddRange(courseDataElemsValues)
                    'Trace.WriteLine("Found Credit Hours ==============>")
                ElseIf courseDataElemsValues.All(Function(num) (num Mod 10) = 0) Then
                    CWEX_Fractions.AddRange(courseDataElemsValues)
                    'Trace.WriteLine("Found Fractions ==============>")
                End If

            End If
        Next

        'If ExamType = 0 Then
        '	FirstSemCoursesExtracted.Add("{")
        '	FirstSemCoursesExtracted.AddRange(CourseCodes)
        '	FirstSemCoursesExtracted.Add("}")
        'End If

        For i As Integer = 0 To CoursesCount - 1
            Dim course As Course = Nothing
            course.CourseCode = CourseCodes(i)
            course.CreditHours = CreditHours(i)
            If 2 * i < CWEX_Fractions.Count Then
                course.ExamFraction = CWEX_Fractions(2 * i + 1)
                course.CWFraction = CWEX_Fractions(2 * i)
            Else
                Trace.WriteLine("SPECIAL RULE INVOKE: SET EXAM 100 CW 0 For CW = 0 EX = 100 For Course: " & course.CourseCode)
                course.ExamFraction = 100
                course.CWFraction = 0
            End If
            course.Semester = Guesser.GetSemesterGuess(TYear, Grade, course.CourseCode)
            If Not Multidisciplinary Then

            End If
            'If CoursesWatchList.Contains(course.CourseCode) Then
            '	WatchCourses.Add(course.CourseCode)
            '	WatchCourses.Add(TargetFile)
            '	WatchCourses.Add(SheetName)
            '	WatchCourses.Add("...........................................................")
            'End If
            CoursesData.Add(course)
        Next

        'Use first headers region to determine where the students data elements are found
        'If SheetName = "Pow" Then
        '	Beep()
        'End If
        Dim headers = ws.ExtractDataFromSubrange(Segmenter.BigHeaderRegions.First)
        Dim colHeaders() As String = {"الرقم", "الرقم الجامعي", "رقم الجلوس", "الاسم"}
        Dim NamePosition As Integer
        Dim IndexPosition As Integer
        Dim UNumberPosition As Integer
        If Segmenter.BigHeaderRegions.Count = 2 Then
            NamePosition = headers.IndexOf(colHeaders(3))
            IndexPosition = headers.IndexOf(colHeaders(2))
            UNumberPosition = headers.IndexOf(colHeaders(1))
            'Trace.WriteLine("Name Column: " & NamePosition)
            'Trace.WriteLine("Index Column: " & IndexPosition)
            'Trace.WriteLine("UNumber Column: " & UNumberPosition)
        Else
            Throw New Exception("Fragmented Headers section probably due to hidden columns or non-uniform coloring")
        End If

        If IndexPosition < 0 Or NamePosition < 0 Then
            Throw New Exception("Suspending Data Extraction no Name And/Or Index Headers")
            Return
        End If
        'Dim msg As String = ""
        'For Each cl In headers
        '	msg &= cl & vbCrLf
        'Next
        'MsgBox(msg)

        'Before collecting students data dteermine if sheet contains multipl disciplines. This is indicated by the presence of NT subjects for
        'every student. We test first few students.
        Dim NTSheet As Boolean = True

        For i As Integer = 0 To System.Math.Min(3, Segmenter.GradeRegions.Count - 1)
            Dim dat = ws.ExtractDataFromSubrange(Segmenter.GradeRegions(i))
            If dat.Contains("NT") Then
                'Multidisciplinary sheet
                Trace.WriteLine("MULTI DISCIPLINARY SHEET ===================///// OUCH!!!")
                If ExamType = 2 Then
                    Exit For
                End If
            Else
                NTSheet = False
            End If
        Next

        If (NTSheet Xor Multidisciplinary) Then
            Throw New Exception("Incorrect naming of sheet for mutidisciplinary")
        End If

        StudentsData = New List(Of Student)(Segmenter.StudentRegions.Count)
        'Analyze students area and extract data
        Dim SCorrectlyConverted As Integer = 0
        Dim SFailedConversion As Integer = 0
        For Each SubRange In Segmenter.StudentRegions
            Dim st As Student
            Dim Data = ws.ExtractDataFromSubrange(SubRange)
            If Data.All(Function(s) String.IsNullOrWhiteSpace(s)) Then
                Dim temp = StudentsData(StudentsData.Count - 1)
                temp.AssociatedRecords += CRange.RangeHeight(SubRange) / 2
                StudentsData(StudentsData.Count - 1) = temp
            Else
                Dim SpecialEnroll As Integer = 0
                st.Name = Data(NamePosition)
                st.SIndex = ProcessIndex(Data(IndexPosition), SpecialEnroll)
                If st.SIndex <> 0 Then
                    SCorrectlyConverted += 1
                Else
                    SFailedConversion += 1
                End If
                st.SpecialEnroll = SpecialEnroll
                If UNumberPosition > 0 Then
                    st.SUNumber = ProcessUNumber(Data(UNumberPosition))
                Else
                    st.SUNumber = Nothing
                End If
                st.AssociatedRecords = CRange.RangeHeight(SubRange) / 2
                StudentsData.Add(st)
            End If
        Next
        If SFailedConversion > SCorrectlyConverted Then
            Trace.WriteLine("Students data parsing failure")
            StudentsData.Clear()
            Dim __stemp = IndexPosition
            IndexPosition = UNumberPosition
            UNumberPosition = __stemp
            For Each SubRange In Segmenter.StudentRegions
                Dim st As Student
                Dim Data = ws.ExtractDataFromSubrange(SubRange)
                If Data.All(Function(s) String.IsNullOrWhiteSpace(s)) Then
                    Dim temp = StudentsData(StudentsData.Count - 1)
                    temp.AssociatedRecords += CRange.RangeHeight(SubRange) / 2
                    StudentsData(StudentsData.Count - 1) = temp
                Else
                    Dim SpecialEnroll As Integer = 0
                    st.Name = Data(NamePosition)
                    st.SIndex = ProcessIndex(Data(IndexPosition), SpecialEnroll)
                    If st.SIndex <> 0 Then
                        SCorrectlyConverted += 1
                    Else
                        SFailedConversion += 1
                    End If
                    st.SpecialEnroll = SpecialEnroll
                    If UNumberPosition > 0 Then
                        st.SUNumber = ProcessUNumber(Data(UNumberPosition))
                    Else
                        st.SUNumber = Nothing
                    End If
                    st.AssociatedRecords = CRange.RangeHeight(SubRange) / 2
                    StudentsData.Add(st)
                End If
            Next
        End If

        If Me.StudentsData.Any(Function(s) s.AssociatedRecords = 0) Then
            Throw New ArgumentException("Associated Records = 0")
        End If

        'msg = ""
        'For Each stud In StudentsData
        '	msg += vbCrLf & stud.SIndex & vbTab & stud.SUNumber & "     " & stud.AssociatedRecords & vbTab & stud.Name
        'Next
        'MsgBox(msg)

        'Analyze the main data region
        'First confirmatory statistics!!
        Dim MinAssociatedRecords As Integer = StudentsData.Min(Of Integer)(Function(st) st.AssociatedRecords)
        Dim MaxAssociatedRecords As Integer = StudentsData.Max(Of Integer)(Function(st) st.AssociatedRecords)
        Trace.WriteLine("Max: " & MaxAssociatedRecords & "     Min:" & MinAssociatedRecords)
        If MinAssociatedRecords = 0 Then
            Throw New ArgumentException("Associated Records = 0")
        End If
        'MinMax.Add(New System.Tuple(Of String, String, Integer, Integer)(TargetFile, SheetName, MinAssociatedRecords, MaxAssociatedRecords))


        Dim recordCounter As Integer = 0
        MarksData = New List(Of List(Of Mark))(StudentsData.Count)
        SummaryData = New List(Of Summary)(StudentsData.Sum(Function(s) s.AssociatedRecords))

        Dim summaryHeaders = ws.ExtractDataFromSubrange(Segmenter.BigHeaderRegions(1))
        Dim summaryColHeaders() As String = {"غياب", "رسوب", "المعدل", "التوصية", "المعدل التراكمي", "التراكمي"}
        Dim AbsentPosition As Integer = -1
        Dim FailedPosition As Integer = -1
        Dim GPAPosition As Integer = -1
        Dim Recomm1Position As Integer = -1
        Dim Recomm2Position As Integer = -1
        Dim CGPAPosition As Integer = -1
        Dim assignedColumns As Integer = 0

        For j = 0 To summaryHeaders.Count - 1
            For i As Integer = 0 To summaryColHeaders.Length - 1
                If SudaneseCompare(summaryHeaders(j), summaryColHeaders(i)) Then
                    Select Case i
                        Case 0
                            AbsentPosition = j
                            assignedColumns += 1
                        Case 1
                            FailedPosition = j
                            assignedColumns += 1
                        Case 2
                            GPAPosition = j
                            assignedColumns += 1
                        Case 3
                            Recomm1Position = j
                            assignedColumns += 1
                        Case 4
                            CGPAPosition = j
                            assignedColumns += 1
                        Case 5
                            CGPAPosition = j
                            assignedColumns += 1
                    End Select
                End If
            Next
        Next

        If (assignedColumns) < summaryHeaders.Count Then
            Recomm2Position = summaryHeaders.IndexOf(Nothing)
        End If

        If GPAPosition < 0 OrElse AbsentPosition < 0 OrElse FailedPosition < 0 Then
            Throw New Exception("Summary Header fields could not be parsed")
        End If

        'HeadersList.Add(summaryHeaders)

        For n As Integer = 0 To StudentsData.Count - 1
            Dim Stud = StudentsData(n)
            'If Stud.AssociatedRecords > 3 Then
            '    Throw New ArgumentOutOfRangeException("AssociatedRecords", "AssociatedRecords exceeded three.")
            'End If
            For i = 1 To Stud.AssociatedRecords
                'Extract Summary Data
                Dim summaryData = ws.ExtractDataFromSubrange(Segmenter.SummaryRegions(recordCounter))
                'Check if all fields are empty. If yes remove from records and ignore
                If summaryData.All(Function(s) String.IsNullOrWhiteSpace(s)) Then
                    Stud.AssociatedRecords -= 1
                    StudentsData(n) = Stud
                    recordCounter += 1
                    Continue For
                End If
                Dim sumry As New Summary()
                sumry.Absent = Integer.Parse(summaryData(AbsentPosition))
                Integer.TryParse(summaryData(FailedPosition), sumry.Fail)
                sumry.GPA = New Nullable(Of Decimal)(0)
                If Not Decimal.TryParse(summaryData(GPAPosition), sumry.GPA) Then
                    sumry.GPA = Nothing
                End If

                If CGPAPosition > -1 Then
                    sumry.CGPA = New Nullable(Of Decimal)(0)
                    If Not Decimal.TryParse(summaryData(CGPAPosition), sumry.CGPA) Then
                        sumry.CGPA = Nothing
                    End If
                End If
                If Recomm1Position > -1 And Recomm2Position > -1 Then
                    sumry.Recomm = summaryData(Recomm2Position)
                    sumry.FinalRecomm = summaryData(Recomm1Position)
                    AddToRecommendationsList(sumry.Recomm)
                    AddToRecommendationsList(sumry.FinalRecomm)
                    If UseTranslator Then
                        sumry.Recomm = Translator.Translate(sumry.Recomm, sumry.GPA)
                        sumry.FinalRecomm = Translator.Translate(sumry.FinalRecomm, sumry.CGPA)
                    End If
                ElseIf Recomm1Position > -1 And Not (Me.Grade = 5) Then
                    sumry.Recomm = summaryData(Recomm1Position)
                    sumry.FinalRecomm = Nothing
                    AddToRecommendationsList(sumry.Recomm)
                    If UseTranslator Then
                        sumry.Recomm = Translator.Translate(sumry.Recomm, sumry.GPA)
                    End If
                ElseIf Recomm1Position > -1 Then
                    sumry.FinalRecomm = summaryData(Recomm1Position)
                    AddToRecommendationsList(sumry.FinalRecomm)
                    If UseTranslator Then
                        sumry.FinalRecomm = Translator.Translate(sumry.FinalRecomm, sumry.CGPA)
                    End If
                End If

                Me.SummaryData.Add(sumry)

                'Extract Mark and Grades
                Dim StudMarks As New List(Of Mark)
                Dim StudMarkData = ws.ExtractDataFromSubrange(Segmenter.MarksRegions(recordCounter))
                Dim StudGradesData = ws.ExtractDataFromSubrange(Segmenter.GradeRegions(recordCounter))

                Dim StudMarkDataStartColumn As Integer = CRange.RangeColumn(Segmenter.MarksRegions(recordCounter))
                Dim StudGradesDataStartColumn As Integer = CRange.RangeColumn(Segmenter.GradeRegions(recordCounter))

                Dim StudMarkDataEndColumn As Integer = StudMarkDataStartColumn + CRange.RangeWidth(Segmenter.MarksRegions(recordCounter)) - 1
                Dim StudGradesDataEndColumn As Integer = StudGradesDataStartColumn + CRange.RangeWidth(Segmenter.GradeRegions(recordCounter)) - 1

                If StudMarkDataStartColumn <> StudGradesDataStartColumn Then
                    Trace.WriteLine("ERROR: Student: " & Stud.Name & " Index: " & Stud.SIndex & " Univ No.: " & Stud.SUNumber & " Has unaligned makrs and grades fields at start")
                    Throw New Exception("Marks and Grades fields misaligned")
                End If
                If StudGradesDataEndColumn <> StudMarkDataEndColumn Then
                    Trace.TraceError("Student: " & Stud.Name & " Index: " & Stud.SIndex & " Univ No.: " & Stud.SUNumber & " Has unaligned makrs and grades fields at start")
                End If

                Dim courseRef As Integer = (StudMarkDataStartColumn - DataAreaStartColumn) / 2
                If StudMarkData.Count <> StudGradesData.Count Then
                    Trace.TraceError("Student: " & Stud.Name & " Index: " & Stud.SIndex & " Univ No.: " & Stud.SUNumber & " Has excess fields in either marks or grades")
                    If (StudMarkData.Count + 1) = StudGradesData.Count And StudGradesData.Last = "AB" Then
                        Trace.WriteLine("CORRECTED: added missing empty exam mark")
                        StudMarkData.Add(Nothing)
                    Else
                        Trace.TraceInformation("ATTEMPT: Taking minimum of Grades and Marks count")
                    End If

                End If

                'Extract the data from the range and add it to the StudMakrs List. If both CW and EX are Nothing Ignore
                'If StudMarkData.Count Mod 2 = 1 Then
                '	StudMarkData.Add(Nothing)
                'End If

                For j As Integer = 0 To System.Math.Min((StudMarkData.Count / 2) - 1, (StudGradesData.Count / 2) - 1)
                    If String.IsNullOrWhiteSpace(StudMarkData(2 * j)) And String.IsNullOrWhiteSpace(StudMarkData(2 * j + 1)) Then
                        If StudGradesData(2 * j + 1) Is Nothing Then
                        ElseIf StudGradesData(2 * j + 1).Contains("AB") Then
                            Dim m As Mark
                            Dim cwmark, exmark As Decimal
                            m.CourseRef = courseRef
                            If Decimal.TryParse(StudMarkData(2 * j), cwmark) Then
                                m.CWMark = cwmark
                            Else
                                m.CWMark = Nothing
                            End If
                            If Decimal.TryParse(StudMarkData(2 * j + 1), exmark) Then
                                m.ExamMark = exmark
                            Else
                                m.ExamMark = Nothing
                            End If
                            m.ExType = i
                            StudMarks.Add(m)
                        End If
                    Else
                        Dim m As Mark
                        Dim cwmark, exmark As Decimal
                        m.CourseRef = courseRef
                        If Decimal.TryParse(StudMarkData(2 * j), cwmark) Then
                            m.CWMark = cwmark
                        Else
                            m.CWMark = Nothing
                        End If
                        If Decimal.TryParse(StudMarkData(2 * j + 1), exmark) Then
                            m.ExamMark = exmark
                        Else
                            m.ExamMark = Nothing
                        End If
                        m.ExType = i
                        If StudGradesData(2 * j + 1) Is Nothing Then
                            'We are here so either exam or CW mark is non-null
                            Trace.TraceError("ERROR: LONE MARK for Student: " & Stud.Name & " Index: " & Stud.SIndex & " Course: " & CourseCodes(courseRef) & _
                              "EX: " & m.ExamMark & " CW: " & m.CWMark)
                        ElseIf Not StudGradesData(2 * j + 1).Contains("NT") Then
                            StudMarks.Add(m)
                        End If
                    End If
                    courseRef += 1
                Next
                MarksData.Add(StudMarks)
                recordCounter += 1
            Next
        Next

        MinAssociatedRecords = StudentsData.Min(Of Integer)(Function(st) st.AssociatedRecords)
        MaxAssociatedRecords = StudentsData.Max(Of Integer)(Function(st) st.AssociatedRecords)
        Trace.WriteLine("Max: " & MaxAssociatedRecords & "     Min:" & MinAssociatedRecords)
        If MinAssociatedRecords = 0 Then
            Throw New ArgumentException("Associated Records = 0")
        End If
        Trace.WriteLine("Students, Courses and Marks Data structures assembled.")
        Trace.WriteLine("Students Count: " & StudentsData.Count)
        Trace.WriteLine("Courses Count: " & CoursesData.Count)
        Trace.WriteLine("Marks records Count: " & MarksData.Count & " Total Fields: " & MarksData.Sum(Function(ml) ml.Count))

        'Assign discipline patterns to each student

        Dim StudentsNTPattern As List(Of Integer) = Nothing
        Dim PatternsDisciplines As Dictionary(Of Integer, Disciplines) = Nothing


        If NTSheet Then
            PatternsDisciplines = New Dictionary(Of Integer, Disciplines)(4)
            StudentsNTPattern = New List(Of Int32)(Segmenter.StudentRegions.Count)

            'Accumulate patterns: take first record for each student form NTPattern and assign to student in StudentsNTPattern. Try to add to dictionary as key.
            'The dictionary at the end will include only the unique patterns.
            recordCounter = 0
            For n As Integer = 0 To StudentsData.Count - 1
                Dim StudGradesData = ws.ExtractDataFromSubrange(Segmenter.GradeRegions(recordCounter))
                Dim spattern As Integer = GetNTPattern(StudGradesData)

                If Not PatternsDisciplines.ContainsKey(spattern) Then
                    PatternsDisciplines.Add(spattern, 0)
                End If
                recordCounter += StudentsData(n).AssociatedRecords
                StudentsNTPattern.Add(spattern)
            Next
        End If

        'We want to make sure sheet is ordered by disciplines. Pass through NT Patterns list, check if pattern is the same as previous if t is good, 
        'otherwise check that it didn't exist befo in the list.
        If NTSheet Then
            For i As Integer = 1 To StudentsNTPattern.Count - 1
                Dim patt = StudentsNTPattern(i)
                If Not (StudentsNTPattern(i) = StudentsNTPattern(i - 1)) Then
                    'Check this pattern didn't exist before
                    If StudentsNTPattern.IndexOf(patt) < i Then
                        'Not good patter existed before. Throw Exception
                        'Throw New NotImplementedException("Not implemeneted for sheets with scrampled departments")
                        Trace.TraceWarning("WARNING scrambled sheets. Not ordered by departments will take order of first occurence")
                        OrderedDisciplines = False
                    End If
                End If
            Next
        End If


        'Determine which patterns correspond to which disciplines
        If NTSheet Then
            Dim dcount As Integer = 1
            Dim PatternsCount As New Dictionary(Of Integer, Integer)
            For Each patt In PatternsDisciplines
                Trace.WriteLine("Discipline " & dcount & " does not take courses: " & GetNTCourses(CourseCodes, patt.Key))
                Dim spatt = patt.Key
                PatternsCount.Add(patt.Key, StudentsNTPattern.LongCount(Function(s) s = spatt))
                Trace.WriteLine("Students Count: " & PatternsCount.Last.Value)
                dcount += 1
            Next
        End If

        If NTSheet Then
            PatternsDisciplines = DecideDisciplines(PatternsDisciplines)
            For Each patt In PatternsDisciplines
                Trace.WriteLine("Disciplines: " & patt.Value.ToString & " Does not take courses " & GetNTCourses(CourseCodes, patt.Key))
            Next
        Else

        End If

        'Assign Disciplines to courses
        For i As Integer = 0 To CoursesData.Count - 1
            If NTSheet Then
                CoursesData(i) = SetCourseDisciplines(CoursesData(i), PatternsDisciplines, i)
            Else
                Dim c = CoursesData(i)
                c.TDisciplines = New List(Of Disciplines)(1)
                c.TDisciplines.Add(DisciplinesInc.First)
                CoursesData(i) = c
            End If
        Next

        'Assign disciplines to students
        For i As Integer = 0 To StudentsData.Count - 1
            If NTSheet Then
                StudentsData(i) = AssignDisciplines(StudentsData(i), NTSheet, PatternsDisciplines, StudentsNTPattern(i))
            Else
                StudentsData(i) = AssignDisciplines(StudentsData(i), NTSheet)
            End If
            Trace.WriteLine("STUDENT: " & StudentsData(i).SIndex & " Sem 1: " & StudentsData(i).Discipline1.ToString())
            If Not Me.FirstSemesterOnly Then
                Trace.WriteLine("STUDENT: " & StudentsData(i).SIndex & " Sem 2: " & StudentsData(i).Discipline2.ToString())
            End If
        Next

        Trace.Unindent()
    End Sub
    Public Function SudaneseCompare(str1 As String, str2 As String) As Boolean
        If str1 = str2 Then
            Return True
        Else
            If String.IsNullOrWhiteSpace(str1) Or String.IsNullOrWhiteSpace(str2) Then
                Return False
            End If
            str1 = str1.Trim()
            str2 = str2.Trim()
            'Here is where Sudanese people have another definition of equality! Sick and convoluted or excess genius
            Dim match = 0, differ As Integer = 0

            If Math.Max(str1.Length, str2.Length) > 2 * Math.Min(str1.Length, str2.Length) Then
                Return False
            End If

            For i As Integer = 0 To Math.Min(str1.Length, str2.Length) - 1
                If str1(i) = str2(i) Then
                    match += 1
                Else
                    differ += 1
                End If
            Next
            If differ < 2 Then
                Return True
            End If
            Return False
        End If
    End Function
    Private Function AssignDisciplines(ByVal student As Student, ByVal NTSheet As Boolean, Optional ByVal PatternsDisciplines As Dictionary(Of Integer, Disciplines) = Nothing, Optional ByVal p3 As Integer = Nothing) As Student
        AssignDisciplines = student
        If NTSheet Then
            AssignDisciplines.Discipline1 = GradedDisciplines(PatternsDisciplines(p3))
            If Not Me.FirstSemesterOnly Then
                AssignDisciplines.Discipline2 = PatternsDisciplines(p3)
            End If
        Else
            AssignDisciplines.Discipline1 = GradedDisciplines(Me.DisciplinesInc(0))
            If Not Me.FirstSemesterOnly Then
                AssignDisciplines.Discipline2 = Me.DisciplinesInc(0)
            End If
        End If
    End Function
    Function SetCourseDisciplines(ByVal course As Course, ByVal PatternsDisciplines As Dictionary(Of Integer, Disciplines), ByVal courseRef As Integer) As Course
        SetCourseDisciplines = course
        SetCourseDisciplines.TDisciplines = New List(Of Disciplines)
        'If course.CourseCode = "EE3108" Then
        '	Beep()
        'End If
        For Each pattP In PatternsDisciplines
            If (((Not pattP.Key) And (1 << courseRef)) <> 0) Then
                If (course.Semester Mod 2) = 0 Then
                    SetCourseDisciplines.TDisciplines.Add(pattP.Value)
                Else
                    SetCourseDisciplines.TDisciplines.Add(GradedDisciplines(pattP.Value))
                End If
            End If
        Next
    End Function
    Public Function GradedDisciplines(ByVal discp As Disciplines) As Disciplines
        Select Case Me.Grade
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
    Private Function DecideDisciplines(ByVal PatternsDisciplines As Dictionary(Of Integer, Disciplines)) As Dictionary(Of Integer, Disciplines)
        Dim PattsDiscps As New Dictionary(Of Integer, Disciplines)
        If PatternsDisciplines.Count <> Me.DisciplinesInc.Count And Not OrderedDisciplines Then
            Throw New Exception("Disciplines found in name and sheet are not equal")
        Else
            For i As Integer = 0 To System.Math.Min(Me.DisciplinesInc.Count - 1, PatternsDisciplines.Count - 1)
                PattsDiscps.Add(PatternsDisciplines.ElementAt(i).Key, Me.DisciplinesInc(i))
            Next
        End If
        Return PattsDiscps
    End Function
    Private Function DecideDisciplines(ByVal PatternsDisciplines As Dictionary(Of Integer, Disciplines), ByVal PatternsCount As Dictionary(Of Integer, Integer), ByVal CourseCodes As List(Of String)) _
     As Dictionary(Of Integer, Disciplines)
        'TODO: Hard coded knowledge about disciplines not very good. Remove
        Select Case Me.Grade
            Case 3
                'Only two possible disciplines: Dept / Soft
                If PatternsDisciplines.Count <> 2 Then
                    Beep()
                    Throw New Exception("")
                End If
                Dim DeptNT = {"EC3140", "EC3139", "EE3221"}
                Dim SoftNT = {"EE3105", "EE3106", "EE3108", "EE3209"}

                Dim patt = PatternsDisciplines.ElementAt(0)
                Dim allNT As Boolean = True
                For Each el In DeptNT
                    allNT = allNT And GetNTCourses(CourseCodes, patt.Key).Contains(el)
                Next
                If allNT Then
                    PatternsDisciplines.Item(patt.Key) = Disciplines.Dept
                    PatternsDisciplines(PatternsDisciplines.ElementAt(1).Key) = Disciplines.Soft
                Else
                    PatternsDisciplines.Item(patt.Key) = Disciplines.Soft
                    PatternsDisciplines(PatternsDisciplines.ElementAt(1).Key) = Disciplines.Dept
                End If
            Case 4
                'Four possible disciplines
                Beep()
                Throw New NotImplementedException()
            Case 5
                'Five possible disciplines
                Beep()
                Throw New NotImplementedException()
            Case Else
                Beep()
                Throw New Exception("This grade can't contain optional courses/ 2005 -- 2010")
        End Select
        Return PatternsDisciplines
    End Function
    Function ProcessIndex(ByVal indexVal As String, ByRef SC As Integer) As Integer
        Dim sindex As Integer
        If Integer.TryParse(indexVal, sindex) Then
            sindex = Integer.Parse(indexVal)
        ElseIf indexVal.Trim().StartsWith("EX") Then
            'External Student
            sindex = Integer.Parse(indexVal.Trim().Substring(2))
            SC = 2
        Else
            Trace.WriteLine("Failed to convert index " & indexVal & " UNUmber ")
            'Throw New ArgumentException("Index of student is not valid: " & indexVal)
            'sindex = Integer.Parse(Trace.ReadLine())
            Return Nothing
        End If
        Return sindex
    End Function
    Function ProcessUNumber(ByVal unum As String) As String
        Dim unumber As String = Nothing
        If unum Is Nothing Then
        ElseIf unum.Contains("-") Then
            Dim unum_y As String = unum.Split("-")(0).Trim()
            Dim unum_n As String = unum.Split("-")(1).Trim()
            If unum_n.Length < 7 Then
                unum_n = unum_n
            End If
            unumber = unum_y & "-" & unum_n
        ElseIf unum.Length > 3 Then
            'Assume nnnnnn-yy Format
            Dim unum_y As String = unum.Substring(unum.Length - 2)
            Dim unum_n As String = unum.Substring(0, unum.Length - 2)
            unum_n = unum_n.Trim()
            If Integer.Parse(unum_y) > 11 Then
                'Incorrect assumption for format. Do not use the UNumber Fields
                unum_n = Nothing
                unum_y = Nothing
            End If
            unumber = unum_y & "-" & unum_n
        End If
        Return unumber
    End Function

    Private Function GetNTPattern(ByVal list As List(Of String)) As Integer
        Dim sum As Integer = 0
        For i As Integer = 0 To (list.Count / 2) - 1
            If list(2 * i + 1) = "NT" Then
                sum += (1 << i)
            End If
        Next
        Return sum
    End Function
    Private Function ConsolidateSubRanges(ByVal SubRangesToBeConsolidated As IEnumerable(Of String)) As String
        Dim startCol As Integer = SubRangesToBeConsolidated.Min(Function(s) CRange.RangeColumn(s))
        Dim endCol As Integer = SubRangesToBeConsolidated.Max(Function(s) CRange.RangeColumn(s) + CRange.RangeWidth(s)) - 1
        Return CColumn.GetColumnName(startCol) & CRange.RangeRow(SubRangesToBeConsolidated.First) & ":" & CColumn.GetColumnName(endCol) & CRange.RangeRow(SubRangesToBeConsolidated.First)
    End Function
    Private Function GetNTCourses(ByVal ccodes As List(Of String), ByVal pat As Integer) As String
        Dim rcstr As String = ""
        For i As Integer = 0 To ccodes.Count - 1
            If ((1 << i) And (pat)) Then
                rcstr &= ccodes(i) & "   "
            End If
        Next
        Return rcstr
    End Function

    Private Sub AddToRecommendationsList(Recomm As String)
        If String.IsNullOrWhiteSpace(Recomm) Then
            Exit Sub
        End If
        Dim recstr = Recomm.Trim()
        Dim z As Integer
        If Not Recomms.TryGetValue(recstr, z) Then
            Recomms(recstr) = 0
        Else
            Recomms(recstr) += 1
        End If
    End Sub

End Class

Class SemesterGuessHelper
    Private Semesters As List(Of List(Of String))
    Public Sub New()
        Semesters = {
         {"EM1203", "AR1101", "CH1101", "EC1101", "PH1101", "SD1101", "EM1102"}.ToList,
          {"EM2105", "ME2101", "ME2102", "EE2101", "EE2102", "EE2103", "EC2105", "EC2106", "EC2107", "EN2201", "AR2101"}.ToList(),
          {"EM3209", "EE3209", "EE3106", "EE3107", "EC3113", "EC3114", "EC3115"}.ToList(),
          {"AD4202", "SC4104", "SC4105", "SC4208", "EE4110", "EC4121", "EC4122", "EC4123", "AD4202", "SC4104", "SC4105", "SC4208", "EC4130", "EC4121", "EC4122", "EC4123"}.ToList(),
          {"AD5103", "SC5211", "EC5126", "SC5112", "SC5216", "SC5114", "AD5103", "SC5211", "EC5126", "EC5130", "EC5128", "EC5129", "AD5103", "SC5211", "SC5121", "SC5122", "SC5123", "SC5124",
        "AD5103", "SC5211", "EE5114", "EE5116", "EE5218", "EE5117", "AD5103", "SC5211", "EC5142", "EC5143", "EC5144", "EC5149"}.ToList()
         }.ToList()
    End Sub
    Function GetSemesterGuess(ByVal TYear As Integer, ByVal Grade As Integer, ByVal CourseCode As String) As Integer
        If TYear = 2005 Then
            If CourseCode = "EE3106" Or CourseCode = "EE3209" Or CourseCode = "EE3108" Then
                Return 6
            End If
        End If
        If TYear = 2007 Then
            If CourseCode = "EE3105" Or CourseCode = "EE3108" Then
                Return 6
            End If
        End If
        If Semesters(Grade - 1).IndexOf(CourseCode) < 0 Then
            Return Grade * 2
        Else
            Return Grade * 2 - 1
        End If

    End Function
End Class

Public Class RecommendationsTraslator
    Public Shared EmptyToSpecial As Integer = 0
    Dim lookup As New SortedDictionary(Of String, String)
    Sub New(filepath As String)
        Dim str As New IO.StreamReader(filepath)
        While Not str.EndOfStream()
            Dim txt = str.ReadLine()
            Dim dat() As String = txt.Split(",")
            'If dat(0) = "فصل" Then
            '    Beep()
            'End If
            lookup(dat(0).Trim()) = dat(1).Trim()
        End While
        str.Close()
    End Sub
    Sub New(dict As Dictionary(Of String, String))
        lookup = New SortedDictionary(Of String, String)(dict)
    End Sub

    Function Translate(recstr As String, GPA? As Decimal) As String
        If String.IsNullOrWhiteSpace(recstr) Then
            EmptyToSpecial += 1
            Return "Special Case"
        End If
        If Not lookup.ContainsKey(recstr.Trim()) And recstr.Contains("ملحق") Then
            Return "Supp."
        End If
        'If recstr.Trim() = "II" Then
        '    If GPA >= 6.5 And GPA <= 7.0 Then
        '        Return "II-1"
        '    Else
        '        Return "II-2"
        '    End If
        'End If
        Dim rec As String = Nothing
        If lookup.TryGetValue(recstr.Trim(), rec) Then
            Return rec
        Else
            Return recstr.Trim()
        End If
        'If lookup(recstr.Trim()) = "Exception" Then
        '    Throw New Exception()
        'Else
        '    Dim rec As String = Nothing
        '    If lookup.TryGetValue(recstr.Trim(), rec) Then
        '        Return rec
        '    Else
        '        'This translation is not available, suppress translation
        '        Throw New Exception("Incomplete translation records")
        '    End If
        'End If
    End Function
    Function GetRecommendationsList() As List(Of String)
        Return lookup.Values.ToList()
    End Function
End Class


'Check alignment of Grades and Marks sub-records.
'If StudMarkDataStartColumn <> StudGradesDataStartColumn Then
'	'We have trouble. Could be because:
'	'1. First subject has no coursework diff --> 1
'	'2. First subject is NT then difference --> 2  (Not likely it appears first subjects are arranged as to be taken by all)
'	'3. Absent in first set of subjects difference is at least 2
'	Trace.WriteLine("ERROR: Student: " & Stud.Name & " Index: " & Stud.SIndex & " Univ No.: " & Stud.SUNumber & " Has unaligned makrs and grades fields at start")
'	'Loop through subjects if NT or AB add two nothings at the beginning of the list
'	If (StudMarkDataStartColumn - StudGradesDataStartColumn) = 1 Then
'		StudMarkData.Insert(0, Nothing)
'		Trace.WriteLine("CORRECTED: Added single padding for non-coursework subject")
'	Else
'		Dim Pads As Integer = 0
'		For k As Integer = 0 To ((StudMarkDataStartColumn - StudGradesDataStartColumn) / 2 - 1)
'			If StudGradesData(2 * k + 1) = "NT" Or StudGradesData(2 * k + 1).StartsWith("AB") Then
'				StudMarkData.InsertRange(0, {Nothing, Nothing})
'				Pads += 1
'			End If
'		Next
'		Trace.WriteLine("CORRECTED: Padded beginning of marks data with nulls. NUMBER: " & Pads)
'	End If
'End If