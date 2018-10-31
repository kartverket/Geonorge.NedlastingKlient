
using System;
using System.Windows;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class SettingsDialog
    {
        public static readonly DependencyProperty DownloadDirectoryPathProperty = DependencyProperty.Register("DownloadDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());


        public static readonly DependencyProperty LogDirectoryPathProperty = DependencyProperty.Register("LogDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());

        public string DownloadDirectory { get { return (string)GetValue(DownloadDirectoryPathProperty) ?? string.Empty; } set { SetValue(DownloadDirectoryPathProperty, value); } }
        public string LogDirectory { get { return (string)GetValue(LogDirectoryPathProperty) ?? string.Empty; } set { SetValue(LogDirectoryPathProperty, value); } }


        public SettingsDialog()
        {
            AppSettings appSettings = ApplicationService.GetAppSettings();
            DownloadDirectory = appSettings.DownloadDirectory;
            LogDirectory = appSettings.LogDirectory;

            InitializeComponent();

            txtUsername.Text = appSettings.Username;
            txtPassword.Password = ProtectionService.GetUnprotectedPassword(appSettings.Password);
            GetDownloadUsage(appSettings);
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
            AppSettings appSettings = ApplicationService.GetAppSettings();
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
            GetDownloadUsage(ApplicationService.GetAppSettings());
        }
    }

}
