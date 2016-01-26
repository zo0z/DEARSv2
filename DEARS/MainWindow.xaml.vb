Imports System.Data.Entity
Imports System.ComponentModel

Class MainWindow

    'Dim db As AcademicResultsDBEntities
    Dim ViewDictionary As New Dictionary(Of String, UserControl)

    Private TimeYearsViewSource As CollectionViewSource
    Private SemestersViewSource As CollectionViewSource
    Private SharedStateInstance As SharedState = SharedState.GetSingleInstance()
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ViewDictionary("Disciplines") = New DisciplinesScreen()
        ViewDictionary("Courses") = New CoursesScreen()
        ViewDictionary("Teachers") = New TeachersScreen()
        ViewDictionary("CoursesOffered") = New CoursesOfferedScreen()
        ViewDictionary("LoadDistribution") = New LoadDistributionScreen()
        ViewDictionary("StudentEnrollment") = New StudentEnrollmentScreen()
        ViewDictionary("DisciplinesEnrollment") = New DisciplineEnrollmentScreen()
        ViewDictionary("CourseEnrollment") = New CourseEnrollmentScreen()

        ViewDictionary("CourseworkMarks") = New CourseWorkMarksScreen()
        ViewDictionary("ExamAttendance") = New ExamAttendanceScreen()
        ViewDictionary("ExamExcuse") = New ExamExcuseScreen()
        ViewDictionary("ExamMarks") = New ExamMarksScreen()
        ViewDictionary("MeetingResults") = New MeetingResultsScreen()
        ViewDictionary("BoardResults") = New BoardResultsScreen()
        ViewDictionary("PassedFailedLists") = New PassedFailedListsScreen()

        ViewDictionary("TimeYears") = New TimeYearsScreen()
        ViewDictionary("AcademicClasses") = New AcademicClassesScreen()
        ViewDictionary("Semesters") = New SemestersScreen()
        ViewDictionary("Batches") = New BatchesScreen()
        ViewDictionary("SemesterBatches") = New SemesterBatchesScreen()

        ViewDictionary("OfferedDisciplines") = New OfferedDisciplinesScreen()

        ViewDictionary("Transcripts") = New TranscriptScreen()
        ViewDictionary("Students") = New StudentScreen()

        ViewDictionary("DisciplineCurriculum") = New DisciplineCurriculumScreen()
        ViewDictionary("DatabaseManagement") = New ManageDatabase()
        ViewDictionary("TimelineManagement") = New TimelineManagementScreen()



        TimeYearsViewSource = CType(Me.FindResource("TimeYearsViewSource"), CollectionViewSource)
        SemestersViewSource = CType(Me.FindResource("SemestersViewSource"), CollectionViewSource)

    End Sub
    Sub RefreshMainView(sender As Object, e As PropertyChangedEventArgs)
       
        If MainArea.Content IsNot Nothing Then
            CType(ViewDictionary(selectedbtn.Tag), IBaseScreen).LoadData(e.PropertyName)
        End If
    End Sub
    Dim selectedbtn As RadioButton
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim btn = CType(sender, RadioButton)
        If ViewDictionary.ContainsKey(btn.Tag) Then
            MainArea.Content = ViewDictionary(btn.Tag)
            'If CType(ViewDictionary(btn.Tag), IBaseScreen).DBContext Is Nothing Then
            '    CType(ViewDictionary(btn.Tag), IBaseScreen).DBContext = db
            'End If
        End If
        selectedbtn = sender
    End Sub
    Public Sub ReloadData()
        TimeYearsViewSource.Source = SharedState.DBContext.TimeYears.ToList()
        SemestersViewSource.Source = SharedState.DBContext.Semesters.ToList()
    End Sub
    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs)
        SharedState.DBContext.SaveChanges()
    End Sub

    Private Sub DiscardButton_Click(sender As Object, e As RoutedEventArgs)
        Dim benr = New BatchEnrollment() With {.StudentId = 101}
        
        RollBack()
        CType(ViewDictionary(selectedbtn.Tag), IBaseScreen).LoadData("")
    End Sub


    Public Sub RollBack()
        Dim context = SharedState.DBContext
        Dim changedEntries = context.ChangeTracker.Entries().Where(Function(x) x.State <> EntityState.Unchanged).ToList()

        For Each entry In changedEntries.Where(Function(x) x.State = EntityState.Modified)
            entry.CurrentValues.SetValues(entry.OriginalValues)
            entry.State = EntityState.Unchanged
        Next

        For Each entry In changedEntries.Where(Function(x) x.State = EntityState.Added)
            entry.State = EntityState.Detached
        Next

        For Each entry In changedEntries.Where(Function(x) x.State = EntityState.Deleted)
            entry.State = EntityState.Unchanged
        Next

    End Sub

    Private Sub RadioButton_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
        If SharedState.DBContext.ChangeTracker.HasChanges Then
            'CType(ViewDictionary(selectedbtn.Tag), IBaseScreen).CancelEdit()
            If MsgBox("There are unsaved changes navigating without saving them will cause loss of changes. Do you want to navigate?", MsgBoxStyle.YesNo, "Unsaved Changes") _
                = MsgBoxResult.Yes Then
                RollBack()
                CType(ViewDictionary(selectedbtn.Tag), IBaseScreen).LoadData("")
                CType(sender, RadioButton).IsChecked = True
            Else
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub SaveToExcelButton_Click(sender As Object, e As RoutedEventArgs)
        Dim dgs As IEnumerable(Of DataGrid) = FindVisualChildren(Of DataGrid)(Me.MainArea)
        Dim SaveDialog As New Windows.Forms.SaveFileDialog()
        SaveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx"
        If SaveDialog.ShowDialog() = Forms.DialogResult.OK Then
            Dim SheetName As String = selectedbtn.Tag
            If selectedbtn.Tag = "CourseEnrollment" Then
                Dim crs = (From cr In SharedState.DBContext.Courses.Local Where cr.Id = SharedState.GetSingleInstance.CourseID Select cr).Single
                SheetName = SharedState.GetSingleInstance.YearID & "-" & SharedState.GetSingleInstance.GradeID & "-" & crs.TitleEnglish & "-" & crs.CourseCode
            End If
            ExcelExporter.ExportData(dgs.First, SaveDialog.FileName, SheetName)
            MsgBox("Done!")
        End If
    End Sub
    Public Iterator Function FindVisualChildren(Of T As DependencyObject)(depObj As DependencyObject) As IEnumerable(Of T)
        If depObj IsNot Nothing Then
            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(depObj) - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(depObj, i)
                If child IsNot Nothing AndAlso TypeOf child Is T Then
                    Yield DirectCast(child, T)
                End If

                For Each childOfChild As T In FindVisualChildren(Of T)(child)
                    Yield childOfChild
                Next
            Next
        End If
    End Function

    Private Sub ImportFromExcelButton_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Window_ContentRendered(sender As Object, e As EventArgs)
        Dim sqlConnDialog As New SQLConnectWindow()
        If sqlConnDialog.ShowDialog() = True Then
            If SharedState.DBContext Is Nothing Then
                Me.Close()
            End If
        Else
            Me.Close()
            Exit Sub
        End If

        'ResultsProcessingUtilities.ProcessResults(2010, 1, 1, False, False)

        'If Not SharedState.DBContext.Database.CompatibleWithModel(False) Then
        '    MsgBox("Database is not compatible with Schema. Recreate Database to use correct Schema")
        '    Exit Sub
        'End If
        SharedState.DBContext.TimeYears.ToList()
        SharedState.DBContext.Semesters.ToList()
        TimeYearsViewSource.Source = SharedState.DBContext.TimeYears.Local
        SemestersViewSource.Source = SharedState.DBContext.Semesters.Local

        SharedState.DBContext.Grades.ToList()
        SharedState.DBContext.Semesters.ToList()
        'SharedState.DBContext.Batches.ToList()
        'SharedState.DBContext.SemesterBatches.ToList()

        SharedState.GetSingleInstance.YearID = 2010
        SharedState.GetSingleInstance.SemesterID = 1
        SharedState.GetSingleInstance.GradeID = 1
        SharedState.GetSingleInstance.DisciplineID = 1

        SharedDataDisplayGrid.DataContext = SharedStateInstance

        AddHandler SharedState.GetSingleInstance().PropertyChanged, AddressOf RefreshMainView
    End Sub
End Class