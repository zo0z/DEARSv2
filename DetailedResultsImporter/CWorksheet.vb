Imports DocumentFormat.OpenXml.Spreadsheet
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml
Imports System.Text.RegularExpressions
Imports Microsoft.Office.Interop

Public Class CWorkbook
	Dim IndexColorsValue() As Integer = {&H0, &HFFFFFF, &HFF0000, &HFF00, &HFF, &HFFFF00, &HFF00FF, &HFFFF, &H0, &HFFFFFF, &HFF0000, &HFF00, &HFF, &HFFFF00, &HFF00FF, &HFFFF,
	 &H800000, &H8000, &H80, &H808000, &H800080, &H8080, &HC0C0C0, &H808080, &H9999FF, &H993366, &HFFFFCC, &HCCFFFF, &H660066, &HFF8080, &H66CC, &HCCCCFF, &H80,
	   &HFF00FF, &HFFFF00, &HFFFF, &H800080, &H800000, &H8080, &HFF, &HCCFF, &HCCFFFF, &HCCFFCC, &HFFFF99, &H99CCFF, &HFF99CC, &HCC99FF, &HFFCC99, &H3366FF,
	  &H33CCCC, &H3300, &H99CC00, &HFFCC00, &HFF9900, &HFF6600, &H666699, &H969696, &H3366, &H339966, &H333300, &H993300, &H993366, &H333399, &H333333, &HFFFFFF, &HFFFFFF
	 }

	Dim xlWB As SpreadsheetDocument
	Dim _ss As Stylesheet
	Friend _sst As SharedStringTable
	Dim _wb As Workbook
	Dim _wshs As List(Of CWorksheet)
	Dim _readop As Boolean = False

	Public FillColors As List(Of String)
	Public FinStyles As List(Of String)
    Dim tmpFile As String = My.Computer.FileSystem.SpecialDirectories.Temp + "\Temp134.xlsx"
	Public Sub Close()
		xlWB.Close()
        IO.File.Delete(tmpFile)
	End Sub
	Public Sub New(ByVal FileName As String, Optional ByVal Editable As Boolean = True)
		If IO.File.Exists(FileName) Then
            'File is already there read from it and form data structures

            IO.File.Copy(FileName, tmpFile, True)
            xlWB = SpreadsheetDocument.Open(tmpFile, False)
			_readop = True
		Else
			'File does not exist, createit and start forming data structures
			xlWB.Close()
			Throw New NotImplementedException("Later")
		End If

		_ss = xlWB.WorkbookPart.WorkbookStylesPart.Stylesheet
		_sst = xlWB.WorkbookPart.SharedStringTablePart.SharedStringTable
		Dim Fills = _ss.Elements(Of Fills).Single().Elements(Of Fill).ToList()
		FillColors = New List(Of String)
		For Each f In Fills
			FillColors.Add(ChaseFillColor(f))
		Next

		_wshs = New List(Of CWorksheet)()
		_wb = xlWB.WorkbookPart.Workbook
		Dim Styles = _ss.Elements(Of CellFormats).Single().Elements(Of CellFormat)().ToList()
		FinStyles = Styles.ConvertAll(Of String)(Function(s)
													 If s.ApplyFill IsNot Nothing AndAlso s.ApplyFill.Value Then
														 Return FillColors(s.FillId.Value)
													 Else
														 Return Nothing
													 End If
												 End Function)
	End Sub
	Public Function GetWorksheet(ByVal SheetName As String) As CWorksheet
		Dim ws As New CWorksheet(xlWB, Me, SheetName, _readop)
		_wshs.Add(ws)
		Return ws
	End Function
	Public Function GetWorksheetNames() As List(Of String)
		Return _wb.Sheets.Elements(Of Sheet).ToList.ConvertAll(Of String)(Function(sh) sh.Name.Value).ToList()
	End Function
	Private Function ChaseFillColor(ByVal mcellFill As Fill) As String
		Dim mpattern As PatternFill = mcellFill.Elements(Of PatternFill).Single()
		Select Case mpattern.PatternType.Value
			Case PatternValues.Solid
				If mpattern.Descendants(Of ForegroundColor).Count <= 0 Then
					Return mpattern.BackgroundColor.Rgb.Value
				Else
					Dim bgClr As ForegroundColor = mpattern.Descendants(Of ForegroundColor).Single()
					If IsNothing(bgClr.Rgb) Then
						'No rgb so it is probably indexed
						If bgClr.Indexed IsNot Nothing Then
							Return IndexColorsValue(bgClr.Indexed.Value).ToString("X6")
						Else
							Return "FFFFFF"
						End If
					Else
						Return bgClr.Rgb.Value
					End If
				End If
			Case PatternValues.None
				Return "FFFFFF"
			Case Else
				Return mpattern.PatternType.Value
		End Select
		Return "FFFFFF"
	End Function
End Class

Public Class CWorksheet
	Const MaxRowWidth As Integer = 256
	Dim _wsh As Worksheet
	Dim _SheetName As String
	Dim mcells As SortedList(Of Integer, String)
	Private cols As List(Of CColumn)
	Private wb As CWorkbook	'Back reference to workbook

	Public _ranges As SortedList(Of Integer, CRange)
	Public erange As String
	Public Function GetWorksheetName() As String
		Return Me._SheetName
	End Function

	Friend Sub New(ByVal xlWB As SpreadsheetDocument, ByVal wb As CWorkbook, ByVal SheetName As String, ByVal _readop As Boolean)
		Me.wb = wb
		_SheetName = SheetName
		'Trace.WriteLine("Creating/Opening worksheet from file: " & FileName)
		'Trace.WriteLine("SHEETNAME: " & SheetName)
		'Dim xlWb As SpreadsheetDocument
		Dim sh As Sheet = xlWB.WorkbookPart.Workbook.Sheets.Elements(Of Sheet).SingleOrDefault(Function(s) s.Name = SheetName)
		If sh Is Nothing Then
			'Trace.WriteLine("Sheet Does not exist")
			xlWB.Close()
			Throw New ArgumentException("Sheet Does not exist", "SheetName")
		End If


		_wsh = DirectCast(xlWB.WorkbookPart.GetPartById(sh.Id), WorksheetPart).Worksheet
		'MaxRowWidth = CRange.RangeWidth(_wsh.SheetDimension.Reference)
		_ranges = New SortedList(Of Integer, CRange)


		If _readop Then
			'Start formulating data structures
			Dim mcellcont As MergeCells = Nothing
			If _wsh.Elements(Of MergeCells).Count > 0 Then
				mcellcont = _wsh.Elements(Of MergeCells).Single()
			End If

			If mcellcont IsNot Nothing Then
				mcells = New SortedList(Of Integer, String)( _
				  mcellcont.Elements(Of MergeCell).ToDictionary(Of Integer, String)(Function(m) RangeSelector(m.Reference), Function(m) m.Reference))
			End If
			'Trace.WriteLine(TimeOfDay)
			Dim _cols = _wsh.Elements(Of Columns).Single().Elements(Of Spreadsheet.Column)().ToList

			cols = _cols.ConvertAll(Of CColumn)(AddressOf CColumn.MakeColumn)

			erange = GetCurrentRegion()

			'Trace.WriteLine(TimeOfDay)

			'Prepare styles here

			'Read cells and form ranges element
			Dim MaxBackSel As Integer = Integer.MaxValue
			If mcellcont IsNot Nothing Then
				MaxBackSel = mcells.Max(Of Integer)(Function(m) CRange.RangeHeight(m.Value) * MaxRowWidth)
			End If

			

			Dim sheetData As SheetData = _wsh.Elements(Of SheetData).Single()
			Dim rows As IEnumerable(Of Row) = sheetData.Elements(Of Row)()
			For i As Integer = 0 To rows.Count() - 1
				Dim rw As Row = rows(i)
				Dim cells As IEnumerable(Of Cell) = rw.Elements(Of Cell)()

				For j As Integer = 0 To cells.Count - 1
					Dim cl As Cell = cells(j)
					'If cl.CellReference.Value = "B5" Then
					'	Beep()
					'End If
					If Not IsCellInRange(erange, CRange.RangeColumn(cl.CellReference), rw.RowIndex.Value) Then
						Exit For
					End If
					'Is this cell part of a merged cell range or an unmerged cell
					Dim cellSel As Integer = RangeSelector(cl.CellReference.Value)
					'Check if value exists as key. If it does then this cell is part of a merge cell range and actually it is the first. 
					'If it Is Not there Then we take all the elements with selectors greater or equal to current selector and check 
					'if cell in range. If it is it is part of a merged cell range other wise it is a single cell
					Dim CellIsSingle As Boolean = False
					Dim cont_p As KeyValuePair(Of Integer, String)
					If (mcellcont IsNot Nothing) AndAlso mcells.ContainsKey(cellSel) Then
						'Cell is part of range (first)
						cont_p = New KeyValuePair(Of Integer, String)(cellSel, mcells(cellSel))
						'TODO CREATE NEW RANGE FOR ALL MERGED AREA
						Dim r As New CRange
						r.Reference = cont_p.Value
						r.Column = CRange.RangeColumn(cl.CellReference.Value)
						r.Row = rw.RowIndex.Value
						r.NRows = CRange.RangeHeight(cont_p.Value)
						r.NCols = CRange.RangeWidth(cont_p.Value)
						Dim cv = GetCellValue(cl)
						If cv Is Nothing Then
							r.HasValue = False
						Else
							r.HasValue = True
							r.Value = cv
							If (cl.DataType IsNot Nothing) AndAlso (cl.DataType.HasValue) Then
								r.DataType = cl.DataType.Value
							End If
						End If
						r.Width = CalculateRangeWidth(cols, r)
						If r.Width > 0 Then
							r.Style.BGColor = "FFFFFF"
							If (cl.StyleIndex IsNot Nothing) AndAlso (wb.FinStyles(cl.StyleIndex.Value) IsNot Nothing) Then
								r.Style.BGColor = wb.FinStyles(cl.StyleIndex.Value)
							End If
                            _ranges.Add(cellSel, r)
                        Else
                            _ranges.Add(cellSel, r)
                        End If
						j += r.NCols - 1
						'Remove(cells)
						If r.NRows > 1 Then
							Dim r1 As Integer = CRange.RangeRow(cont_p.Value)
							Dim r2 As Integer = r1 + CRange.RangeHeight(cont_p.Value) - 1
							Dim c1 As Integer = CRange.RangeColumn(cont_p.Value)
							Dim c2 As Integer = c1 + CRange.RangeWidth(cont_p.Value) - 1
							Dim rm As Row = rw
							For z As Integer = (r1 + 1) To r2
								Dim zz = z
								rm = rm.NextSibling
								For y = c1 To c2
									Dim yy = y
									'Snap cell out
									rm.Elements(Of Cell).First(Function(c) c.CellReference.Value = CColumn.GetColumnName(yy) & zz).Remove()
								Next
							Next
						End If
						'ElseIf (mcellcont IsNot Nothing) Then
						'	Dim lim As Integer = System.Math.Max((cellSel - MaxBackSel), 0)
						'	Dim cands = mcells.Where(Function(p) (p.Key < cellSel) And (p.Key > lim))
						'	Dim found As Boolean = False
						'	For k As Integer = cands.Count - 1 To 0 Step -1
						'		If IsCellInRange(cands(k).Value, RangeColumn(cl.CellReference), rw.RowIndex.Value) Then
						'			found = True
						'			Exit For
						'		End If
						'	Next
						'	If found Then
						'		Continue For
						'	Else
						'		CellIsSingle = True
						'	End If
					Else
						CellIsSingle = True
					End If
					If CellIsSingle Then
						'TODO CREATE NEW RANGE CONTAINING ONLY CELL
						Dim cv = GetCellValue(cl)
						'If cv Is Nothing Then
						'	Continue For
						'End If
						Dim r As New CRange
						r.Reference = cl.CellReference.Value
						r.Column = CRange.RangeColumn(cl.CellReference.Value)
						r.Row = rw.RowIndex.Value
						r.NRows = 1
						r.NCols = 1

						If cv Is Nothing Then
							r.HasValue = False
						Else
							r.HasValue = True
							r.Value = cv
							If (cl.DataType IsNot Nothing) AndAlso (cl.DataType.HasValue) Then
								r.DataType = cl.DataType.Value
							End If
						End If
						r.Width = CalculateRangeWidth(cols, r)
						If r.Width > 0 Then
							r.Style.BGColor = "FFFFFF"
							If (cl.StyleIndex IsNot Nothing) AndAlso (wb.FinStyles(cl.StyleIndex.Value) IsNot Nothing) Then
								r.Style.BGColor = wb.FinStyles(cl.StyleIndex.Value)
							End If
                            _ranges.Add(cellSel, r)
                        Else
                            _ranges.Add(cellSel, r)
                        End If
					Else

					End If
				Next
			Next
		End If
	End Sub
	Function ExtractDataFromSubrange(ByVal Subrange As String) As List(Of String)
		Dim data As New List(Of String)
		Dim rnSel As Integer = Me.RangeSelector(Subrange)
		Dim base As CRange = Me._ranges(rnSel)
		Dim crn As CRange = Nothing
		Dim colInc As Integer = Me._ranges(rnSel).NCols
		Dim i As Integer = 0
		For icol As Integer = base.Column To (base.Column + CRange.RangeWidth(Subrange) - 1) Step base.NCols
			If Me._ranges.TryGetValue(rnSel + i * base.NCols, crn) Then
				data.Add(crn.Value)
			Else
				data.Add(Nothing)
			End If
			i += 1
		Next
		Return data
	End Function
	Function GetCellValue(ByVal cl As Cell) As String
		If cl Is Nothing Then
			Return Nothing
		ElseIf cl.DataType IsNot Nothing Then
			If cl.DataType.Value = CellValues.SharedString Then
				Dim ssi As SharedStringItem = wb._sst.Elements(Of SharedStringItem)()(Integer.Parse(cl.CellValue.Text))
				Return ssi.Text.Text
			ElseIf cl.DataType.Value = CellValues.InlineString Then
				Return cl.InlineString.Text.Text
			ElseIf cl.CellValue IsNot Nothing Then
				Return cl.CellValue.Text
			Else
				Return Nothing
			End If
		ElseIf cl.CellValue IsNot Nothing Then
			Return cl.CellValue.Text
		Else
			Return Nothing
		End If
	End Function
	Private Function GetCell(ByVal irow As Integer, ByVal icol As Integer) As Cell
		'If column is hidden ignore cell
		Dim contcol = Me.cols.Where(Function(col) col.Min <= icol And col.Max >= icol)
		If contcol.Count() > 0 Then
			If contcol.Single().Hidden Then
                Return Nothing
			End If
		End If
		Dim cl As Cell = Nothing
		Dim row = _wsh.Elements(Of SheetData).Single().Elements(Of Row).FirstOrDefault(Function(r) r.RowIndex.Value = irow)
		If row IsNot Nothing Then
			Dim ref = CColumn.GetColumnName(icol) & irow
			cl = row.Elements(Of Cell).FirstOrDefault(Function(c) c.CellReference.Value = ref)
		End If
		Return cl
    End Function
    Private Function IsColumnHidden(icol As Integer) As Boolean
        Dim contcol = Me.cols.Where(Function(col) col.Min <= icol And col.Max >= icol)
        If contcol.Count() > 0 Then
            If contcol.Single().Hidden Then
                Return True
            End If
        End If
        Return False
    End Function
	Private Function GetCellvalue(ByVal sheetData As SheetData, ByVal icol As Integer, ByVal irow As Integer) As CellValue
		Dim row As Row = sheetData.Descendants(Of Row).SingleOrDefault(Function(r) r.RowIndex.Value = irow)
		If row Is Nothing Then
			Return Nothing
		End If
		Dim cell As Cell = row.Descendants(Of Cell).SingleOrDefault(Function(c) c.CellReference.Value = CColumn.GetColumnName(icol) & irow)
		If cell Is Nothing Then
			Return Nothing
		End If
		If cell.DataType IsNot Nothing Then
			If cell.DataType.Value = CellValues.SharedString Then
				Dim ssi As SharedStringItem = wb._sst.Descendants(Of SharedStringItem)()(Integer.Parse(cell.CellValue.Text))
				Dim cv As New CellValue(ssi.Text.InnerText)
				Return cv
			End If
		End If

		Return cell.CellValue
	End Function

	Private Function GetCurrentRegion() As String
		Dim sheetDims As SheetDimension = _wsh.SheetDimension
		Dim sheetData As SheetData = _wsh.Descendants(Of SheetData).Single()

		Dim icol As Integer = 7
		Dim irow As Integer = 5

		Dim maxRow As Integer = CRange.RangeHeight(sheetDims.Reference.Value)
		Dim maxCol As Integer = CRange.RangeWidth(sheetDims.Reference.Value)

		Dim lastIncCol As Boolean = False
		Dim selector As Integer = 0
		Dim ref As String = ""
		While IsCellInRange(sheetDims.Reference.Value, icol, irow)
			'Simple: check cell if empty move down else move right
			selector = irow * MaxRowWidth + icol
			Dim mergedRange As Boolean = mcells.TryGetValue(selector, ref)

			'If (GetCellValue(sheetData, icol, irow) Is Nothing) Xor (GetCellValue(GetCell(irow, icol)) Is Nothing) Then
			'	Beep()
			'End If

            If (GetCellValue(sheetData, icol, irow) Is Nothing) Then
                If mergedRange Then
                    irow += CRange.RangeHeight(ref)
                Else
                    irow += 1
                End If
                'Try to see if this row seals the above region
                Dim sealrow As Boolean = True
                Dim i As Integer
                For i = 1 To icol
                    If GetCellValue(sheetData, i, irow) IsNot Nothing Then
                        sealrow = False
                        Exit For
                    End If
                Next
                If sealrow Then
                    Exit While
                End If
                lastIncCol = False
            Else
                If mergedRange Then
                    icol += CRange.RangeWidth(ref)
                Else
                    icol += 1
                End If
                lastIncCol = True
            End If
		End While
		If lastIncCol Then
			irow = maxRow
		End If
		Dim lastRow As Integer = System.Math.Min(maxRow, irow)
        Dim lastCol As Integer = System.Math.Min(maxCol, icol - 1)
        'Remove last cols if hidden
        While IsColumnHidden(lastCol)
            lastCol -= 1
        End While
        'Remove empty rows
        Dim done As Boolean = False
        For j As Integer = lastRow To 1 Step -1
            For k As Integer = lastCol To 1 Step -1
                If GetCellValue(sheetData, k, j) IsNot Nothing Then
                    done = True
                    lastRow = j
                    Exit For
                End If
            Next
            If done Then
                Exit For
            End If
        Next
        Return "A1:" & CColumn.GetColumnName(System.Math.Min(maxCol, lastCol)) & System.Math.Min(maxRow, lastRow)
    End Function
	'Private Function GetCurrentRegion() As String
	'	Dim sheetDims As SheetDimension = _wsh.SheetDimension
	'	Dim sheetData As SheetData = _wsh.Descendants(Of SheetData).Single()

	'	Dim icol As Integer = 7
	'	Dim irow As Integer = 5

	'	Dim maxRow As Integer = CRange.RangeHeight(sheetDims.Reference.Value)
	'	Dim maxCol As Integer = CRange.RangeWidth(sheetDims.Reference.Value)

	'	Dim lastIncCol As Boolean = False
	'	Dim selector As Integer = 0
	'	Dim ref As String = ""
	'	erange = sheetDims.Reference.Value

	'	While IsCellInRange(erange, icol, irow)
	'		'Simple: check cell if empty move down else move right
	'		selector = irow * MaxRowWidth + icol
	'		Dim mergedRange As Boolean = mcells.TryGetValue(selector, ref)
	'		'If icol = 46 Then
	'		'	Beep()
	'		'End If
	'		If GetCellValue(GetCell(irow, icol)) Is Nothing Then
	'			If mergedRange Then
	'				irow += CRange.RangeHeight(ref)
	'			Else
	'				irow += 1
	'			End If
	'			'Try to see if this row seals the above region
	'			Dim sealrow As Boolean = True
	'			Dim i As Integer
	'			For i = 1 To icol
	'				If GetCellValue(GetCell(irow, i)) IsNot Nothing Then
	'					sealrow = False
	'					Exit For
	'				End If
	'			Next
	'			If sealrow Then
	'				Exit While
	'			End If
	'			lastIncCol = False
	'		Else
	'			If mergedRange Then
	'				icol += CRange.RangeWidth(ref)
	'			Else
	'				icol += 1
	'			End If
	'			lastIncCol = True
	'		End If
	'	End While
	'	If lastIncCol Then
	'		irow = maxRow
	'	End If
	'	Return "A1:" & CColumn.GetColumnName(System.Math.Min(maxCol, icol - 1)) & System.Math.Min(maxRow, irow - 1)
	'End Function

	Private Function WidthContribution(ByVal minCol As Integer, ByVal maxCol As Integer, ByVal col As CColumn) As Double
		'Find Intersection range
		Dim max As Integer = System.Math.Min(maxCol, col.Max)
		Dim min As Integer = System.Math.Max(minCol, col.Min)
		If col.Hidden Then
			Return 0
		End If
		Return (max - min + 1) * col.Width
	End Function
	Function RangeSelector(ByVal rangeRef As String) As Integer
		Return CRange.RangeRow(rangeRef) * MaxRowWidth + CRange.RangeColumn(rangeRef)
	End Function

	Function RangeSelector(ByVal rangeRef As MergeCell) As Integer
		Return RangeSelector(rangeRef.Reference)
	End Function

	Function IsCellInRange(ByVal range As String, ByVal ColumnIndex As Integer, ByVal RowIndex As Integer) As Boolean
		Dim scol As Integer = CRange.RangeColumn(range)
		Dim srow As Integer = CRange.RangeRow(range)
		Return (ColumnIndex >= scol) And (ColumnIndex < scol + CRange.RangeWidth(range)) And (RowIndex >= srow) And (RowIndex < srow + CRange.RangeHeight(range))
	End Function

	Private Function CalculateRangeWidth(ByRef cols As List(Of CColumn), ByVal rn As CRange) As Double
		Dim IncludedColumns As New List(Of CColumn)
		For Each col In cols
			If DoRangesIntersect(col.Min, col.Max, rn.Column, rn.Column + rn.NCols - 1) Then
				IncludedColumns.Add(col)
			End If
		Next
		Dim width As Double = 0
		If IncludedColumns.Count = 0 Then
			width = 1 'Column uses default parameters
		End If
		For Each colu In IncludedColumns
			width += WidthContribution(rn.Column, rn.Column + rn.NCols - 1, colu)
		Next
		Return width
	End Function
	Function DoRangesIntersect(ByVal r1colmin As Integer, ByVal r1colmax As Integer, ByVal r2colmin As Integer, ByVal r2colmax As Integer) As Boolean
		If (r2colmax < r1colmin) Then
			Return False
		End If
		If (r1colmax < r2colmin) Then
			Return False
		End If
		Return True
	End Function
	Sub Debug_ShowRanges(ByVal FileName As String, ByVal FastForwardTill As String)
		Dim xApp As Excel.Application = Nothing
		Try
			xApp = New Excel.Application()

			Dim xWB As Excel.Workbook = xApp.Workbooks.Open(FileName)
			Dim xWs As Excel.Worksheet = xWB.Worksheets(_SheetName)
			xWs.Activate()
			xApp.Visible = True
			xWs.Range(erange).Select()
			Dim ff As Boolean = True
            Threading.Thread.Sleep(2000)
			If FastForwardTill = "ShowERangeOnly" Then
				xWs.Range(erange.Split(":")(1)).Select()
                Threading.Thread.Sleep(2000)
				xApp.Workbooks.Close()
				xApp.Quit()
				Return
			End If
			For Each rn In _ranges
				If Not (rn.Value.Reference.StartsWith(FastForwardTill)) And ff Then
					Continue For
				Else
					ff = False
				End If
				xWs.Range(rn.Value.Reference).Select()
				Threading.Thread.Sleep(200)
			Next
			xApp.Workbooks.Close()
			xApp.Quit()
		Catch ex As System.Runtime.InteropServices.COMException
			If xApp IsNot Nothing Then
				xApp.Workbooks.Close()
				xApp.Quit()
			End If
			Trace.WriteLine("Exception occured quitting ....")
			Trace.WriteLine(ex.Message)
			Trace.WriteLine(ex.ErrorCode.ToString("X"))
		End Try
	End Sub

End Class

Public Structure CRange
	Public Shared ColumnNameRegex As New System.Text.RegularExpressions.Regex("[A-Za-z]+")
	Public Shared RowNumRegex As New System.Text.RegularExpressions.Regex("[1-9][0-9]*")
	Public Shared Function RangeRow(ByVal subrange As String) As Integer
		Dim RowNumMatch As MatchCollection = RowNumRegex.Matches(subrange)
		Dim yStart As Integer = Integer.Parse(RowNumMatch.Item(0).Value)
		Return yStart
	End Function
	Public Shared Function RangeColumn(ByVal subrange As String) As Integer
		Dim ColumnNameMatch As MatchCollection = ColumnNameRegex.Matches(subrange)
		Dim xStart As Integer = CColumn.GetColumnNumber(ColumnNameMatch.Item(0).Value)
		Return xStart
	End Function
	Public Shared Function RangeWidth(ByVal subrange As String) As Integer
		Dim ColumnNameMatch As MatchCollection = ColumnNameRegex.Matches(subrange)
		Dim xStart As Integer = CColumn.GetColumnNumber(ColumnNameMatch.Item(0).Value)
		Dim xEnd As Integer = CColumn.GetColumnNumber(ColumnNameMatch.Item(1).Value)
		Return (xEnd - xStart) + 1
	End Function
	Public Shared Function RangeHeight(ByVal subrange As String) As Integer
		Dim RowNumMatch As MatchCollection = RowNumRegex.Matches(subrange)
		Dim yStart As Integer = Integer.Parse(RowNumMatch.Item(0).Value)
		Dim yEnd As Integer = Integer.Parse(RowNumMatch.Item(1).Value)
		Return (yEnd - yStart) + 1
	End Function
	
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

    Shared Function MutateRange(subrange As String, p2 As Integer, p3 As Integer, p4 As Integer, p5 As Integer) As String
        Dim x1, y1, x2, y2 As Integer
        x1 = CRange.RangeColumn(subrange)
        y1 = CRange.RangeRow(subrange)
        x2 = x1 + CRange.RangeWidth(subrange) - 1
        y2 = y1 + CRange.RangeHeight(subrange) - 1
        Return CColumn.GetColumnName(x1 + p2) & (y1 + p3) & ":" & CColumn.GetColumnName(x2 + p4) & (y2 + p5)
    End Function

End Structure

Public Structure CStyle
	Public Font As String
	Public BGColor As String
	'Public cNumFmt As NumFmt
End Structure

Structure CColumn
	Public Shared Function GetColumnName(ByVal ColNumber As Integer) As String
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
	Public Shared Function GetColumnNumber(ByVal ColumnName As String) As Integer
		Dim j As Integer
		Dim numb As Integer = 0
		For i As Integer = ColumnName.Length - 1 To 0 Step -1
			j = ColumnName.Length - 1 - i
			numb = numb + (AscW(ColumnName.Chars(i)) - 64) * System.Math.Pow(26, j)
		Next
		Return numb
	End Function
	Public Shared Function MakeColumn(ByVal col As Spreadsheet.Column) As CColumn
		Dim cl As CColumn
		cl.Min = col.Min.Value
		cl.Max = col.Max.Value
		If col.Hidden IsNot Nothing Then
			cl.Hidden = col.Hidden.Value
		End If
		If col.Width IsNot Nothing Then
			cl.Width = col.Width.Value
			If col.Width.Value = 0 Then
				cl.Hidden = True
			End If
		End If
		Return cl
	End Function

	Dim Min As Integer
	Dim Max As Integer
	Dim Width As Double
	Dim Hidden As Boolean
End Structure
