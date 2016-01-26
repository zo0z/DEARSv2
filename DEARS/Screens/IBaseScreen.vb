Public Interface IBaseScreen
    ReadOnly Property DBContext As AcademicResultsDBEntities
    Sub LoadData(PropertyName As String)
End Interface

Public Class DuplicateValidationRule
    Inherits ValidationRule
    Public Overloads Overrides Function Validate(value As Object, cultureInfo As Globalization.CultureInfo) As ValidationResult
        Dim Tx = TryCast(value, BindingGroup).Items(0).GetType()
        Dim it = CTypeDynamic(TryCast(value, BindingGroup).Items(0), Tx)
   
        If (From z In SharedState.DBContext.Set(Tx).Local _
            Where z.Equals1(it) And (SharedState.DBContext.Entry(z).State <> System.Data.Entity.EntityState.Deleted Or SharedState.DBContext.Entry(z).State <> System.Data.Entity.EntityState.Detached) _
            ).Count > 1 Then
            Return New ValidationResult(False, "Duplicate Item insert a different item")
        Else
            Return ValidationResult.ValidResult
        End If
    End Function
End Class

Public Class NotBoolConverter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Return Not CType(value, Boolean)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException(0)
    End Function
End Class