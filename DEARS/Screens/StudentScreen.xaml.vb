Public Class StudentScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private StudentsViewSource As CollectionViewSource

    Private Sub SearchButton_Click(sender As Object, e As RoutedEventArgs)

        Dim IsindexNo As Boolean
        Integer.TryParse(StudentSearchTextBox.Text, IsindexNo)

        If String.IsNullOrWhiteSpace(StudentSearchTextBox.Text) Then
            StudentsViewSource.Source = New ObservableEntityCollection(Of Student)(DBContext)
        ElseIf IsindexNo Then
            Dim IndexNo As Integer = Integer.Parse(StudentSearchTextBox.Text)
            Dim q_student = From st In DBContext.Students
                            Where st.Index = IndexNo
                            Select st
            StudentsViewSource.Source = New ObservableEntityCollection(Of Student)(DBContext, q_student)
        Else
            Dim q_str As String = StudentSearchTextBox.Text
            Dim q_students = From st In DBContext.Students _
                             Where st.NameArabic.Contains(q_str) Or st.NameEnglish.Contains(q_str) Or st.UnivNo.Contains(q_str)
                             Select st

            StudentsViewSource.Source = New ObservableEntityCollection(Of Student)(DBContext, q_students)
        End If
    End Sub

    Private Sub NewStudentButton_Click(sender As Object, e As RoutedEventArgs)
        If StudentsViewSource.Source Is Nothing Then
            StudentsViewSource.Source = New ObservableEntityCollection(Of Student)(DBContext)
        End If
        CType(StudentsViewSource.Source, ObservableEntityCollection(Of Student)).Add(New Student())
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        StudentsViewSource = CType(Me.FindResource("StudentsViewSource"), CollectionViewSource)
    End Sub

    Private Sub DeleteStudentButton_Click(sender As Object, e As RoutedEventArgs)
        If Not StudentsViewSource.Source Is Nothing Then
            CType(StudentsViewSource.Source, ObservableEntityCollection(Of Student)).Remove(StudentsDataGrid.SelectedItem)
        End If
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData

    End Sub
End Class
