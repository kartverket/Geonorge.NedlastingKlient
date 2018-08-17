
using System;
using System.Windows;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class SettingsDialog
    {
        public string downloadDirectory;
        public string logDirectory;
        public SettingsDialog()
        {
            InitializeComponent();

            AppSettings appSettings = ApplicationService.GetAppSettings();

            txtUsername.Text = appSettings.Username;
            txtPassword.Password = ProtectionService.GetUnprotectedPassword(appSettings.Password);
            downloadDirectory = appSettings.DownloadDirectory;
            logDirectory = appSettings.LogDirectory;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AppSettings appSettings = new AppSettings();
            appSettings.Password = ProtectionService.CreateProtectedPassword(txtPassword.Password);
            appSettings.Username = txtUsername.Text;
            appSettings.DownloadDirectory = FolderPickerDialogBox.DirectoryPath;
            appSettings.LogDirectory = FolderPickerDialogBoxLog.DirectoryPath;
            ApplicationService.WriteToAppSettingsFile(appSettings);

            this.Close();
        }

    }

}
