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

Partial Public Class CourseDiscipline
	Implements IEquatable(Of CourseDiscipline)

	Function Equals1(other as CourseDiscipline) as Boolean Implements IEquatable(Of CourseDiscipline).Equals
		
		 If DirectCast(Me, Object).Equals(DirectCast(other, Object)) Then
            Return True
        Else
			If {Me.YearId,other.YearId,Me.GradeId,other.GradeId,Me.SemesterId,other.SemesterId,Me.CourseId,other.CourseId,Me.DisciplineId,other.DisciplineId}.Any(Function(s) s = 0) Then 
				Return False
			End If
		    Return (Me.YearId = other.YearId) And(Me.GradeId = other.GradeId) And(Me.SemesterId = other.SemesterId) And(Me.CourseId = other.CourseId) And(Me.DisciplineId = other.DisciplineId)
		End If	
	
	End Function
    Public Property YearId As Integer
    Public Property GradeId As Integer
    Public Property SemesterId As Integer
    Public Property CourseId As Integer
    Public Property DisciplineId As Integer
    Public Property [Optional] As Boolean

    Public Overridable Property Course As Course
    Public Overridable Property Discipline As Discipline
    Public Overridable Property Grade As Grade
    Public Overridable Property OfferedCourse As OfferedCourse
    Public Overridable Property OfferedDiscipline As OfferedDiscipline
    Public Overridable Property SemesterBatch As SemesterBatch
    Public Overridable Property Semester As Semester
    Public Overridable Property TimeYear As TimeYear

End Class
