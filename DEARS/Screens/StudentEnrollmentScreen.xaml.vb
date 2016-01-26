Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data

Public Class StudentEnrollmentScreen
    Implements IBaseScreen
    Public ReadOnly Property DBContext As AcademicResultsDBEntities Implements IBaseScreen.DBContext
        Get
            Return SharedState.DBContext
        End Get
    End Property
     
    Private GradesViewSource As CollectionViewSource
    Private StudentsEnrollmentViewSource As CollectionViewSource
    Private EnrollmentsViewSource As CollectionViewSource

    Dim StudsCollection As StudentSearchCollection
    Sub LoadData(PropertyName As String) Implements IBaseScreen.LoadData
        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID
        Dim SemesterID As Integer = SharedState.GetSingleInstance().SemesterID

        If Not (PropertyName = "GradeID") Then
            Dim q_grades = From bt In DBContext.SemesterBatches.Include("Grades")
                       Where bt.SemesterId = SemesterID And bt.YearId = YearID
                       Select bt.Grade Distinct

            GradesViewSource.Source = q_grades.ToList()

            If q_grades.Count > 0 AndAlso Not (q_grades.ToList().Any(Function(gr) gr.Id = GradeID)) Then
                SharedState.GetSingleInstance.GradeID = q_grades.First().Id
                GradeID = SharedState.GetSingleInstance().GradeID
            End If
        End If

        EnrollmentsViewSource.Source = DBContext.EnrollmentTypes.ToList()

        
        Dim q_studenr = From enr In DBContext.BatchEnrollments.Include("Student")
                        Where enr.YearId = YearID And enr.GradeId = GradeID
                        Select enr

        If StudsCollection Is Nothing Then
            StudsCollection = New StudentSearchCollection()
        Else
            StudsCollection.Clear()
        End If
        StudsCollection.AddExisting(q_studenr)

        StudentsEnrollmentViewSource.Source = StudsCollection

        'Dim q_elstud = From enr In DBContext.BatchEnrollments
        '               Where enr.YearId = YearID And enr.GradeId = GradeID And
    End Sub


    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        GradesViewSource = CType(Me.FindResource("GradesViewSource"), CollectionViewSource)
        StudentsEnrollmentViewSource = CType(Me.FindResource("StudentsEnrollmentViewSource"), CollectionViewSource)
        EnrollmentsViewSource = CType(Me.FindResource("EnrollmentsViewSource"), CollectionViewSource)

        DBContext.Configuration.ProxyCreationEnabled = False
        LoadData("")
        QueryParamnsBox.DataContext = SharedState.GetSingleInstance()
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        QueryParamnsBox.DataContext = Nothing
        DBContext.Configuration.ProxyCreationEnabled = True
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)

    End Sub
End Class

Class StudentSearchCollection
    Inherits System.Collections.ObjectModel.ObservableCollection(Of StudentSearcher)
    Sub New()
        MyBase.New()
    End Sub
    Protected Overrides Sub InsertItem(index As Integer, item As StudentSearcher)
        item.Add()
        MyBase.InsertItem(index, item)
        'SharedState.DBContext.ChangeTracker.DetectChanges()
    End Sub
    Protected Overrides Sub RemoveItem(index As Integer)
        Me.Item(index).Remove()
        MyBase.RemoveItem(index)
        'SharedState.DBContext.ChangeTracker.DetectChanges()
    End Sub
    Sub AddExisting(q_enrs As IEnumerable(Of BatchEnrollment))
        Dim i As Integer = 0
        For Each y In q_enrs.ToList()
            MyBase.InsertItem(i, New StudentSearcher(y))
            i = i + 1
        Next
    End Sub
End Class
Class EnrollmentSearcherPair
    Inherits Tuple(Of BatchEnrollment, StudentSearcher)
    Implements IEquatable(Of EnrollmentSearcherPair)

    Sub New()
        MyBase.New(SharedState.DBContext.BatchEnrollments.Create(), New StudentSearcher())
    End Sub
    Sub New(y As BatchEnrollment)
        MyBase.New(y, New StudentSearcher() With {.StudentId = y.StudentId})
    End Sub

    Public Function Equals1(other As EnrollmentSearcherPair) As Boolean Implements IEquatable(Of EnrollmentSearcherPair).Equals
        Return Me.Item1.Equals1(other.Item1)
    End Function
End Class

Public Class StudentEnrollmentDuplicateValidationRule
    Inherits ValidationRule
    Public Overloads Overrides Function Validate(value As Object, cultureInfo As Globalization.CultureInfo) As ValidationResult
        Dim it As StudentSearcher = TryCast(value, BindingGroup).Items(0)

        If (From z In SharedState.DBContext.Set(Of BatchEnrollment).Local Where z.Equals1(it.BatchEnrollment)).Count > 1 Then
            Return New ValidationResult(False, "Duplicate Item insert a different item")
        Else
            Return ValidationResult.ValidResult
        End If
    End Function
End Class


''' <summary>
''' This class binds to data enry locations and acts as a proxy for the actual student entities. The class once activated retrieves an actual
''' student entoty if it exists and throws and exception if the student does no exist.
''' </summary>
''' <remarks></remarks>
Public Class StudentSearcher
    Implements INotifyPropertyChanged
    Implements IEquatable(Of StudentSearcher)

    Private _Id As Integer
    Private _Index As Integer
    Private _UnivNo As String
    Private _NameEnglish As String
    Private _NameArabic As String
    Private _student As Student
    Sub SetProps(q_st As Student)
        _Id = q_st.Id
        _Index = q_st.Index
        _UnivNo = q_st.UnivNo
        _NameArabic = q_st.NameArabic
        _NameEnglish = q_st.NameEnglish
        _student = q_st

        Dim YearID As Integer = SharedState.GetSingleInstance().YearID
        Dim GradeID As Integer = SharedState.GetSingleInstance().GradeID

        If _benr.StudentId <> 0 AndAlso _benr.StudentId <> _Id Then
            Me.Remove()
            _benr = Nothing
            _benr = SharedState.DBContext.BatchEnrollments.Create()
            _benr.Student = _student
            Me.Add()
        Else
            _benr.Student = _student
        End If



        '_benr.Student = _student
        OnPropertyChanged("StudentId")
        OnPropertyChanged("Index")
        OnPropertyChanged("UnivNo")
        OnPropertyChanged("NameEnglish")
        OnPropertyChanged("NameArabic")
        OnPropertyChanged("Student")
    End Sub
    Property StudentId As Integer
        Get
            Return _Id
        End Get
        Set(value As Integer)
            Dim q_st = (From st In SharedState.DBContext.Students
                       Where st.Id = value Select st).Single()
            SetProps(q_st)
        End Set
    End Property
    Property Index As Integer
        Get
            Return _Index
        End Get
        Set(value As Integer)
            Dim q_st = (From st In SharedState.DBContext.Students
                       Where st.Index = value Select st).Single()
            SetProps(q_st)
        End Set
    End Property
    Property UnivNo As String
        Get
            Return _UnivNo
        End Get
        Set(value As String)
            Dim q_st = (From st In SharedState.DBContext.Students
                       Where st.UnivNo = value Select st).Single()
            SetProps(q_st)
        End Set
    End Property
    Property NameArabic As String
        Get
            Return _NameArabic
        End Get
        Set(value As String)
            Dim q_st = (From st In SharedState.DBContext.Students
                       Where st.NameArabic = value Select st).Single()
            SetProps(q_st)
        End Set
    End Property
    Property NameEnglish As String
        Get
            Return _NameEnglish
        End Get
        Set(value As String)
            Dim q_st = (From st In SharedState.DBContext.Students
                       Where st.NameEnglish = value Select st).Single()
            SetProps(q_st)
        End Set
    End Property
    Property Student As Student
        Get
            Return _student
        End Get
        Set(value As Student)
            _student = value
            SetProps(_student)
        End Set
    End Property

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
    Private _benr As BatchEnrollment
    ReadOnly Property BatchEnrollment As BatchEnrollment
        Get
            Return _benr
        End Get
    End Property
    Sub New()
        _benr = New BatchEnrollment()
    End Sub
    Sub New(benr As BatchEnrollment)
        _benr = benr
        Me.Student = benr.Student
    End Sub
    Sub Add()
        SharedState.DBContext.BatchEnrollments.Add(_benr)
    End Sub
    Sub Remove()
        SharedState.DBContext.BatchEnrollments.Remove(_benr)
    End Sub

    Public Function Equals1(other As StudentSearcher) As Boolean Implements IEquatable(Of StudentSearcher).Equals
        Return _student.Equals1(other.Student)
    End Function
End Class
