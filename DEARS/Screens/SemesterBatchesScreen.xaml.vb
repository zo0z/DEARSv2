Public Class SemesterBatchesScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property


    Private GradesViewSource As CollectionViewSource
    Private SemesterBatchesViewSource As CollectionViewSource


    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)

        LoadData("")

    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID

        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        SemesterBatchesViewSource = CType(Me.FindResource("SemesterBatchesViewSource"), CollectionViewSource)

        Dim q_grades = From gr In DBContext.Grades
                       Select gr

        GradesViewSource.Source = New ObservableEntityCollection(Of Grade)(DBContext, q_grades)

        Dim q_batches = From bt In DBContext.SemesterBatches _
                         Where bt.YearId = YearID And bt.SemesterId = SemesterID
                         Select bt

        SemesterBatchesViewSource.Source = New ObservableEntityCollection(Of SemesterBatch)(DBContext, q_batches)
    End Sub
End Class
