Imports System.Collections.ObjectModel
Imports System.Data.Entity
Imports System.Data.Entity.Core

Public Class SQLConnectWindow

    Dim cts As System.Threading.CancellationTokenSource
    Dim sqlAuthenticationBinding As Binding

    Dim sqlConn As EntityClient.EntityConnection

    Public ReadOnly Property SqlConnection As EntityClient.EntityConnection
        Get
            Return sqlConn
        End Get
    End Property
    Public Property AutoConnect As Boolean = True

    Dim ServersCollection As New ObservableCollection(Of String)()

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        For Each st In My.Settings.ServersCollection
            ServersCollection.Add(st)
        Next
        DirectCast(Me.FindResource("ServersViewSource"), CollectionViewSource).Source = Me.ServersCollection
        Me.ServerComboBox.SelectedIndex = 0
        Me.WindowsAuthenticationRadioButton.IsChecked = My.Settings.IsWindowsAuthen
        If Not My.Settings.IsWindowsAuthen Then
            Me.UsernameTextbox.Text = My.Settings.DBUserID
        End If
        Me.CancelButton.IsEnabled = False
        sqlAuthenticationBinding = New Binding()
        sqlAuthenticationBinding.Source = SqlServerAuthenticationRadioButton
        sqlAuthenticationBinding.Path = New PropertyPath("IsChecked")
        sqlAuthenticationBinding.Mode = BindingMode.TwoWay
        sqlAuthenticationBinding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
        If AutoConnect AndAlso My.Settings.IsWindowsAuthen And Not String.IsNullOrWhiteSpace(Me.ServerComboBox.Text) Then
            TestConnection(1)
        Else
            SetDialogEnabled(True)
        End If
    End Sub
    Sub SetDialogEnabled(State As Boolean)
        Me.ServerComboBox.IsEnabled = State
        Me.ConnectButton.IsEnabled = State
        Me.WindowsAuthenticationRadioButton.IsEnabled = State
        Me.SqlServerAuthenticationRadioButton.IsEnabled = State
        If State Then
            BindingOperations.SetBinding(UsernameTextbox, TextBox.IsEnabledProperty, sqlAuthenticationBinding)
            BindingOperations.SetBinding(PasswordTextbox, TextBox.IsEnabledProperty, sqlAuthenticationBinding)
        Else
            Me.UsernameTextbox.IsEnabled = State
            Me.PasswordTextbox.IsEnabled = State
        End If
        Me.CancelButton.IsEnabled = Not State
    End Sub
    Dim dbName As String = "AcademicResultsDB"
    Sub TestConnection(Timeout As Integer)
        SetDialogEnabled(False)

        Dim sqlBuild As New System.Data.SqlClient.SqlConnectionStringBuilder()
        sqlBuild("Server") = Me.ServerComboBox.Text
        sqlBuild.InitialCatalog = dbName
        sqlBuild.IntegratedSecurity = Not SqlServerAuthenticationRadioButton.IsChecked
        If Not sqlBuild.IntegratedSecurity Then
            sqlBuild.UserID = UsernameTextbox.Text
            sqlBuild.Password = PasswordTextbox.Password
            'sqlBuild.PersistSecurityInfo = True
        End If
        sqlBuild.ConnectTimeout = Timeout

        Dim entBuild As New EntityClient.EntityConnectionStringBuilder()
        entBuild.Provider = "System.Data.SqlClient"
        entBuild.Metadata = "res://*/DataAccess.AcademicResultsDB.csdl|res://*/DataAccess.AcademicResultsDB.ssdl|res://*/DataAccess.AcademicResultsDB.msl"
        entBuild.ProviderConnectionString = sqlBuild.ConnectionString

        System.Data.SqlClient.SqlConnection.ClearAllPools()


        If cts IsNot Nothing Then
            cts.Dispose()
        End If
        cts = New System.Threading.CancellationTokenSource()

        sqlConn = New EntityClient.EntityConnection(entBuild.ConnectionString)

        sqlConn.OpenAsync(cts.Token).ContinueWith(Sub(s)
                                                      If s.IsCanceled Then
                                                          Me.Dispatcher.Invoke(New Action(Of Boolean)(AddressOf SetDialogEnabled), True)
                                                          Exit Sub
                                                      End If
                                                      If s.Exception IsNot Nothing Then
                                                          For Each ex In s.Exception.InnerExceptions
                                                              MsgBox(Application.FlattenOutException(ex))
                                                          Next
                                                          Me.Dispatcher.Invoke(New Action(Of Boolean)(AddressOf SetDialogEnabled), True)
                                                      Else
                                                          Me.Dispatcher.Invoke(New Action(Of Integer)(
                                                                      Sub(q)
                                                                          Me.DialogResult = True
                                                                          Me.Close()
                                                                          If My.Settings.ServersCollection.Contains(Me.ServerComboBox.Text) Then
                                                                              My.Settings.ServersCollection.Remove(Me.ServerComboBox.Text)
                                                                          End If
                                                                          My.Settings.ServersCollection.Insert(0, ServerComboBox.Text)
                                                                          My.Settings.IsWindowsAuthen = WindowsAuthenticationRadioButton.IsChecked
                                                                          If Me.WindowsAuthenticationRadioButton.IsChecked Then
                                                                              My.Settings.DBUserID = Me.UsernameTextbox.Text
                                                                          End If
                                                                          My.Settings.Save()
                                                                          SharedState.DBContext = New AcademicResultsDBEntities(sqlConn, False)
                                                                      End Sub), 0)
                                                      End If
                                                  End Sub)
    End Sub

    Private Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs)
        TestConnection(5)
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs)
        cts.Cancel()
    End Sub

    Private Sub ServerComboBox_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyboardDevice.Modifiers = ModifierKeys.Shift Then
            If e.Key = Key.Delete Then
                'For Each server In ServerComboBox.Items
                '    Dim it As ComboBoxItem = ServerComboBox.ItemContainerGenerator.ContainerFromItem(server)
                '    If it.IsHighlighted Then
                '        My.Settings.ServersCollection.Remove(it.ToString)
                '    End If
                'Next
                ServersCollection.Remove(ServerComboBox.Text)
            End If
        End If
    End Sub


    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        My.Settings.ServersCollection.Clear()
        My.Settings.ServersCollection.AddRange(ServersCollection.ToArray())
        My.Settings.Save()
    End Sub

End Class
