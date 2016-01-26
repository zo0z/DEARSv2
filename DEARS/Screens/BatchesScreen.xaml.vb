Imports System.Collections.ObjectModel
Public Class BatchesScreen
    Implements IBaseScreen

    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Private GradesViewSource As CollectionViewSource
    Private BatchesViewSource As CollectionViewSource
    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID

        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        BatchesViewSource = CType(Me.FindResource("BatchesViewSource"), CollectionViewSource)


        Dim q_grades = From gr In DBContext.Grades
                   Select gr

        GradesViewSource.Source = q_grades.ToList()

        Dim q_batches = (From bt In DBContext.Batches
                         Where bt.YearId = YearID
                         Select bt).ToList()

        'BatchesViewSource.Source = 
        BatchesViewSource.Source = New ObservableEntityCollection(Of Batch)(DBContext, q_batches)

    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)

        Dim x = DirectCast(BatchesViewSource.Source, ObservableEntityCollection(Of Batch))
        Dim y As New Batch() With {.YearId = 2010, .GradeId = 3}
        x.Insert(x.Count, y)
        x.Remove(y)
        Dim y1 As New Batch() With {.YearId = 2010, .GradeId = 3}
        x.Insert(x.Count, y1)
        x.Remove(y1)
        DBContext.SaveChanges()
    End Sub
End Class