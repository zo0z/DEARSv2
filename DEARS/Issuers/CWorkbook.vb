Imports DocumentFormat.OpenXml.Spreadsheet
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml
Imports System.Text.RegularExpressions
Namespace ExcelSimplified
    Public Module ExcelSimple
        Const MaxRowWidth = 256
        Private RowNumRegex As New System.Text.RegularExpressions.Regex("[1-9][0-9]*")
        Private ColumnNameRegex As New System.Text.RegularExpressions.Regex("[A-Za-z]+")
        Private IndexColorsValue() As Integer = {&H0, &HFFFFFF, &HFF0000, &HFF00, &HFF, &HFFFF00, &HFF00FF, &HFFFF, &H0, &HFFFFFF, &HFF0000, &HFF00, &HFF, &HFFFF00, &HFF00FF, &HFFFF,
          &H800000, &H8000, &H80, &H808000, &H800080, &H8080, &HC0C0C0, &H808080, &H9999FF, &H993366, &HFFFFCC, &HCCFFFF, &H660066, &HFF8080, &H66CC, &HCCCCFF, &H80,
         &HFF00FF, &HFFFF00, &HFFFF, &H800080, &H800000, &H8080, &HFF, &HCCFF, &HCCFFFF, &HCCFFCC, &HFFFF99, &H99CCFF, &HFF99CC, &HCC99FF, &HFFCC99, &H3366FF,
        &H33CCCC, &H3300, &H99CC00, &HFFCC00, &HFF9900, &HFF6600, &H666699, &H969696, &H3366, &H339966, &H333300, &H993300, &H993366, &H333399, &H333333, &HFFFFFF, &HFFFFFF
          }
        Public Enum CBorders
            None = 0
            Left = 1
            Right = 2
            Top = 4
            Bottom = 8
            All = CBorders.Left Or CBorders.Top Or CBorders.Right Or CBorders.Bottom
        End Enum
        Public Class CWorkbook
            Private _Worksheets As Dictionary(Of String, CWorksheet)
            Private _Stylesheet As CStyleSheet
            Private xlWB As SpreadsheetDocument
            Private _SharedStringsTable As List(Of String)

            Public Sub New(ByVal FileName As String, ByVal Editable As Boolean)
                xlWB = SpreadsheetDocument.Create(FileName, SpreadsheetDocumentType.Workbook, True)
                _Worksheets = New Dictionary(Of String, CWorksheet)
                _SharedStringsTable = New List(Of String)
                _Stylesheet = New CStyleSheet()
            End Sub
            Public Function AddSharedStringItem(ByVal str As String) As Integer
                Dim loc As Integer = _SharedStringsTable.IndexOf(str)
                If loc < 0 Then
                    loc = _SharedStringsTable.Count
                    _SharedStringsTable.Add(str)
                End If
                Return loc
            End Function
            Public Function CreateNewWorksheet(ByVal SheetName As String, Optional ByVal RTL As Boolean = False) As CWorksheet
                If _Worksheets.ContainsKey(SheetName) Then
                    Throw New Exception("Sheet already exists")
                Else
                    Dim ws As New CWorksheet(SheetName, RTL)
                    _Worksheets.Add(SheetName, ws)
                    Return ws
                End If
            End Function
            Public Sub Save()
                Dim wb As Workbook = New Workbook()
                If xlWB.WorkbookPart Is Nothing Then
                    xlWB.AddWorkbookPart()
                End If
                xlWB.WorkbookPart.Workbook = wb

                If _Worksheets.Count > 0 Then
                    wb.AppendChild(Of Sheets)(New Sheets())
                    Dim shID As Integer = 1
                    For Each ws In _Worksheets
                        Dim wsp As WorksheetPart = xlWB.WorkbookPart.AddNewPart(Of WorksheetPart)()
                        wb.Sheets.AppendChild(Of Sheet)(New Sheet() With {.Name = ws.Value.SheetName, .Id = xlWB.WorkbookPart.GetIdOfPart(wsp), .SheetId = shID})
                        ws.Value.Save(wsp, Me, _Stylesheet)
                        shID += 1
                    Next
                End If

                If xlWB.WorkbookPart.WorkbookStylesPart Is Nothing Then
                    Dim wbsp = xlWB.WorkbookPart.AddNewPart(Of WorkbookStylesPart)()
                End If
                Dim ss As Stylesheet = New Stylesheet()
                xlWB.WorkbookPart.WorkbookStylesPart.Stylesheet = ss
                _Stylesheet.Save(ss)


                xlWB.WorkbookPart.Workbook.Save()
                If _SharedStringsTable.Count > 0 Then
                    Dim sstp = xlWB.WorkbookPart.AddNewPart(Of SharedStringTablePart)()
                    sstp.SharedStringTable = New SharedStringTable() With {.Count = _SharedStringsTable.Count}
                    For Each _str In _SharedStringsTable
                        sstp.SharedStringTable.AppendChild(Of SharedStringItem)(New SharedStringItem(New Text(_str)))
                    Next
                End If

                xlWB.Close()
            End Sub
        End Class

        Class CStyleSheet
            Private _Fills As New List(Of String)
            Private _Fonts As New List(Of String)
            Private _Styles As New List(Of CCellFromat)
            Private _Borders As New List(Of BorderData)
            Private Structure BorderData
                Dim Border As Integer
                Dim BorderStyle As BorderStyleValues
            End Structure

            Public Sub New()
                _Fills.Add("FFFFFFFF")
                _Fills.Add("Gray125")

                _Fonts.Add("Calibri")
                _Borders.Add(New BorderData() With {.Border = CBorders.None, .BorderStyle = BorderStyleValues.None})

                _Styles.Add(New CCellFromat() With {.FontIndex = 0, .FillIndex = 0})
            End Sub
            Public Sub Save(ByVal ss As Stylesheet)
                ss.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006")
                ss.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac")

                Dim fonts1 As New Fonts() With {.Count = _Fonts.Count, .KnownFonts = True}

                'Dim font1 As New Font() With _
                ' {.FontSize = New FontSize() With {.Val = New DoubleValue(11.0)}, _
                '  .FontName = New FontName() With {.Val = New StringValue("Calibri")}, _
                '  .Color = New Color() With {.Rgb = New HexBinaryValue("00000000")}}
                'fonts1.Append(font1)
                For Each f In _Fonts
                    Dim fontx As New Font() With _
                    {.FontSize = New FontSize() With {.Val = New DoubleValue(11.0)}, _
                     .FontName = New FontName() With {.Val = New StringValue(f)}, _
                     .Color = New Color() With {.Rgb = New HexBinaryValue("00000000")}}
                    fonts1.AppendChild(fontx)
                Next

                Dim fills1 As New Fills() With {.Count = _Fills.Count}

                Dim fill1 As New Fill(New PatternFill() With {.PatternType = PatternValues.None})
                Dim fill2 As New Fill(New PatternFill() With {.PatternType = PatternValues.Gray125})
                fills1.Append(fill1, fill2)
                For i As Integer = 2 To _Fills.Count - 1
                    Dim f = _Fills(i)
                    Dim fillx As New Fill(New PatternFill() With {.PatternType = PatternValues.Solid, .ForegroundColor = New ForegroundColor() With {.Rgb = New HexBinaryValue(f)}})
                    fills1.AppendChild(fillx)
                Next

                Dim borders1 As New Borders() With {.Count = 2}

                Dim border1 As New Border()
                Dim leftBorder1 As New LeftBorder()
                Dim rightBorder1 As New RightBorder()
                Dim topBorder1 As New TopBorder()
                Dim bottomBorder1 As New BottomBorder()
                Dim diagonalBorder1 As New DiagonalBorder()

                border1.Append(leftBorder1)
                border1.Append(rightBorder1)
                border1.Append(topBorder1)
                border1.Append(bottomBorder1)
                border1.Append(diagonalBorder1)
                borders1.Append(border1) '(border2)
                Dim clblack As Color = New Color() With {.Indexed = 64}
                For i As Integer = 1 To _Borders.Count - 1
                    Dim br As New Border()
                    Dim lbr As LeftBorder = Nothing
                    Dim rbr As RightBorder = Nothing
                    Dim tbr As TopBorder = Nothing
                    Dim bbr As BottomBorder = Nothing
                    If (_Borders(i).Border And CBorders.Left) <> 0 Then
                        lbr = New LeftBorder() With {.Color = clblack.CloneNode(True), .Style = _Borders(i).BorderStyle}
                    End If
                    If (_Borders(i).Border And CBorders.Right) <> 0 Then
                        rbr = New RightBorder() With {.Color = clblack.CloneNode(True), .Style = _Borders(i).BorderStyle}
                    End If
                    If (_Borders(i).Border And CBorders.Top) <> 0 Then
                        tbr = New TopBorder() With {.Color = clblack.CloneNode(True), .Style = _Borders(i).BorderStyle}
                    End If
                    If (_Borders(i).Border And CBorders.Bottom) <> 0 Then
                        bbr = New BottomBorder() With {.Color = clblack.CloneNode(True), .Style = _Borders(i).BorderStyle}
                    End If
                    For Each gbr As OpenXmlElement In {lbr, rbr, tbr, bbr}
                        br.AppendChild(gbr)
                    Next
                    borders1.AppendChild(br)
                Next



                Dim cfs As New CellFormats() With {.Count = _Styles.Count}

                Dim cf1 As New CellFormat() With {.FillId = 0, .NumberFormatId = 0, .BorderId = 0, .FormatId = 0}
                cfs.AppendChild(cf1)

                'Dim cf2 As New CellFormat() With {.FillId = 2, .NumberFormatId = 2, .BorderId = 0, .FormatId = 0, _
                ' .ApplyNumberFormat = True, .ApplyFill = True, .FontId = 1, .ApplyFont = True, .Alignment = New Alignment() With {.TextRotation = 90}}
                For i As Integer = 1 To _Styles.Count - 1
                    Dim xf = _Styles(i)
                    Dim cf2 As New CellFormat()
                    cf2.FillId = xf.FillIndex
                    cf2.FontId = xf.FontIndex
                    cf2.BorderId = xf.BorderID
                    cf2.ApplyBorder = True
                    cf2.ApplyFill = True
                    cf2.ApplyFont = True
                    cf2.Alignment = New Alignment()
                    cf2.Alignment.Vertical = VerticalAlignmentValues.Center
                    If xf.HAlign <> 0 Then
                        cf2.Alignment.Horizontal = xf.HAlign
                    End If
                    If xf.Alignment <> 0 Then
                        cf2.Alignment.TextRotation = xf.Alignment
                    End If
                    '		With {.FillId = xf.FillIndex, .NumberFormatId = 0, .BorderId = 1, .FormatId = 0, _
                    '.ApplyNumberFormat = True, .ApplyFill = True, .FontId = xf.FontIndex, .ApplyFont = True, .ApplyBorder = True, _
                    '.Alignment = New Alignment() With {.Horizontal = HorizontalAlignmentValues.Center, .Vertical = VerticalAlignmentValues.Center, .TextRotation = xf.Alignment}, .ApplyAlignment = True}
                    cfs.AppendChild(cf2)
                Next


                ss.Append(fonts1)
                ss.Append(fills1)
                ss.Append(borders1)
                ss.Append(cfs)
            End Sub
            Public Function AddFill(ByVal fill As String) As Integer
                Dim loc As Integer = _Fills.IndexOf(fill)
                If loc < 0 Then
                    loc = _Fills.Count
                    _Fills.Add(fill)
                End If
                Return loc
            End Function
            Public Function AddFont(ByVal font As String) As Integer
                Dim loc As Integer = _Fonts.IndexOf(font)
                If loc < 0 Then
                    loc = _Fonts.Count
                    _Fonts.Add(font)
                End If
                Return loc
            End Function
            Public Function AddBorder(ByVal Border As Integer, ByVal BorderStyle As BorderStyleValues) As Integer
                Dim vx As BorderData
                vx.Border = Border
                vx.BorderStyle = BorderStyle
                Dim loc As Integer = _Borders.IndexOf(vx)
                If loc < 0 Then
                    loc = _Borders.Count
                    _Borders.Add(vx)
                End If
                Return loc
            End Function
            Public Function AddCellFormat(ByVal FontID As Integer, ByVal FillID As Integer, ByVal BorderID As Integer, ByVal Alignment As Integer,
                     ByVal HAlign As HorizontalAlignmentValues) As Integer
                Dim cf As CCellFromat
                cf.FillIndex = FillID
                cf.FontIndex = FontID
                cf.BorderID = BorderID
                cf.Alignment = Alignment
                cf.HAlign = HAlign
                Dim loc As Integer = _Styles.IndexOf(cf)
                If loc < 0 Then
                    loc = _Styles.Count
                    _Styles.Add(cf)
                End If
                Return loc
            End Function
        End Class

        Class CWorksheet
            Private _SheetName As String
            Private _MergedCellsList As SortedList(Of Integer, String)
            Private _Columns As List(Of CColumn)
            Private _ranges As SortedList(Of Integer, Range)
            Private _RTL As Boolean
            Private Function GetRow(ByVal shd As SheetData, ByVal RowIndex As Integer) As Row
                Dim row = shd.Elements(Of Row).FirstOrDefault(Function(r) r.RowIndex.Value = RowIndex)
                If row Is Nothing Then
                    row = shd.AppendChild(Of Row)(New Row() With {.RowIndex = RowIndex, .Height = 18, .CustomHeight = True})
                End If
                Return row
            End Function
            Private Function GetCell(ByVal row As Row, ByVal ColumnIndex As Integer) As Cell
                Dim ref As String = GetColumnName(ColumnIndex) & row.RowIndex.Value
                Dim cell = row.Elements(Of Cell).FirstOrDefault(Function(c) c.CellReference.Value = ref)
                If cell Is Nothing Then
                    Dim prevCell = row.Elements(Of Cell).FirstOrDefault(Function(c) Range.RangeColumn(c.CellReference) > ColumnIndex)
                    If prevCell Is Nothing Then
                        cell = row.AppendChild(Of Cell)(New Cell() With {.CellReference = ref})
                    Else
                        cell = row.InsertBefore(Of Cell)(New Cell() With {.CellReference = ref}, prevCell)
                    End If

                End If
                Return cell
            End Function
            Public Sub Save(ByVal wsp As WorksheetPart, ByVal wb As CWorkbook, ByVal ss As CStyleSheet)
                wsp.Worksheet = New Worksheet()
                wsp.Worksheet.SheetViews = New SheetViews()
                wsp.Worksheet.SheetViews.AppendChild(Of SheetView)(New SheetView() With {.WorkbookViewId = 0, .RightToLeft = _RTL})

                If _Columns.Count > 0 Then
                    Dim cls = wsp.Worksheet.AppendChild(Of Columns)(New Columns())
                    For Each cl In _Columns
                        cls.AppendChild(Of Column)(New Column() With {.Width = cl.Width, .Max = cl.Max, .Min = cl.Min, .CustomWidth = True})
                    Next
                End If

                Dim shd = wsp.Worksheet.AppendChild(Of SheetData)(New SheetData())
                For Each rn In Me._ranges
                    Dim rnc = rn

                    Dim row = GetRow(shd, rn.Value.Row)
                    Dim cell = GetCell(row, rn.Value.Column)

                    Dim FillID As Integer = ss.AddFill(rn.Value.Style.Fill)
                    Dim FontID As Integer = ss.AddFont(rn.Value.Style.Font)
                    Dim BorderID As Integer = ss.AddBorder(rn.Value.Style.Border, rn.Value.Style.BorderStyle)
                    Dim StyleInd As Integer = ss.AddCellFormat(FontID, FillID, BorderID, rn.Value.Style.Alignment, rn.Value.Style.HorizontalAlignment)

                    If StyleInd >= 0 Then
                        cell.StyleIndex = StyleInd
                    End If

                    If rn.Value.HasValue Then
                        cell.DataType = rn.Value.DataType
                        Select Case rn.Value.DataType
                            Case CellValues.SharedString
                                cell.CellValue = New CellValue(wb.AddSharedStringItem(rn.Value.Value))
                            Case CellValues.InlineString
                                cell.InlineString = New InlineString(New Text(rn.Value.Value))
                            Case Else
                                cell.CellValue = New CellValue(rn.Value.Value)
                        End Select
                    ElseIf rn.Value.HasFormula Then
                        cell.CellFormula = New CellFormula(rn.Value.Value)
                    End If

                    If rn.Value.NRows <> 1 Or rn.Value.NCols <> 1 Then
                        For rIndex As Integer = rn.Value.Row To (rn.Value.Row + rn.Value.NRows - 1)
                            row = GetRow(shd, rIndex)
                            For cIndex As Integer = rn.Value.Column To (rn.Value.Column + rn.Value.NCols - 1)
                                cell = GetCell(row, cIndex)
                                cell.StyleIndex = StyleInd
                            Next
                        Next
                    End If
                Next
                If _MergedCellsList.Count > 0 Then
                    Dim mcs = wsp.Worksheet.AppendChild(Of MergeCells)(New MergeCells() With {.Count = _MergedCellsList.Count})
                    For Each mc In _MergedCellsList
                        mcs.AppendChild(Of MergeCell)(New MergeCell() With {.Reference = mc.Value})
                    Next
                End If

            End Sub
            Public Sub New(ByVal SheetName As String, Optional ByVal RightToLeft As Boolean = False)
                _ranges = New SortedList(Of Integer, Range)
                _SheetName = SheetName
                Me._MergedCellsList = New SortedList(Of Integer, String)
                _RTL = RightToLeft
                _Columns = New List(Of CColumn)
            End Sub
            Public Sub SetWidths(ByVal Min As Integer, ByVal Max As Integer, ByVal Width As Double)
                _Columns.Add(New CColumn() With {.Max = Max, .Min = Min, .Width = Width})
            End Sub
            Public ReadOnly Property SheetName As String
                Get
                    Return _SheetName
                End Get
            End Property
            Public Function CreateNewRange(ByVal Column As Integer, ByVal Row As Integer, ByVal Value As String, ByVal DataType As CellValues, ByVal NRows As Integer, ByVal NCols As Integer,
             Optional ByVal Font As String = "Calibri", Optional ByVal BGColor As String = "FFFFFFFF", Optional ByVal Alignment As Integer = 0, Optional ByVal Border As Integer = CBorders.None,
             Optional ByVal BorderStyle As BorderStyleValues = BorderStyleValues.None, Optional ByVal HAlign As HorizontalAlignmentValues = HorizontalAlignmentValues.Center) As Range
                Dim rn As Range
                rn.Column = Column
                rn.Row = Row
                rn.NRows = NRows
                rn.NCols = NCols
                rn.Value = Value
                rn.DataType = DataType
                rn.HasValue = (Value IsNot Nothing)
                rn.Reference = GetColumnName(Column) & Row
                rn.HasFormula = False
                If BGColor IsNot Nothing Then
                    rn.Style.Fill = BGColor
                    rn.Style.Font = Font
                Else
                    rn.Style.Font = Font
                    rn.Style.Fill = Nothing
                End If
                rn.Style.Alignment = Alignment
                rn.Style.Border = Border
                rn.Style.BorderStyle = BorderStyle
                rn.Style.HorizontalAlignment = HAlign

                Me._ranges.Add(Range.RangeSelector(rn.Reference), rn)
                If (NRows <> 1) Or (NCols <> 1) Then
                    'Merged cell range
                    Me._MergedCellsList.Add(Range.RangeSelector(rn.Reference), rn.Reference & ":" & GetColumnName(rn.Column + rn.NCols - 1) & (rn.Row + rn.NRows - 1))
                End If
                Return rn
            End Function
            Public Function CreateNewFormulaRange(ByVal Column As Integer, ByVal Row As Integer, ByVal Formula As String, ByVal NRows As Integer, ByVal NCols As Integer,
              Optional ByVal Font As String = "Calibri", Optional ByVal BGColor As String = "FFFFFFFF", Optional ByVal Alignment As Integer = 0,
              Optional ByVal Border As Integer = CBorders.None, Optional ByVal BorderStyle As BorderStyleValues = BorderStyleValues.None, Optional ByVal HAlign As HorizontalAlignmentValues = HorizontalAlignmentValues.General)

                Dim rn As Range
                rn.Column = Column
                rn.Row = Row
                rn.NRows = NRows
                rn.NCols = NCols
                rn.Value = Formula
                rn.DataType = Nothing
                rn.HasValue = False
                rn.HasFormula = True
                rn.Reference = GetColumnName(Column) & Row

                If BGColor IsNot Nothing Then
                    rn.Style.Fill = BGColor
                    rn.Style.Font = Font
                Else
                    rn.Style.Font = Font
                    rn.Style.Fill = Nothing
                End If
                rn.Style.Alignment = Alignment
                rn.Style.Border = Border
                rn.Style.BorderStyle = BorderStyle
                rn.Style.HorizontalAlignment = HAlign

                Me._ranges.Add(Range.RangeSelector(rn.Reference), rn)
                If (NRows <> 1) Or (NCols <> 1) Then
                    'Merged cell range
                    Me._MergedCellsList.Add(Range.RangeSelector(rn.Reference), rn.Reference & ":" & GetColumnName(rn.Column + rn.NCols - 1) & (rn.Row + rn.NRows - 1))
                End If
                Return rn
            End Function
        End Class

        Structure CColumn
            Dim Min As Integer
            Dim Max As Integer
            Dim Width As Double
        End Structure

        Structure Range
            Public Reference As String
            Public Column As Integer
            Public Row As Integer
            Public NRows As Integer
            Public NCols As Integer
            Public Width As Double
            Public Height As Double
            Public HasValue As Boolean
            Public HasFormula As Boolean
            Public Value As String
            Public DataType As CellValues
            Public Style As CStyle

            Public Shared Function RangeSelector(ByVal rangeRef As String) As Integer
                Return RangeRow(rangeRef) * MaxRowWidth + RangeColumn(rangeRef)
            End Function

            Public Shared Function RangeRow(ByVal subrange As String) As Integer
                Dim RowNumMatch As MatchCollection = RowNumRegex.Matches(subrange)
                Dim yStart As Integer = Integer.Parse(RowNumMatch.Item(0).Value)
                Return yStart
            End Function

            Public Shared Function RangeColumn(ByVal subrange As String) As Integer
                Dim ColumnNameMatch As MatchCollection = ColumnNameRegex.Matches(subrange)
                Dim xStart As Integer = GetColumnNumber(ColumnNameMatch.Item(0).Value)
                Return xStart
            End Function

            Public Shared Function RangeWidth(ByVal subrange As String) As Integer
                Dim ColumnNameMatch As MatchCollection = ColumnNameRegex.Matches(subrange)
                Dim xStart As Integer = GetColumnNumber(ColumnNameMatch.Item(0).Value)
                Dim xEnd As Integer = GetColumnNumber(ColumnNameMatch.Item(1).Value)
                Return (xEnd - xStart) + 1
            End Function

            Public Shared Function RangeHeight(ByVal subrange As String) As Integer
                Dim RowNumMatch As MatchCollection = RowNumRegex.Matches(subrange)
                Dim yStart As Integer = Integer.Parse(RowNumMatch.Item(0).Value)
                Dim yEnd As Integer = Integer.Parse(RowNumMatch.Item(1).Value)
                Return (yEnd - yStart) + 1
            End Function
        End Structure

        Structure CStyle
            Public Font As String
            Public Fill As String
            Public Alignment As Integer
            Public HorizontalAlignment As HorizontalAlignmentValues
            Public Border As Integer
            Public BorderStyle As BorderStyleValues
            'Public cNumFmt As NumFmt
        End Structure

        Structure CCellFromat
            Dim FontIndex As Integer
            Dim FillIndex As Integer
            Dim BorderID As Integer
            Dim Alignment As Integer
            Dim HAlign As HorizontalAlignmentValues
        End Structure

        Public Function GetColumnName(ByVal ColNumber As Integer) As String
            'Verify in range number
            Dim colName As String = ""
            Dim m As Integer = 0
            While (ColNumber > 0)
                m = System.Math.Floor((ColNumber - 1) Mod 26)
                colName = Chr(65 + m) + colName
                ColNumber = Int((ColNumber - m) / 26)
            End While
            Return colName
        End Function

        Public Function GetColumnNumber(ByVal ColumnName As String) As Integer
            Dim j As Integer
            Dim numb As Integer = 0
            For i As Integer = ColumnName.Length - 1 To 0 Step -1
                j = ColumnName.Length - 1 - i
                numb = numb + (AscW(ColumnName.Chars(i)) - 64) * System.Math.Pow(26, j)
            Next
            Return numb
        End Function
    End Module

    'Dim border2 As New Border()

    'Dim leftBorder2 As New LeftBorder() With {.Style = BorderStyleValues.Thin}
    'Dim color2 As New Color() With {.Indexed = 64}
    'leftBorder2.Append(color2)

    'Dim rightBorder2 As New RightBorder() With {.Style = BorderStyleValues.Thin}
    'Dim color3 As New Color() With {.Indexed = 64}
    'rightBorder2.Append(color3)

    'Dim topBorder2 As New TopBorder() With {.Style = BorderStyleValues.Thin}
    'Dim color4 As New Color() With {.Indexed = 64}
    'topBorder2.Append(color4)

    'Dim bottomBorder2 As New BottomBorder() With {.Style = BorderStyleValues.Thin}
    'Dim color5 As New Color() With {.Indexed = 64}
    'bottomBorder2.Append(color5)

    'Dim diagonalBorder2 As New DiagonalBorder()

    'border2.Append(leftBorder2)
    'border2.Append(rightBorder2)
    'border2.Append(topBorder2)
    'border2.Append(bottomBorder2)
    'border2.Append(diagonalBorder2)
End Namespace