﻿using System;
using System.Windows;

namespace NedlastingKlient.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class SettingsDialog
    {
        public string downloadDirectory;

        public SettingsDialog()
        {
            InitializeComponent();

            AppSettings appSettings = ApplicationService.GetAppSettings();

            txtUsername.Text = appSettings.Username;
            txtPassword.Password = appSettings.Password;
            downloadDirectory = appSettings.DownloadDirectory;
            //txtDownloadDirectory.Text = appSettings.DownloadDirectory;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AppSettings appSettings = new AppSettings();
            appSettings.Password = txtPassword.Password;
            appSettings.Username = txtUsername.Text;
            appSettings.DownloadDirectory = FolderPickerDialogBox.DirectoryPath;
            ApplicationService.WriteToAppSettingsFile(appSettings);

            this.Close();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtUsername.SelectAll();
            txtUsername.Focus();
        }

        public string Answer
        {
            get { return txtUsername.Text; }
        }

    }

}