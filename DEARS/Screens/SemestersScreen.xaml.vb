Public Class SemestersScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property


    Private SemestersViewSource As CollectionViewSource

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        SemestersViewSource = CType(Me.FindResource("SemestersViewSource"), CollectionViewSource)

        Dim q_semesters = From gr In DBContext.Semesters
                     Select gr

        SemestersViewSource.Source = New ObservableEntityCollection(Of Semester)(DBContext, q_semesters)
    End Sub
End Class