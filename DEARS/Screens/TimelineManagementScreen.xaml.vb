Imports System.ComponentModel

Public Class TimelineManagementScreen
    Implements IBaseScreen

    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property

    Public Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData

    End Sub

    Private Sub PrevYearButton_Click(sender As Object, e As RoutedEventArgs)
        Dim YearID = SharedState.GetSingleInstance.YearID
        Dim prvYr = SharedState.DBContext.TimeYears.Where(Function(s) s.Id = (YearID - 1)).SingleOrDefault()
        If prvYr Is Nothing Then
            prvYr = New TimeYear() With {.Id = YearID - 1, .NameArabic = (YearID - 1) & "/" & (YearID), .NameEnglish = (YearID - 1) & "/" & (YearID)}
            SharedState.DBContext.TimeYears.Add(prvYr)
            SharedState.DBContext.SaveChanges()
        End If
        SharedState.GetSingleInstance.YearID = YearID - 1
    End Sub

    Private Sub NextYearButton_Click(sender As Object, e As RoutedEventArgs)
        Dim YearID = SharedState.GetSingleInstance.YearID
        Dim nxtYr = SharedState.DBContext.TimeYears.Where(Function(s) s.Id = (YearID + 1)).SingleOrDefault()
        If nxtYr Is Nothing Then
            nxtYr = New TimeYear() With {.Id = YearID + 1, .NameArabic = (YearID + 1) & "/" & (YearID + 2), .NameEnglish = (YearID + 1) & "/" & (YearID + 2)}
            SharedState.DBContext.TimeYears.Add(nxtYr)
            SharedState.DBContext.SaveChanges()
        End If
        SharedState.GetSingleInstance.YearID = YearID + 1
    End Sub

    Private Sub FullControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim x As New TimelineSnapshotViewModel()
        Me.DataContext = x
        AddHandler SharedState.GetSingleInstance.PropertyChanged, AddressOf x.NotifyChanges
    End Sub

    Private Sub FullControl_Unloaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler SharedState.GetSingleInstance.PropertyChanged, AddressOf DirectCast(Me.DataContext, TimelineSnapshotViewModel).NotifyChanges
    End Sub

    Class TimelineSnapshotViewModel
        Implements INotifyPropertyChanged


        ReadOnly Property CurrentYear As String
            Get
                Dim YearID = SharedState.GetSingleInstance.YearID
                Return (YearID) & "/" & (YearID + 1)
            End Get
        End Property
        ReadOnly Property PreviousYear As String
            Get
                Dim YearID = SharedState.GetSingleInstance.YearID
                Return (YearID - 1) & "/" & (YearID)
            End Get
        End Property
        ReadOnly Property NextYear As String
            Get
                Dim YearID = SharedState.GetSingleInstance.YearID
                Return (YearID + 1) & "/" & (YearID + 2)
            End Get
        End Property
        ReadOnly Property IsFirstSemesterActive As String
            Get
                Dim YearID = SharedState.GetSingleInstance.YearID
                Dim sem1 = (From s In SharedState.DBContext.Semesters.Local
                           Where s.Id = 1).Single()
                Dim csbt = sem1.SemesterBatches.Where(Function(s) s.YearId = YearID).Count()
                Return "Active Grades: " & csbt
            End Get
        End Property
        ReadOnly Property IsSecondSemesterActive As String
            Get
                Dim YearID = SharedState.GetSingleInstance.YearID
                Dim sem1 = (From s In SharedState.DBContext.Semesters.Local
                           Where s.Id = 2).Single()
                Dim csbt = sem1.SemesterBatches.Where(Function(s) s.YearId = YearID).Count()
                Return "Active Grades: " & csbt
            End Get
        End Property

        Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
        Public Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
        Public Sub NotifyChanges(sender As Object, e As PropertyChangedEventArgs)
            If e.PropertyName = "YearID" Then
                OnPropertyChanged("CurrentYear")
                OnPropertyChanged("PreviousYear")
                OnPropertyChanged("NextYear")
                OnPropertyChanged("IsFirstSemesterActive")
                OnPropertyChanged("IsSecondSemesterActive")
            End If
            If e.PropertyName = "SemesterID" Then
                OnPropertyChanged("IsFirstSemesterActive")
                OnPropertyChanged("IsSecondSemesterActive")
            End If
        End Sub
    End Class

    Private Sub FirstSemesterButton_Click(sender As Object, e As RoutedEventArgs)
        CreateBatchandSems(1)
    End Sub

    Private Sub SecondtSemesterButton_Click(sender As Object, e As RoutedEventArgs)
        CreateBatchandSems(2)
    End Sub

    Sub CreateBatchandSems(sem As Integer)
        Dim YearID = SharedState.GetSingleInstance.YearID
        Dim ty = (From t In DBContext.TimeYears.Local Where t.Id = YearID).Single()
        If ty.Batches.Count < DBContext.Grades.Local.Count Then
            'Not All batches exist, create them
            For Each gid In DBContext.Grades.Local.Select(Of Integer)(Function(g) g.Id).Except(ty.Batches.Select(Of Integer)(Function(g) g.GradeId))
                Dim bt As New Batch() With {.GradeId = gid}
                DBContext.Batches.Add(bt)
            Next
        End If
        If (From sbt In DBContext.SemesterBatches Where sbt.YearId = YearID And sbt.SemesterId = sem).Count() < DBContext.Grades.Local.Count Then
            For Each gid In DBContext.Grades.Local.Select(Of Integer)(Function(g) g.Id).Except(ty.SemesterBatches.
                                                                                               Where(Function(s) s.YearId = YearID And s.SemesterId = sem).
                                                                                               Select(Of Integer)(Function(g) g.GradeId))
                Dim bt As New SemesterBatch() With {.GradeId = gid, .SemesterId = sem}
                DBContext.SemesterBatches.Add(bt)
            Next
        End If
        SharedState.GetSingleInstance.SemesterID = sem
    End Sub
End Class

Public Class HeightCalculator
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Dim h1 As Integer = value
        Return (h1 - 4 * 60) / 2
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class