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

Partial Public Class Student
	Implements IEquatable(Of Student)

	Function Equals1(other as Student) as Boolean Implements IEquatable(Of Student).Equals
		
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
    Public Property Index As Nullable(Of Integer)
    Public Property UnivNo As String
    Public Property NameArabic As String
    Public Property NameEnglish As String

    Public Overridable Property BatchEnrollments As ICollection(Of BatchEnrollment) = New HashSet(Of BatchEnrollment)
    Public Overridable Property CourseEnrollments As ICollection(Of CourseEnrollment) = New HashSet(Of CourseEnrollment)
    Public Overridable Property GPAwRecomms As ICollection(Of GPAwRecomm) = New HashSet(Of GPAwRecomm)
    Public Overridable Property MarksExamCWs As ICollection(Of MarksExamCW) = New HashSet(Of MarksExamCW)
    Public Overridable Property SemesterBatchEnrollments As ICollection(Of SemesterBatchEnrollment) = New HashSet(Of SemesterBatchEnrollment)

End Class
