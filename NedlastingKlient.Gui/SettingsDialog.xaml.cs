
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
        public SettingsDialog()
        {
            InitializeComponent();

            AppSettings appSettings = ApplicationService.GetAppSettings();

            txtUsername.Text = appSettings.Username;
            txtPassword.Password = ProtectionService.GetUnprotectedPassword(appSettings.Password);
            downloadDirectory = appSettings.DownloadDirectory;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AppSettings appSettings = new AppSettings();
            appSettings.Password = ProtectionService.CreateProtectedPassword(txtPassword.Password);
            appSettings.Username = txtUsername.Text;
            appSettings.DownloadDirectory = FolderPickerDialogBox.DirectoryPath;
            ApplicationService.WriteToAppSettingsFile(appSettings);

            this.Close();
        }

    }

}
