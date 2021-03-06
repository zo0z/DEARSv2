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

Partial Public Class CourseEnrollment
	Implements IEquatable(Of CourseEnrollment)

	Function Equals1(other as CourseEnrollment) as Boolean Implements IEquatable(Of CourseEnrollment).Equals
		
		 If DirectCast(Me, Object).Equals(DirectCast(other, Object)) Then
            Return True
        Else
			If {Me.YearId,other.YearId,Me.GradeId,other.GradeId,Me.SemesterId,other.SemesterId,Me.StudentId,other.StudentId,Me.CourseId,other.CourseId}.Any(Function(s) s = 0) Then 
				Return False
			End If
		    Return (Me.YearId = other.YearId) And(Me.GradeId = other.GradeId) And(Me.SemesterId = other.SemesterId) And(Me.StudentId = other.StudentId) And(Me.CourseId = other.CourseId)
		End If	
	
	End Function
    Public Property YearId As Integer
    Public Property GradeId As Integer
    Public Property SemesterId As Integer
    Public Property StudentId As Integer
    Public Property CourseId As Integer

    Public Overridable Property Cours As Course
    Public Overridable Property Grade As Grade
    Public Overridable Property OfferedCourse As OfferedCourse
    Public Overridable Property SemesterBatchEnrollment As SemesterBatchEnrollment
    Public Overridable Property Semester As Semester
    Public Overridable Property Student As Student
    Public Overridable Property TimeYear As TimeYear
    Public Overridable Property MarksExamCW As MarksExamCW

End Class
