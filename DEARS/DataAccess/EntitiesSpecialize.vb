Imports System.ComponentModel
Imports System.Data.Common
Imports System.Data.Entity.Infrastructure
Imports System.Xml
Imports System.Reflection
Imports System.Data.Entity.Core.EntityClient
Imports System.Data.Entity.Core.Metadata.Edm

Public Class SharedState
    Implements System.ComponentModel.INotifyPropertyChanged

    Private _YearID
    Property YearID As Integer
        Get
            Return _YearID
        End Get
        Set(value As Integer)
            _YearID = value
            OnPropertyChanged("YearID")
        End Set
    End Property
    Private _GradeID As Integer
    Property GradeID As Integer
        Get
            Return _GradeID
        End Get
        Set(value As Integer)
            _GradeID = value
            OnPropertyChanged("GradeID")
        End Set
    End Property
    Private _SemesterID As Integer
    Property SemesterID As Integer
        Get
            Return _SemesterID
        End Get
        Set(value As Integer)
            _SemesterID = value
            OnPropertyChanged("SemesterID")
        End Set
    End Property
    Private _CourseID As Integer
    Property CourseID As Integer
        Get
            Return _CourseID
        End Get
        Set(value As Integer)
            _CourseID = value
            OnPropertyChanged("CourseID")
        End Set
    End Property
    Private _DisciplineID As Integer
    Property DisciplineID As Integer
        Get
            Return _DisciplineID
        End Get
        Set(value As Integer)
            _DisciplineID = value
            OnPropertyChanged("DisciplineID")
        End Set
    End Property
    Private _AllDisciplines As Boolean
    Property AllDisciplines As Boolean
        Get
            Return _AllDisciplines
        End Get
        Set(value As Boolean)
            _AllDisciplines = value
        End Set
    End Property
    Dim evRaised As Boolean = False
    Public Event PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(propertyName As String)
        If Not evRaised Then
            evRaised = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            evRaised = False
        End If
    End Sub
    Private Sub New()

    End Sub
    Private Shared SingleInstance As SharedState = New SharedState()
    Public Shared Function GetSingleInstance() As SharedState
        Return SingleInstance
    End Function

    'Private Shared _db As AcademicResultsDBEntities
    Public Shared Property DBContext As AcademicResultsDBEntities

End Class

Public Class OfferedCourse
    Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        Me.GradeId = SharedState.GetSingleInstance().GradeID
        Me.SemesterId = SharedState.GetSingleInstance().SemesterID
    End Sub
End Class
Public Class Batch
    Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        'Me.GradeId = 0
    End Sub
End Class

Public Class SemesterBatch
    Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        Me.SemesterId = SharedState.GetSingleInstance().SemesterID
    End Sub
End Class

Public Class OfferedDiscipline
    Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        Me.SemesterId = SharedState.GetSingleInstance().SemesterID
    End Sub
End Class

Public Class CourseTeacher
    Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        Me.GradeId = SharedState.GetSingleInstance().GradeID
        Me.SemesterId = SharedState.GetSingleInstance().SemesterID
        Me.CourseId = SharedState.GetSingleInstance().CourseID
    End Sub
End Class

Public Class BatchEnrollment
    Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        Me.GradeId = SharedState.GetSingleInstance().GradeID
    End Sub
End Class
Class CourseDiscipline
    Public Sub New()
        Me.YearId = SharedState.GetSingleInstance().YearID
        Me.GradeId = SharedState.GetSingleInstance().GradeID
        Me.SemesterId = SharedState.GetSingleInstance().SemesterID
        Me.DisciplineId = SharedState.GetSingleInstance().DisciplineID
    End Sub

End Class

Partial Class AcademicResultsDBEntities
    Sub New(connectionString As String)
        MyBase.New("metadata=res://*/DataAccess.AcademicResultsDB.csdl|res://*/DataAccess.AcademicResultsDB.ssdl|res://*/DataAccess.AcademicResultsDB.msl;provider=System.Data.SqlClient;provider connection string = " + """" + connectionString + """")
    End Sub
    'Sub New(dbconn As DbConnection)
    '    MyBase.New(CreateEntityConnection("dbo", "", "DataAccess.AcademicResultsDB", dbconn), False)
    'End Sub

    'Public Shared Function CreateEntityConnection(schema As String, connString As String, model As String, dbconn As DbConnection) As EntityConnection
    '    Dim conceptualReader As XmlReader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(model & ".csdl"))
    '    Dim mappingReader As XmlReader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(model & ".msl"))

    '    Dim storageReader As XmlReader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(model + ".ssdl"))

    '    Dim storageNS As XNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl"

    '    Dim storageXml = XElement.Load(storageReader)

    '    'For Each entitySet In storageXml.Descendants(storageNS + "EntitySet")
    '    '    Dim schemaAttribute = entitySet.Attributes("Schema").FirstOrDefault()
    '    '    If schemaAttribute IsNot Nothing Then
    '    '        schemaAttribute.SetValue(schema)
    '    '    End If
    '    'Next

    '    storageXml.CreateReader()

    '    'Dim storageCollection As New StoreItemCollection(New XmlReader() {storageXml.CreateReader()})
    '    'Dim conceptualCollection As New EdmItemCollection(conceptualReader)
    '    'Dim mappingCollection As New StorageMappingItemCollection(conceptualCollection, storageCollection, mappingReader)

    '    Dim lmeta As New List(Of String) From {"res://*/DataAccess.AcademicResultsDB.csdl", "res://*/DataAccess.AcademicResultsDB.ssdl", "res://*/DataAccess.AcademicResultsDB.msl"}
    '    Dim lass As New List(Of Assembly)
    '    lass.Add(Assembly.GetExecutingAssembly)

    '    Dim workspace = New MetadataWorkspace(lmeta, lass)
    '    'workspace.RegisterItemCollection(conceptualCollection)
    '    'workspace.RegisterItemCollection(storageCollection)
    '    'workspace.RegisterItemCollection(mappingCollection)

    '    'Dim connectionData = New EntityConnectionStringBuilder(connString)
    '    'Dim connection = DbProviderFactories.GetFactory(connectionData.Provider).CreateConnection()
    '    'connection.ConnectionString = connectionData.ProviderConnectionString

    '    Return New EntityConnection(workspace, dbconn)
    'End Function

    Sub New(dbConnection As DbConnection, p2 As Boolean)
        MyBase.New(MixConnection(dbConnection), p2)
    End Sub

    Public Shared Function GetEDMXConnectionString(Of T)(dbconn As DbConnection) As EntityConnection
        Dim resourceArray As String() = {"res://*/DataAccess.AcademicResultsDB.csdl|res://*/DataAccess.AcademicResultsDB.ssdl|res://*/DataAccess.AcademicResultsDB.msl"}
        Dim assemblyList As Assembly() = {GetType(T).Assembly}
        Dim metaData As New MetadataWorkspace(resourceArray, assemblyList)
        Dim edmxConnection As New EntityConnection(metaData, dbconn)

        Return edmxConnection
    End Function
    Shared Function MixConnection(DbConnection As DbConnection) As DbConnection
        If TryCast(DbConnection, EntityConnection) IsNot Nothing Then
            Return DbConnection
        Else
            Return New EntityConnection(GetMetaDataWorkspace, DbConnection)
        End If
    End Function
    Shared Function GetMetaDataWorkspace()
        If SharedState.DBContext IsNot Nothing Then
            If TryCast(SharedState.DBContext, IObjectContextAdapter) IsNot Nothing Then
                Return TryCast(SharedState.DBContext, IObjectContextAdapter).ObjectContext.MetadataWorkspace()
            End If
        End If
        GetMetaDataWorkspace = Nothing
    End Function
End Class