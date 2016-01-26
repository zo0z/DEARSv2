Imports DEARS.ExcelSimplified
Imports System.Reflection

Public Class ExcelExporter
    Public Shared Sub ExportData(dg As DataGrid, Filename As String, SheetName As String)
        Dim colWidths = New List(Of Integer)(dg.Columns.Count)
        Dim wb = New ExcelSimplified.CWorkbook(Filename, True)
        Dim ws = wb.CreateNewWorksheet(SheetName)
        For i As Integer = 0 To dg.Columns.Count - 1
            Dim clmn = dg.Columns(i)
            ws.CreateNewRange(i + 1, 1, clmn.Header, DocumentFormat.OpenXml.Spreadsheet.CellValues.String, 1, 1)
            colWidths.Add(clmn.Header.ToString.Length)
        Next

        Dim LastValidElement As Integer = 0
        If dg.CanUserAddRows Then
            LastValidElement = dg.Items.Count - 2
        Else
            LastValidElement = dg.Items.Count - 1
        End If
        For i As Integer = 0 To LastValidElement
            Dim it = dg.Items(i)
            Dim j = 0
            For Each clmn In dg.Columns
                If TryCast(clmn, DataGridTextColumn) IsNot Nothing Then
                    Dim txtClmn = TryCast(clmn, DataGridTextColumn)
                    Dim prprtyName As String = Nothing
                    If txtClmn.Binding IsNot Nothing Then
                        prprtyName = DirectCast(txtClmn.Binding, Binding).Path.Path
                    End If
                    If Not String.IsNullOrWhiteSpace(prprtyName) Then
                        Dim value
                        If prprtyName.Contains(".") Then
                            Dim prps = prprtyName.Split(".")
                            value = it.GetType().GetProperty(prps(0)).GetValue(it)
                            For Each px In prps.Skip(1)
                                value = value.GetType().GetProperty(px).GetValue(value)
                            Next
                        Else
                            value = it.GetType().GetProperty(prprtyName).GetValue(it)
                        End If
                        If value IsNot Nothing Then
                            ws.CreateNewRange(j + 1, i + 2, value.ToString(), DocumentFormat.OpenXml.Spreadsheet.CellValues.String, 1, 1)
                            colWidths(j) = Math.Max(colWidths(j), value.ToString.Length)
                        End If
                    End If
                    j += 1
                ElseIf TryCast(clmn, DataGridComboBoxColumn) IsNot Nothing Then
                    Dim cmbClmn = TryCast(clmn, DataGridComboBoxColumn)
                    Dim prprtyName As String = Nothing
                    Dim value = GetComboBoxValue(it, cmbClmn)
                    ws.CreateNewRange(j + 1, i + 2, value.ToString(), DocumentFormat.OpenXml.Spreadsheet.CellValues.String, 1, 1)
                    colWidths(j) = Math.Max(colWidths(j), value.ToString.Length)
                    j += 1
                ElseIf TryCast(clmn, DataGridCheckBoxColumn) IsNot Nothing Then
                    Dim cbClmn = TryCast(clmn, DataGridCheckBoxColumn)
                    Dim prprtyName As String = Nothing

                    If cbClmn.Binding IsNot Nothing Then
                        prprtyName = DirectCast(cbClmn.Binding, Binding).Path.Path
                    End If
                    If Not String.IsNullOrWhiteSpace(prprtyName) Then
                        Dim value
                        If prprtyName.Contains(".") Then
                            Dim prps = prprtyName.Split(".")
                            value = it.GetType().GetProperty(prps(0)).GetValue(it)
                            value = value.GetType().GetProperty(prps(1)).GetValue(value)
                        Else
                            value = it.GetType().GetProperty(prprtyName).GetValue(it)
                        End If
                        If value Then
                            ws.CreateNewRange(j + 1, i + 2, 1, DocumentFormat.OpenXml.Spreadsheet.CellValues.Number, 1, 1)
                        Else
                            ws.CreateNewRange(j + 1, i + 2, 0, DocumentFormat.OpenXml.Spreadsheet.CellValues.Number, 1, 1)
                        End If

                    End If
                    j += 1
                End If
            Next
        Next
        For j = 0 To colWidths.Count - 1
            ws.SetWidths(j + 1, j + 1, 1.2 * colWidths(j))
        Next
        wb.Save()
    End Sub

    Shared Function GetComboBoxValue(it As Object, clmn As DataGridComboBoxColumn)
        Return GetValueFollowPath(it, TryCast(clmn.SelectedValueBinding, Binding).Path.Path, clmn.DisplayMemberPath)
    End Function

    'Shared Function GetComboboxValue(it As Object, column As DataGridComboBoxColumn) As Object

    '    Dim cmbColumn As DataGridComboBoxColumn = TryCast(column, DataGridComboBoxColumn)
    '    Dim propertyValue As String = String.Empty

    '    ' Get the property name from the column's binding 

    '    Dim bb As BindingBase = cmbColumn.SelectedValueBinding
    '    If bb IsNot Nothing Then
    '        Dim binding As Binding = TryCast(bb, Binding)
    '        If binding IsNot Nothing Then
    '            Dim boundProperty As String = binding.Path.Path
    '            'returns "Category" (or CategoryId)
    '            ' Get the selected property 

    '            Dim pi As PropertyInfo = GetValueFollowPath(it, boundProperty)
    '            If pi IsNot Nothing Then
    '                Dim boundProperyValue As Object = pi.GetValue(it)
    '                'returns the selected Category object or CategoryId
    '                If boundProperyValue IsNot Nothing Then
    '                    Dim propertyType As Type = boundProperyValue.[GetType]()
    '                    If propertyType.IsPrimitive OrElse propertyType.Equals(GetType(String)) Then
    '                        If cmbColumn.ItemsSource IsNot Nothing Then
    '                            ' Find the Category object in the ItemsSource of the ComboBox with
    '                            '                             * an Id (SelectedValuePath) equal to the selected CategoryId 

    '                            Dim comboBoxSource As IEnumerable(Of Object) = cmbColumn.ItemsSource.Cast(Of Object)()
    '                            Dim obj As Object = (From oo In comboBoxSource
    '                                                 Let prop = oo.[GetType]().GetProperty(cmbColumn.SelectedValuePath)
    '                                                 Where prop IsNot Nothing AndAlso prop.GetValue(oo).Equals(boundProperyValue) Select oo).FirstOrDefault()
    '                            If obj IsNot Nothing Then
    '                                ' Get the Name (DisplayMemberPath) of the Category object 

    '                                If String.IsNullOrEmpty(cmbColumn.DisplayMemberPath) Then
    '                                    propertyValue = obj.[GetType]().ToString()
    '                                Else
    '                                    Dim displayNameProperty As PropertyInfo = obj.[GetType]().GetProperty(cmbColumn.DisplayMemberPath)
    '                                    If displayNameProperty IsNot Nothing Then
    '                                        Dim displayName As Object = displayNameProperty.GetValue(obj)
    '                                        If displayName IsNot Nothing Then
    '                                            propertyValue = displayName.ToString()
    '                                        End If
    '                                    End If
    '                                End If
    '                            End If
    '                        Else
    '                            ' Export the scalar property value of the selected object
    '                            '                             * specified by the SelectedValuePath property of the DataGridComboBoxColumn 

    '                            propertyValue = boundProperyValue.ToString()
    '                        End If
    '                    ElseIf Not String.IsNullOrEmpty(cmbColumn.DisplayMemberPath) Then
    '                        ' Get the Name (DisplayMemberPath) property of the selected Category object 

    '                        Dim pi2 As PropertyInfo = boundProperyValue.[GetType]().GetProperty(cmbColumn.DisplayMemberPath)

    '                        If pi2 IsNot Nothing Then
    '                            Dim displayName As Object = pi2.GetValue(boundProperyValue)
    '                            If displayName IsNot Nothing Then
    '                                propertyValue = displayName.ToString()
    '                            End If
    '                        End If
    '                    Else
    '                        propertyValue = it.[GetType]().ToString()
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If
    '    Return propertyValue
    'End Function

    Shared Function GetValueFollowPath(it As Object, propertyName As String, DisplayMemberPath As String) As Object
        If Not DisplayMemberPath.Contains(".") Then
            Dim prps = propertyName.Split(".")
            Dim value = it.GetType().GetProperty(prps(0)).GetValue(it)
            For Each px In prps.Skip(1)
                value = value.GetType().GetProperty(px).GetValue(value)
            Next
            Return value.GetType().GetProperty(DisplayMemberPath).GetValue(value)
        Else
            Dim prps = propertyName.Split(".")
            Dim value = it.GetType().GetProperty(prps(0)).GetValue(it)
            For Each px In prps.Skip(1)
                value = value.GetType().GetProperty(px).GetValue(value)
            Next
            Return value.GetType().GetProperty(DisplayMemberPath.Split(".")(1)).GetValue(value)
        End If
    End Function
End Class


