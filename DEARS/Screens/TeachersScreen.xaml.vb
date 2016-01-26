Public Class TeachersScreen
    Implements IBaseScreen

    Private _db As AcademicResultsDBEntities
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property


    Private TeachersViewSource As CollectionViewSource

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        LoadData("")
    End Sub

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        TeachersViewSource = CType(Me.FindResource("TeachersViewSource"), CollectionViewSource)

        Dim q_teachers = From tr In DBContext.Teachers
                     Select tr

        TeachersViewSource.Source = New ObservableEntityCollection(Of Teacher)(DBContext, q_teachers)
    End Sub
End Class
