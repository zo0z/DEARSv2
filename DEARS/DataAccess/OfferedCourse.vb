'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated from a template.
'
'     Manual changes to this file may cause unexpected behavior in your application.
'     Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Partial Public Class OfferedCourse
	Implements IEquatable(Of OfferedCourse)

	Function Equals1(other as OfferedCourse) as Boolean Implements IEquatable(Of OfferedCourse).Equals
		
		 If DirectCast(Me, Object).Equals(DirectCast(other, Object)) Then
            Return True
        Else
			If {Me.YearId,other.YearId,Me.GradeId,other.GradeId,Me.SemesterId,other.SemesterId,Me.CourseId,other.CourseId}.Any(Function(s) s = 0) Then 
				Return False
			End If
		    Return (Me.YearId = other.YearId) And(Me.GradeId = other.GradeId) And(Me.SemesterId = other.SemesterId) And(Me.CourseId = other.CourseId)
		End If	
	
	End Function
    Public Property YearId As Integer
    Public Property GradeId As Integer
    Public Property SemesterId As Integer
    Public Property CourseId As Integer
    Public Property ExamFraction As Integer
    Public Property CourseWorkFraction As Integer
    Public Property CreditHours As Integer

    Public Overridable Property CourseDisciplines As ICollection(Of CourseDiscipline) = New HashSet(Of CourseDiscipline)
    Public Overridable Property CourseEnrollments As ICollection(Of CourseEnrollment) = New HashSet(Of CourseEnrollment)
    Public Overridable Property Course As Course
    Public Overridable Property CourseTeachers As ICollection(Of CourseTeacher) = New HashSet(Of CourseTeacher)
    Public Overridable Property Grade As Grade
    Public Overridable Property MarksExamCWs As ICollection(Of MarksExamCW) = New HashSet(Of MarksExamCW)
    Public Overridable Property SemesterBatch As SemesterBatch
    Public Overridable Property Semester As Semester
    Public Overridable Property TimeYear As TimeYear

End Class
