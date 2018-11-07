
using System;
using System.Windows;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class SettingsDialog
    {
        private AppSettings _appSettings;
        public static readonly DependencyProperty DownloadDirectoryPathProperty = DependencyProperty.Register("DownloadDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());


        public static readonly DependencyProperty LogDirectoryPathProperty = DependencyProperty.Register("LogDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());

        public string DownloadDirectory { get { return (string)GetValue(DownloadDirectoryPathProperty) ?? string.Empty; } set { SetValue(DownloadDirectoryPathProperty, value); } }
        public string LogDirectory { get { return (string)GetValue(LogDirectoryPathProperty) ?? string.Empty; } set { SetValue(LogDirectoryPathProperty, value); } }


        public SettingsDialog()
        {
            _appSettings = ApplicationService.GetAppSettings();
            DownloadDirectory = _appSettings.DownloadDirectory;
            LogDirectory = _appSettings.LogDirectory;

            InitializeComponent();

            txtUsername.Text = _appSettings.Username;
            txtPassword.Password = ProtectionService.GetUnprotectedPassword(_appSettings.Password);
            GetDownloadUsage(_appSettings);
        }

        private void GetDownloadUsage(AppSettings appSettings)
        {
            if (appSettings.DownloadUsage != null)
            {
                DownloadUsagePurpose.ItemsSource = appSettings.DownloadUsage.Purpose;
                DownloadUsageGroup.Text = appSettings.DownloadUsage.Group;
            }
            else
            {
                DownloadUsagePurpose.ItemsSource = "Ikke satt";
                DownloadUsageGroup.Text = "Ikke satt";
                var downloadUsageDialog = new DownloadUsageDialog();
                downloadUsageDialog.ShowDialog();
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AppSettings appSettings = _appSettings;
            appSettings.Password = ProtectionService.CreateProtectedPassword(txtPassword.Password);
            appSettings.Username = txtUsername.Text;
            appSettings.DownloadDirectory = FolderPickerDialogBox.DirectoryPath;
            appSettings.LogDirectory = FolderPickerDialogBoxLog.DirectoryPath;
            ApplicationService.WriteToAppSettingsFile(appSettings);

            this.Close();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var downloadUsageDialog = new DownloadUsageDialog();
            downloadUsageDialog.ShowDialog();
            GetDownloadUsage(_appSettings);
        }
    }

}
