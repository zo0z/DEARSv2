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

Partial Public Class Course
	Implements IEquatable(Of Course)

	Function Equals1(other as Course) as Boolean Implements IEquatable(Of Course).Equals
		
		 If DirectCast(Me, Object).Equals(DirectCast(other, Object)) Then
            Return True
        Else
			If {Me.Id,other.Id}.Any(Function(s) s = 0) Then 
				Return False
			End If
		    Return (Me.Id = other.Id)
		End If	
	
	End Function
    Public Property Id As Integer
    Public Property CourseCode As String
    Public Property TitleArabic As String
    Public Property TitleEnglish As String

    Public Overridable Property CourseDisciplines As ICollection(Of CourseDiscipline) = New HashSet(Of CourseDiscipline)
    Public Overridable Property CourseEnrollments As ICollection(Of CourseEnrollment) = New HashSet(Of CourseEnrollment)
    Public Overridable Property CourseTeachers As ICollection(Of CourseTeacher) = New HashSet(Of CourseTeacher)
    Public Overridable Property MarksExamCWs As ICollection(Of MarksExamCW) = New HashSet(Of MarksExamCW)
    Public Overridable Property OfferedCourses As ICollection(Of OfferedCourse) = New HashSet(Of OfferedCourse)

End Class
