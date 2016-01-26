Imports System.ComponentModel
Public Class TranscriptScreen
    Implements IBaseScreen

    Dim tIssue As New TranscriptIssuer()

    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Dim StudentsViewSource As CollectionViewSource
    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        StudentsViewSource.Source = (From stud In DBContext.Students Select stud).ToList()
        If Not String.IsNullOrWhiteSpace(My.Settings.TemplateLocation) Then
            TemplateLocationTextBox.Text = My.Settings.TemplateLocation
        End If
        If Not String.IsNullOrWhiteSpace(My.Settings.TranscriptOutputLocation) Then
            OutputFileTextBox.Text = My.Settings.TranscriptOutputLocation
        End If
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        StudentsViewSource = Me.FindResource("StudentsViewSource")
        LoadData("")
        Me.DataContext = tIssue
        AddHandler tIssue.PropertyChanged, AddressOf StudentSelected
    End Sub



    Private Sub StudentSelected(sender As Object, e As PropertyChangedEventArgs)
        If tIssue.SelectedStudent IsNot Nothing Then
            TranscriptSummaryDataGrid.ItemsSource =
            (From d In tIssue.SelectedStudent.BatchEnrollments
             Order By d.YearId Ascending, d.GradeId Ascending
             Select d).ToList()

            If (From d In tIssue.SelectedStudent.BatchEnrollments
                 Order By d.YearId Ascending, d.GradeId Ascending
                 Select d).Last.GradeId = 5 Then
                TranscriptTypeLabel.Content = "Graduate"
            Else
                TranscriptTypeLabel.Content = "Undergraduate"
            End If
        End If
    End Sub

    Private Sub OutputFileButton_Click(sender As Object, e As RoutedEventArgs)
        Dim bfldr As New Forms.FolderBrowserDialog()
        If bfldr.ShowDialog() = Forms.DialogResult.OK Then
            OutputFileTextBox.Text = bfldr.SelectedPath
        End If
    End Sub

    Private Sub TemplateLocationButton_Click(sender As Object, e As RoutedEventArgs)
        Dim bfldr As New Forms.FolderBrowserDialog()
        If bfldr.ShowDialog() = Forms.DialogResult.OK Then
            TemplateLocationTextBox.Text = bfldr.SelectedPath
        End If
    End Sub

    Private Sub IssueTranscriptButon_Click(sender As Object, e As RoutedEventArgs)
        Try
            tIssue.IssueTranscript(OutputFileTextBox.Text, TemplateLocationTextBox.Text)
            My.Settings.TemplateLocation = TemplateLocationTextBox.Text
            My.Settings.TranscriptOutputLocation = OutputFileTextBox.Text
            My.Settings.Save()
        Catch ex As Exception
            MsgBox("Transcript Generation Failure: " + vbCr + ex.Message)
        End Try
    End Sub

    Private Sub SaveNameEnglish_Click(sender As Object, e As RoutedEventArgs)
        DBContext.SaveChanges()
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler tIssue.PropertyChanged, AddressOf StudentSelected
    End Sub
End Class