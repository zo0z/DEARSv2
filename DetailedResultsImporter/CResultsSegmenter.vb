Imports System.Text.RegularExpressions
Imports Microsoft.Office.Interop

Public Class CResultsSegmeter
	Shared CourseTitleRegex As New Regex("[A-Z][A-Z]\d{4}")
	Public TitleRegions As New List(Of String)
	Public BigHeaderRegions As New List(Of String)
	Public CoursesRegions As New List(Of String)
	Public StudentRegions As New List(Of String)
	Public MarksRegions As New List(Of String)
	Public SummaryRegions As New List(Of String)
	Public GradeRegions As New List(Of String)
	Private SheetName As String

	''' <summary>
	''' This method classifies the segmented worksheet regions into several data segments containing the data we desire.
	''' These are
	''' 1. Title area
	''' 2. Headers areas 3. Courses Data area
	''' 4. Student Data areas
	''' 5. Marks areas
	''' 6. Grades areas
	''' We first encounter the title region, followed by ther headers + courses region followed by the main data area
	''' </summary>
	''' <param name="ws"></param>
	''' <param name="SAna"></param>
	''' <remarks></remarks>
	Sub ClassifySegmentedIslands(ByRef ws As CWorksheet, ByRef SAna As CWorksheetStructureAnalyzer)
		Dim NCols As Integer = CRange.RangeWidth(ws.erange)
		Dim NRows As Integer = CRange.RangeHeight(ws.erange)

		Dim regionsType As Integer = 0
		Dim MarksOrGrades = True
		'Dim PrevGrades = False
		Dim dataColumn As Integer = 0
		Dim summaryColumn As Integer = 0
		Dim dataStartColumn As Integer = 0
		Dim lim As Integer = SAna.Islands.Count - 1
		For i As Integer = 0 To lim
			Dim subrange As String = SAna.Islands(i)
			Select Case regionsType
				Case 0
					'Title areas
					If CRange.RangeWidth(subrange) > 0.9 * NCols Then
						TitleRegions.Add(subrange)
					Else
						If ws._ranges(ws.RangeSelector(subrange)).Style.BGColor = "C0C0C0" Then
							regionsType += 1
							i -= 1
						End If
					End If
				Case 1
					'Big Headers and Courses area
					Dim bgcolor As String = ws._ranges(ws.RangeSelector(subrange)).Style.BGColor
					If bgcolor = "FFFFFF" Or String.IsNullOrWhiteSpace(bgcolor) Then
						'White region header region ended
						'Trace.WriteLine("<HEADER AREA ENDED ===========================================>")
						regionsType += 1

						If BigHeaderRegions.Count < 2 Then
							Dim S1 As String = Nothing
							Trace.WriteLine("ERROR: < 2 Big Header regions")
							For Each subrange In Me.CoursesRegions
								Dim ctitles = ws.ExtractDataFromSubrange(subrange)
								If ctitles.All(Function(t) CourseTitleRegex.IsMatch(t)) Then
									S1 = subrange
									Exit For
								End If
							Next
							Me.BigHeaderRegions.AddRange(Me.CoursesRegions.Where(Function(s) (CRange.RangeRow(s) = CRange.RangeRow(S1)) And s <> S1))
							Me.CoursesRegions.RemoveAll(Function(s) (CRange.RangeRow(s) = CRange.RangeRow(S1)) And (s <> S1))
							If BigHeaderRegions.Count = 2 Then
								summaryColumn = CRange.RangeColumn(Me.BigHeaderRegions(1))
							End If
						End If
						i -= 1
					Else
						If CRange.RangeHeight(subrange) >= 4 Then
							'Trace.WriteLine(subrange & vbTab & vbTab & "------>BIG HEADER AREA")
							BigHeaderRegions.Add(subrange)
							If BigHeaderRegions.Count = 1 Then
								dataStartColumn = CRange.RangeColumn(subrange)
							End If
							If BigHeaderRegions.Count = 2 Then
								summaryColumn = CRange.RangeColumn(subrange)
							End If
						Else
							'Trace.WriteLine(subrange & vbTab & vbTab & "------>COURSES SUBTABLE AREA")
							CoursesRegions.Add(subrange)
							If CoursesRegions.Count = 1 Then
								dataColumn = CRange.RangeColumn(subrange)
							End If
						End If
					End If
				Case 2
					If CRange.RangeColumn(subrange) = dataStartColumn Then 'Range at beginning of row
						'Trace.WriteLine(subrange & vbTab & vbTab & "------>  STUDENT DATA")
						If CRange.RangeWidth(subrange) < 5 Then
							StudentRegions.Add(subrange)
						Else
							'Very bad and annoying
							Trace.WriteLine("I HATE WHO MADE THOSE RESULTS")
							Trace.TraceWarning("Non-Student Data region starting at student area")
                            Trace.WriteLine("MOVING to grades or marks")
                            Beep()
                            Continue For
							If MarksOrGrades Then
								'Trace.WriteLine(subrange & vbTab & vbTab & "------>  MARKS DATA")
								MarksRegions.Add(subrange)
							Else
								'Trace.WriteLine(subrange & vbTab & vbTab & "------>  GRADES DATA")
								GradeRegions.Add(subrange)
							End If
							MarksOrGrades = Not MarksOrGrades
						End If
					ElseIf CRange.RangeColumn(subrange) = summaryColumn Then
						'Trace.WriteLine(subrange & vbTab & vbTab & "------>  SUMMARY DATA")
						SummaryRegions.Add(subrange)
					ElseIf CRange.RangeColumn(subrange) = dataColumn Then
						If MarksOrGrades Then
							'Trace.WriteLine(subrange & vbTab & vbTab & "------>  MARKS DATA")
							MarksRegions.Add(subrange)
						Else
							'Trace.WriteLine(subrange & vbTab & vbTab & "------>  GRADES DATA")
							GradeRegions.Add(subrange)
						End If
                        MarksOrGrades = Not MarksOrGrades
                    Else
                        If dataColumn > 4 AndAlso (CRange.RangeColumn(subrange) - dataColumn) <= 2 AndAlso CRange.RangeWidth(subrange) > 4 Then
                            Beep()
                            subrange = CRange.MutateRange(subrange, dataColumn - CRange.RangeColumn(subrange), 0, 0, 0)
                            If MarksOrGrades Then
                                'Trace.WriteLine(subrange & vbTab & vbTab & "------>  MARKS DATA")
                                MarksRegions.Add(subrange)
                            Else
                                'Trace.WriteLine(subrange & vbTab & vbTab & "------>  GRADES DATA")
                                GradeRegions.Add(subrange)
                            End If
                            MarksOrGrades = Not MarksOrGrades
                        End If
                    End If
                Case Else
                    Exit For
            End Select
		Next
	End Sub

	Public Sub Debug_ShowRegions(ByVal FileName As String, ByVal SheetName As String)
		Dim xApp As Excel.Application = Nothing
		Try
			xApp = New Excel.Application()

			Dim xWB As Excel.Workbook = xApp.Workbooks.Open(FileName)
			Dim xWs As Excel.Worksheet = xWB.Worksheets(SheetName)
			xWs.Activate()
			xApp.Visible = True
			Dim ff As Boolean = True
			For Each SubRegion In {TitleRegions, BigHeaderRegions, CoursesRegions, StudentRegions, MarksRegions, GradeRegions, SummaryRegions}
				MsgBox("NEW SET")
				For Each rn In SubRegion
					'If Not (rn.StartsWith(FastForwardTill)) And ff Then
					'	Continue For
					'Else
					'	ff = False
					'End If
					xWs.Range(rn).Select()
					Threading.Thread.Sleep(500)
				Next
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

	Public Sub New(ByRef ws As CWorksheet, ByRef SAna As CWorksheetStructureAnalyzer)
		Trace.Indent()
		Me.SheetName = ws.GetWorksheetName()
		ClassifySegmentedIslands(ws, SAna)
		Trace.Unindent()
	End Sub
End Class