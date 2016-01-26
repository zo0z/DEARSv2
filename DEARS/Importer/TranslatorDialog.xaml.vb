Imports ObservableDictionary
Public Class TranslatorDialog
    Private Sub Window_ContentRendered(sender As Object, e As EventArgs)
        If SharedState.DBContext Is Nothing Then
            SharedState.DBContext = New AcademicResultsDBEntities()
        End If
        DirectCast(Me.FindResource("RecommendationTypesViewSource"), CollectionViewSource).Source = _
            SharedState.DBContext.RecommendationTypes.ToList()

        DirectCast(Me.FindResource("RecommendationsViewSource"), CollectionViewSource).Source = SharedState.DBContext.RecommTranslations.Local
        'ObservableEntityCollection(Of RecommTranslation)(SharedState.DBContext, SharedState.DBContext.RecommTranslations.Local())
        

        'If SharedState.DBContext.RecommTranslations.Local.Count = 0 Then

        '    Dim x = getDictionary()
        '    For Each y In x
        '        Dim z As New RecommTranslation() With {.ResText = y.Key, _
        '                                               .RecommendationType = (From rt In SharedState.DBContext.RecommendationTypes.Local Where rt.ShortNameEnglish = y.Value).Single()}
        '        SharedState.DBContext.RecommTranslations.Add(z)
        '    Next
        '    SharedState.DBContext.SaveChanges()
        'End If
    End Sub

    Function getDictionary() As SortedDictionary(Of String, String)
        Dim Lookup As New SortedDictionary(Of String, String)()
        Dim str As New IO.StreamReader("C:\Users\Mohanad\Desktop\DEEE\DEARS\TranslateRecommsDB.txt")
        While Not str.EndOfStream()
            Dim txt = str.ReadLine()
            Dim dat() As String = txt.Split(",")
            'If dat(0) = "فصل" Then
            '    Beep()
            'End If
            If String.IsNullOrWhiteSpace(dat(1).Trim()) Then
                Debug.Assert(False)
            End If
            Lookup(dat(0).Trim()) = dat(1).Trim()
        End While
        str.Close()
        Return Lookup
    End Function

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If SharedState.DBContext.RecommTranslations.Local.Any(Function(s) s.RecommendationTypeN Is Nothing) Then
            MsgBox("There are unfilled items please fill them and then close this window")
            e.Cancel = True
        End If
    End Sub
End Class