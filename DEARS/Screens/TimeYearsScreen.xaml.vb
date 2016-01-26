Public Class TimeYearsScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property


    Private TimeYearsViewSource As CollectionViewSource

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        TimeYearsViewSource = CType(Me.FindResource("TimeYearsViewSource"), CollectionViewSource)

        Dim q_timeyears = From gr In DBContext.TimeYears
                     Select gr

        TimeYearsViewSource.Source = New ObservableEntityCollection(Of TimeYear)(DBContext, q_timeyears)
    End Sub
End Class
