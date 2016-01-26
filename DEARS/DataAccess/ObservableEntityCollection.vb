Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data.Entity
Public Class ObservableEntityCollection(Of Tx As {IEquatable(Of Tx)})
    Inherits System.Collections.ObjectModel.ObservableCollection(Of Tx)
    Private _db As AcademicResultsDBEntities
    ReadOnly Property DBContext As AcademicResultsDBEntities
        Get
            Return _db
        End Get
    End Property

    Public Sub New(db As AcademicResultsDBEntities, collection As IEnumerable(Of Tx))
        MyBase.New(collection)
        _db = db
    End Sub
    Public Sub New(db As AcademicResultsDBEntities)
        MyBase.New()
        _db = db
    End Sub
    Protected Overrides Sub InsertItem(index As Integer, item As Tx)
        If Not Me.Contains(item) Then
            MyBase.InsertItem(index, item)
        End If
        If Not DBContext.Set(GetType(Tx)).Local.Contains(item) Then
            DBContext.Set(GetType(Tx)).Add(item)
        End If

        'Select Case GetType(Tx)
        '    Case GetType(TimeYear)
        '        If Not DBContext.TimeYears.Local.Any(Function(s) CType(CType(item, Object), TimeYear).Equals1(s)) Then
        '            DBContext.TimeYears.Add(CType(CType(item, Object), TimeYear))
        '        End If
        '    Case GetType(Grade)
        '        DBContext.Grades.Add(CType(CType(item, Object), Grade))
        '    Case GetType(Semester)
        '        DBContext.Semesters.Add(CType(CType(item, Object), Semester))

        '    Case GetType(Student)
        '        DBContext.Students.Add(CType(CType(item, Object), Student))
        '    Case GetType(Discipline)
        '        DBContext.Disciplines.Add(CType(CType(item, Object), Discipline))
        '    Case GetType(Course)
        '        DBContext.Courses.Add(CType(CType(item, Object), Course))
        '    Case GetType(Teacher)
        '        DBContext.Teachers.Add(CType(CType(item, Object), Teacher))

        '    Case GetType(Batch)
        '        DBContext.Batches.Add(CType(CType(item, Object), Batch))
        '    Case GetType(SemesterBatch)
        '        DBContext.SemesterBatches.Add(CType(CType(item, Object), SemesterBatch))

        '    Case GetType(BatchEnrollment)
        '        DBContext.BatchEnrollments.Add(CType(CType(item, Object), BatchEnrollment))
        '    Case GetType(SemesterBatchEnrollment)
        '        DBContext.SemesterBatchEnrollments.Add(CType(CType(item, Object), SemesterBatchEnrollment))


        '    Case GetType(OfferedCourse)
        '        DBContext.OfferedCourses.Add(CType(CType(item, Object), OfferedCourse))
        '    Case GetType(OfferedDiscipline)
        '        DBContext.OfferedDisciplines.Add(CType(CType(item, Object), OfferedDiscipline))
        '    Case GetType(CourseEnrollment)
        '        If Not DBContext.CourseEnrollments.Local.Any(Function(s) CType(CType(item, Object), CourseEnrollment).Equals1(s)) Then
        '            DBContext.CourseEnrollments.Add(CType(CType(item, Object), CourseEnrollment))
        '        End If
        '    Case GetType(CourseTeacher)
        '        DBContext.CourseTeachers.Add(CType(CType(item, Object), CourseTeacher))
        '    Case GetType(CourseDiscipline)
        '        DBContext.CourseDisciplines.Add(CType(CType(item, Object), CourseDiscipline))
        '    Case GetType(MarksExamCW)
        '        If Not DBContext.MarksExamCWs.Local.Any(Function(s) CType(CType(item, Object), MarksExamCW).Equals1(s)) Then
        '            DBContext.MarksExamCWs.Add(CType(CType(item, Object), MarksExamCW))
        '        End If
        '    Case Else
        '        Throw New NotImplementedException()
        'End Select
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Dim it = Me.Item(index)
        If DBContext.Set(GetType(Tx)).Local.Contains(it) Then
            DBContext.Set(GetType(Tx)).Remove(it)
        End If
        MyBase.RemoveItem(index)
        'Dim Item As Tx = Me.Item(index)
        'Select Case GetType(Tx)
        '    Case GetType(TimeYear)
        '        DBContext.TimeYears.Remove(CType(CType(Item, Object), TimeYear))
        '    Case GetType(Grade)
        '        DBContext.Grades.Remove(CType(CType(Item, Object), Grade))
        '    Case GetType(Semester)
        '        DBContext.Semesters.Remove(CType(CType(Item, Object), Semester))

        '    Case GetType(Student)
        '        DBContext.Students.Remove(CType(CType(Item, Object), Student))
        '    Case GetType(Discipline)
        '        DBContext.Disciplines.Remove(CType(CType(Item, Object), Discipline))
        '    Case GetType(Course)
        '        DBContext.Courses.Remove(CType(CType(Item, Object), Course))
        '    Case GetType(Teacher)
        '        DBContext.Teachers.Remove(CType(CType(Item, Object), Teacher))

        '    Case GetType(Batch)
        '        DBContext.Batches.Remove(CType(CType(Item, Object), Batch))
        '    Case GetType(SemesterBatch)
        '        DBContext.SemesterBatches.Remove(CType(CType(Item, Object), SemesterBatch))

        '    Case GetType(BatchEnrollment)
        '        DBContext.BatchEnrollments.Remove(CType(CType(Item, Object), BatchEnrollment))
        '    Case GetType(SemesterBatchEnrollment)
        '        DBContext.SemesterBatchEnrollments.Remove(CType(CType(Item, Object), SemesterBatchEnrollment))


        '    Case GetType(OfferedCourse)
        '        DBContext.OfferedCourses.Remove(CType(CType(Item, Object), OfferedCourse))
        '    Case GetType(OfferedDiscipline)
        '        DBContext.OfferedDisciplines.Remove(CType(CType(Item, Object), OfferedDiscipline))
        '    Case GetType(CourseEnrollment)
        '        DBContext.CourseEnrollments.Remove(CType(CType(Item, Object), CourseEnrollment))
        '    Case GetType(CourseTeacher)
        '        DBContext.CourseTeachers.Remove(CType(CType(Item, Object), CourseTeacher))
        '    Case GetType(CourseDiscipline)
        '        DBContext.CourseDisciplines.Remove(CType(CType(Item, Object), CourseDiscipline))
        '    Case GetType(MarksExamCW)
        '        DBContext.MarksExamCWs.Remove(CType(CType(Item, Object), MarksExamCW))
        '    Case Else
        '        Throw New NotImplementedException()
        'End Select
        'MyBase.RemoveItem(index)
    End Sub
End Class