Imports DetailedResultsImporter
Imports EntityFramework.BulkInsert.Extensions

Public Class ImporterDialog

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DataArea.DataContext = SharedState.GetSingleInstance
    End Sub

    Private Sub ExcelFileBrowseButton_Click(sender As Object, e As RoutedEventArgs)
        Dim openFileDialog As New Forms.OpenFileDialog()
        openFileDialog.Filter = "Excel OpenXML Document (*.xlsx) |*.xlsx"
        If openFileDialog.ShowDialog() = Forms.DialogResult.OK Then
            ExcelFilePathTextBox.Text = openFileDialog.FileName
        End If
    End Sub

    Private Sub LogRecommButton_Click(sender As Object, e As RoutedEventArgs)
        Dim saveFileDialog As New Forms.SaveFileDialog()
        saveFileDialog.Filter = "Text Document (*.txt) |*.txt"
        If saveFileDialog.ShowDialog() = Forms.DialogResult.OK Then
            LogRecommPathTextBox.Text = saveFileDialog.FileName
        End If
    End Sub

    'Private Sub TranslatorButton_Click(sender As Object, e As RoutedEventArgs)
    '    Dim openFileDialog As New Forms.OpenFileDialog()
    '    openFileDialog.Filter = "Text Document (*.txt) |*.txt"
    '    If openFileDialog.ShowDialog() = Forms.DialogResult.OK Then
    '        TranslatorTextBox.Text = openFileDialog.FileName
    '    End If
    'End Sub

    Private Sub StartImportButton_Click(sender As Object, e As RoutedEventArgs)
        If bgndWorker.IsBusy Then
            MsgBox("Import already in progress. Wait till it is finished!!!")
            Exit Sub
        End If
        If Not IO.File.Exists(ExcelFilePathTextBox.Text) Then
            Return
        End If
        Trace.Listeners.Add(New RichTextBoxTraceListener(LogRichTextBox))
        Filename = ExcelFilePathTextBox.Text
        CResultsDataExtractor.UseTranslator = True 'TranslatorCheckBox.IsChecked
        If CResultsDataExtractor.UseTranslator Then
            'If IO.File.Exists(TranslatorTextBox.Text) Then
            '    CResultsDataExtractor.Translator = New RecommendationsTraslator(TranslatorTextBox.Text)
            'Else
            '    Return
            'End If
            CResultsDataExtractor.Translator = New RecommendationsTraslator(GetTransDictionary())
        End If
        SuppressFeeder = SuppressFeedersCheckBox.IsChecked
        bgndWorker.RunWorkerAsync()
    End Sub

    Function GetTransDictionary() As Dictionary(Of String, String)
        SharedState.DBContext.RecommendationTypes.ToList()
        SharedState.DBContext.RecommTranslations.ToList()
        Return SharedState.DBContext.RecommTranslations.ToDictionary(Of String, String)(Function(s) s.ResText, Function(q) q.RecommendationType.ShortNameEnglish)
    End Function

    'Function GetTransDictionary() As Dictionary(Of String, String)
    '    Dim tmp = New Dictionary(Of String, String)()
    '    If My.Settings.Trans_Key.Count = My.Settings.Trans_Values.Count Then
    '        For i As Integer = 0 To My.Settings.Trans_Key.Count - 1
    '            tmp(My.Settings.Trans_Key(i)) = My.Settings.Trans_Values(i)
    '        Next
    '    Else
    '        Throw New InvalidOperationException("Keys and values do not match")
    '    End If
    '    Return tmp
    'End Function

    Dim WithEvents bgndWorker As New System.ComponentModel.BackgroundWorker()
    Dim Filename As String
    Dim SuppressFeeder As Boolean = False

    Sub bgndWorker_Progress(sender As Object, e As ComponentModel.ProgressChangedEventArgs) Handles bgndWorker.ProgressChanged
        MainProgressBar.Maximum = 100
        MainProgressBar.Value = e.ProgressPercentage
    End Sub

    Sub bgndWorker_Completed(sender As Object, e As ComponentModel.RunWorkerCompletedEventArgs) Handles bgndWorker.RunWorkerCompleted
        Trace.Listeners.Clear()
    End Sub

    Sub ImportExcelFilebackground(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles bgndWorker.DoWork
        Dim worker As ComponentModel.BackgroundWorker = CType(sender, ComponentModel.BackgroundWorker)
        worker.WorkerReportsProgress = True
        Dim wb As DetailedResultsImporter.CWorkbook = Nothing
        Dim ts = Now
        Try
            wb = New CWorkbook(Filename, False)
            For Each SheetName In ImporterDialog.SelectDetailsSheets(wb.GetWorksheetNames())
                'DisciplinesLabel.Content += SheetName + "   "
            Next
            Dim DetailsSheets = ImporterDialog.SelectDetailsSheets(wb.GetWorksheetNames())
            For Each sheet In DetailsSheets
                Dim ws As CWorksheet = wb.GetWorksheet(sheet)

                Dim analyzer As CWorksheetStructureAnalyzer = New CWorksheetStructureAnalyzer(ws)

                Dim segmenter As CResultsSegmeter = New CResultsSegmeter(ws, analyzer)

                Dim extractor As CResultsDataExtractor = New CResultsDataExtractor(ws, segmenter, SharedState.GetSingleInstance.YearID, SharedState.GetSingleInstance.GradeID, 1)

                If CResultsDataExtractor.Recomms.Keys.Any(Function(s) Not SharedState.DBContext.RecommTranslations.Local.Any(Function(q) q.ResText = s)) Then
                    ' There are unknown translations

                    SharedState.DBContext.RecommTranslations.ToList()
                    Dim NoTransList = CResultsDataExtractor.Recomms.Keys.Where(Function(s) Not SharedState.DBContext.RecommTranslations.Local.Any(Function(q) q.ResText = s))

                    For Each x In NoTransList
                        SharedState.DBContext.RecommTranslations.Add(New RecommTranslation() With {.ResText = x})
                    Next
                    Me.Dispatcher.Invoke(New Action(Of Integer)(Sub(s)
                                                                    Dim transDialog As New TranslatorDialog()
                                                                    transDialog.ShowDialog()
                                                                End Sub), 0)
                    SharedState.DBContext.SaveChanges()

                    For Each sumry In extractor.SummaryData.Where(Function(s) NoTransList.Contains(s.Recomm))
                        sumry.Recomm = CResultsDataExtractor.Translator.Translate(sumry.Recomm, sumry.GPA)
                        If sumry.FinalRecomm IsNot Nothing AndAlso sumry.CGPA IsNot Nothing Then
                            sumry.FinalRecomm = CResultsDataExtractor.Translator.Translate(sumry.FinalRecomm, sumry.CGPA)
                        End If
                    Next
                End If
                If CResultsDataExtractor.Recomms.Keys.Any(Function(s) Not SharedState.DBContext.RecommTranslations.Local.Any(Function(q) q.ResText = s)) Then
                    Throw New Exception()
                End If

                If Not SuppressFeeder Then
                    ' Feed Data to database
                    FeedDataToDatabase(extractor, worker)
                End If
            Next
        Finally
            wb.Close()
        End Try
        Dim te = Now()

        MsgBox((te - ts).TotalSeconds.ToString)
    End Sub


    Private Sub FeedDataToDatabase(extractor As CResultsDataExtractor, worker As ComponentModel.BackgroundWorker)
        Using db As New AcademicResultsDBEntities(SharedState.DBContext.Database.Connection, False)

            If Not extractor.FirstSemesterOnly And extractor.ExamType = 1 Then
                Dim ty = (From y In db.TimeYears
                         Where y.Id = extractor.TYear Select y).SingleOrDefault()
                If ty Is Nothing Then
                    ty = New TimeYear() With {.Id = extractor.TYear, .NameArabic = extractor.TYear.ToString + "/" + (extractor.TYear + 1).ToString(),
                                             .NameEnglish = extractor.TYear.ToString + "/" + (extractor.TYear + 1).ToString()}
                    db.TimeYears.Add(ty)
                End If

                Dim bt = (From b In db.Batches
                         Where b.YearId = extractor.TYear And b.GradeId = extractor.Grade).SingleOrDefault()
                If bt Is Nothing Then
                    bt = New Batch() With {.YearId = extractor.TYear, .GradeId = extractor.Grade}
                    db.Batches.Add(bt)
                End If

                Dim sbt1 = (From sb In db.SemesterBatches
                           Where sb.YearId = extractor.TYear And sb.GradeId = extractor.Grade And sb.SemesterId = 1).SingleOrDefault()
                If sbt1 Is Nothing Then
                    sbt1 = New SemesterBatch() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 1}
                    db.SemesterBatches.Add(sbt1)
                End If

                Dim sbt2 = (From sb In db.SemesterBatches
                           Where sb.YearId = extractor.TYear And sb.GradeId = extractor.Grade And sb.SemesterId = 2).SingleOrDefault()
                If sbt2 Is Nothing Then
                    sbt2 = New SemesterBatch() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 2}
                    db.SemesterBatches.Add(sbt2)
                End If

                'Add Students
                For Each stud In extractor.StudentsData
                    Dim st = (From s In db.Students Where s.Index = stud.SIndex).SingleOrDefault()
                    If st Is Nothing Then
                        st = New Student() With {.Index = stud.SIndex, .UnivNo = stud.SUNumber, .NameArabic = stud.Name, .NameEnglish = stud.Name}
                        db.Students.Add(st)
                    End If
                Next
                db.SaveChanges()


                'Add Disciplines
                For Each disc In extractor.DisciplinesInc
                    Dim disc1 = extractor.GradedDisciplines(disc)
                    Dim dsc1 = (From d In db.OfferedDisciplines
                               Where d.YearId = extractor.TYear And d.GradeId = extractor.Grade And d.SemesterId = 1 And d.DisciplineId = disc1).SingleOrDefault()
                    If dsc1 Is Nothing Then
                        dsc1 = New OfferedDiscipline() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 1, .DisciplineId = disc1}
                        db.OfferedDisciplines.Add(dsc1)
                    End If
                    Dim dsc2 = (From d In db.OfferedDisciplines
                              Where d.YearId = extractor.TYear And d.GradeId = extractor.Grade And d.SemesterId = 2 And d.DisciplineId = disc).SingleOrDefault()
                    If dsc2 Is Nothing Then
                        dsc2 = New OfferedDiscipline() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 2, .DisciplineId = disc}
                        db.OfferedDisciplines.Add(dsc2)
                    End If
                Next
                db.SaveChanges()

                For Each crs In extractor.CoursesData
                    Dim SemID = 0
                    If ((crs.Semester Mod 2) = 0) Then
                        SemID = 2
                    Else
                        SemID = 1
                    End If

                    Dim cr = (From xcr In db.Courses
                               Where xcr.CourseCode = crs.CourseCode).SingleOrDefault()
                    If cr Is Nothing Then
                        Throw New Exception("Course Not already in Database this should not happen")
                    End If

                    Dim cof = (From xcrs In db.OfferedCourses
                               Where xcrs.YearId = extractor.TYear And xcrs.GradeId = extractor.Grade And xcrs.CourseId = cr.Id).SingleOrDefault()
                    If cof Is Nothing Then
                        cof = New OfferedCourse() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = SemID, .CourseId = cr.Id}
                        db.OfferedCourses.Add(cof)
                    End If
                    With cof
                        .CreditHours = crs.CreditHours
                        .ExamFraction = crs.ExamFraction
                        .CourseWorkFraction = crs.CWFraction
                    End With

                    For Each disc In crs.TDisciplines
                        If SemID = 1 Then
                            disc = extractor.GradedDisciplines(disc)
                        End If
                        Dim cds = (From cd In db.CourseDisciplines
                               Where cd.YearId = extractor.TYear And cd.GradeId = extractor.Grade And cd.SemesterId = SemID And cd.CourseId = cr.Id And disc = cd.DisciplineId).SingleOrDefault()
                        If cds Is Nothing Then
                            cds = New CourseDiscipline() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = SemID, .CourseId = cr.Id, .DisciplineId = disc}
                            db.CourseDisciplines.Add(cds)
                        End If
                        With cds
                            .Optional = False
                        End With
                    Next
                Next
                db.SaveChanges()



                For i As Integer = 0 To extractor.StudentsData.Count - 1
                    Dim stud = extractor.StudentsData(i)
                    Dim st = (From s In db.Students Where s.Index = stud.SIndex).SingleOrDefault()
                    If st Is Nothing Then
                        st = New Student() With {.Index = stud.SIndex, .UnivNo = stud.SUNumber, .NameArabic = stud.Name, .NameEnglish = stud.Name}
                        db.Students.Add(st)
                    End If
                    Dim enr = (From ben In db.BatchEnrollments
                              Where ben.YearId = extractor.TYear And ben.GradeId = extractor.Grade And ben.StudentId = st.Id).SingleOrDefault()
                    If enr Is Nothing Then
                        enr = New BatchEnrollment() With {.TimeYear = ty, .GradeId = extractor.Grade, .StudentId = st.Id}
                        If stud.SpecialEnroll = 0 Then
                            enr.EnrollmentTypeId = 1
                        ElseIf stud.SpecialEnroll = 2 Then
                            enr.EnrollmentTypeId = 3
                        End If
                        db.BatchEnrollments.Add(enr)
                    End If

                    Dim senr1 = (From sen In db.SemesterBatchEnrollments
                                 Where sen.YearId = extractor.TYear And sen.GradeId = extractor.Grade And sen.SemesterId = 1 And sen.StudentId = st.Id).SingleOrDefault()
                    If senr1 Is Nothing Then
                        senr1 = New SemesterBatchEnrollment() With {.TimeYear = ty, .GradeId = extractor.Grade, .Student = st, .SemesterId = 1, .DisciplineId = stud.Discipline1}
                        db.SemesterBatchEnrollments.Add(senr1)
                    End If
                    Dim senr2 = (From sen In db.SemesterBatchEnrollments
                                 Where sen.YearId = extractor.TYear And sen.GradeId = extractor.Grade And sen.SemesterId = 2 And sen.StudentId = st.Id).SingleOrDefault()
                    If senr2 Is Nothing Then
                        senr2 = New SemesterBatchEnrollment() With {.TimeYear = ty, .GradeId = extractor.Grade, .Student = st, .SemesterId = 2, .DisciplineId = stud.Discipline2}
                        db.SemesterBatchEnrollments.Add(senr2)
                    End If
                Next
                db.SaveChanges()

                db.Configuration.AutoDetectChangesEnabled = False
                db.Configuration.ValidateOnSaveEnabled = False

                If CResultsDataExtractor.UseTranslator Then
                    db.RecommendationTypes.ToList()
                End If

                For i As Integer = 0 To extractor.StudentsData.Count - 1
                    Dim stud = extractor.StudentsData(i)
                    Dim st = (From s In db.Students Where s.Index = stud.SIndex).SingleOrDefault()


                    Dim summry = extractor.SummaryData(i)

                    Dim gpawr = (From gp In db.GPAwRecomms
                                 Where gp.YearId = extractor.TYear And gp.GradeId = extractor.Grade And gp.StudentId = st.Id).SingleOrDefault()
                    If gpawr Is Nothing Then
                        gpawr = New GPAwRecomm() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .StudentId = st.Id}
                        db.GPAwRecomms.Add(gpawr)
                    End If
                    With gpawr
                        .GPA = summry.GPA
                        If CResultsDataExtractor.UseTranslator Then
                            Dim recType = (From ex In db.RecommendationTypes.Local
                                       Where ex.ShortNameEnglish = summry.Recomm).Single()
                            .YearRecommId = recType.Id
                        End If
                        If summry.CGPA.HasValue Or Not String.IsNullOrWhiteSpace(summry.FinalRecomm) Then
                            .CGPA = summry.CGPA
                            .CumulativeRecommendationType = (From ex In db.RecommendationTypes.Local
                                       Where ex.ShortNameEnglish = summry.Recomm).Single()
                        End If
                    End With


                    Dim mks = extractor.MarksData(i)
                    For Each mk In mks
                        Dim crs = (extractor.CoursesData(mk.CourseRef))
                        If ((crs.Semester Mod 2) = 1) Then
                            ' First Semester mark
                            Dim cr = (From xcr In db.Courses
                              Where xcr.CourseCode = crs.CourseCode).SingleOrDefault()
                            Dim cenr = (From cen In db.CourseEnrollments
                                        Where cen.YearId = extractor.TYear And cen.GradeId = extractor.Grade And cen.SemesterId = 1 And cen.StudentId = st.Id And cen.CourseId = cr.Id).SingleOrDefault()
                            If cenr Is Nothing Then
                                cenr = New CourseEnrollment() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 1, .CourseId = cr.Id, .StudentId = st.Id}
                                db.CourseEnrollments.Add(cenr)
                            End If
                            Dim excwmk = (From cen In db.MarksExamCWs
                                          Where cen.YearId = extractor.TYear And cen.GradeId = extractor.Grade And cen.SemesterId = 1 And cen.StudentId = st.Id And cen.CourseId = cr.Id).SingleOrDefault()
                            If excwmk Is Nothing Then
                                excwmk = New MarksExamCW() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 1, .CourseId = cr.Id, .StudentId = st.Id}
                                db.MarksExamCWs.Add(excwmk)
                            End If
                            With excwmk
                                .CWMark = mk.CWMark
                                .ExamMark = mk.ExamMark
                                .Present = mk.ExamMark.HasValue
                                '.ExamTypeId = 0
                            End With
                        Else
                            'Second Semester Mark
                            Dim cr = (From xcr In db.Courses
                             Where xcr.CourseCode = crs.CourseCode).SingleOrDefault()

                            Dim cenr = (From cen In db.CourseEnrollments
                                        Where cen.YearId = extractor.TYear And cen.GradeId = extractor.Grade And cen.SemesterId = 2 And cen.StudentId = st.Id And cen.CourseId = cr.Id).SingleOrDefault()
                            If cenr Is Nothing Then
                                cenr = New CourseEnrollment() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 2, .CourseId = cr.Id, .StudentId = st.Id}
                                db.CourseEnrollments.Add(cenr)
                            End If
                            Dim excwmk = (From cen In db.MarksExamCWs
                                          Where cen.YearId = extractor.TYear And cen.GradeId = extractor.Grade And cen.SemesterId = 2 And cen.StudentId = st.Id And cen.CourseId = cr.Id).SingleOrDefault()
                            If excwmk Is Nothing Then
                                excwmk = New MarksExamCW() With {.YearId = extractor.TYear, .GradeId = extractor.Grade, .SemesterId = 2, .CourseId = cr.Id, .StudentId = st.Id, _
                                                                 .CWMark = mk.CWMark, .ExamMark = mk.ExamMark, .Present = mk.ExamMark.HasValue}
                                db.MarksExamCWs.Add(excwmk)
                            End If
                            With excwmk
                                .CWMark = mk.CWMark
                                .ExamMark = mk.ExamMark
                                .Present = mk.ExamMark.HasValue
                                '.ExamTypeId = 0
                            End With
                        End If
                    Next

                    'If (i Mod 10) = 0 Then
                    '    db.SaveChanges()
                    'End If
                    worker.ReportProgress(100 * (i / extractor.StudentsData.Count) + 1)
                Next
                db.SaveChanges()
            End If
        End Using
    End Sub

    Shared sheetNames() As String = {"Dept", "Tele", "Elec", "Pow", "Soft", "Cont"}
    Public Shared Function SelectDetailsSheets(ByVal SheetNamesCollection As List(Of String)) As List(Of String)
        Dim SelectedSheets As New List(Of String)
        For Each sheet In SheetNamesCollection
            Dim sheetAttr As Integer = -1
            For i As Integer = 0 To sheetNames.Count - 1
                If sheet.Contains(sheetNames(i)) Then
                    sheetAttr = i
                    Exit For
                End If
            Next
            Select Case sheet.Last()
                Case "S"
                    'Console.WriteLine(" Summary - [IGNORED]")
                Case "B"
                    'Console.WriteLine(" Board - [IGNORED]")
                Case Else
                    If sheetAttr < 0 Or sheetAttr > 5 Then
                        'Console.WriteLine(" IGNORED")
                    Else
                        If sheet.Last() = sheetNames(sheetAttr).Last() Then
                            'Console.WriteLine(" Detailed ACCEPTED")
                            SelectedSheets.Add(sheet)
                        Else
                            'Console.WriteLine(" IGNORED")
                        End If
                    End If
            End Select
        Next
        Return SelectedSheets
    End Function

    Class RichTextBoxTraceListener
        Inherits TraceListener
        Public Overloads Overrides Sub Write(message As String)
            _rtb.Dispatcher.Invoke(New Action(Of String)(Sub(s)
                                                             _rtb.AppendText(s)
                                                         End Sub), message)
        End Sub

        Public Overloads Overrides Sub WriteLine(message As String)
            _rtb.Dispatcher.Invoke(New Action(Of String)(Sub(s)
                                                             _rtb.AppendText(s + vbLf)
                                                         End Sub), message)
        End Sub
        Private _rtb As RichTextBox
        Sub New(rtb As RichTextBox)
            _rtb = rtb
        End Sub
    End Class

End Class
