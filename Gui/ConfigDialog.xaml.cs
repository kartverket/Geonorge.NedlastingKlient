
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class ConfigDialog
    {
        private AppSettings _appSettings;
        public static readonly DependencyProperty DownloadDirectoryPathProperty = DependencyProperty.Register("DownloadDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());


        public static readonly DependencyProperty LogDirectoryPathProperty = DependencyProperty.Register("LogDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());

        public string DownloadDirectory { get { return (string)GetValue(DownloadDirectoryPathProperty) ?? string.Empty; } set { SetValue(DownloadDirectoryPathProperty, value); } }
        public string LogDirectory { get { return (string)GetValue(LogDirectoryPathProperty) ?? string.Empty; } set { SetValue(LogDirectoryPathProperty, value); } }


        public ConfigDialog()
        {
            _appSettings = ApplicationService.GetAppSettings();
            DownloadDirectory = _appSettings.DownloadDirectory;
            LogDirectory = _appSettings.LogDirectory;
            InitializeComponent();

            ConfigFilesList.ItemsSource = ApplicationService.NameConfigFiles();
            ConfigFilesList.SelectedItem = _appSettings.LastOpendConfigFile.Name;
            ConfigNameTextBox.Text = _appSettings.LastOpendConfigFile.Name;

            GetDownloadUsage(_appSettings);
        }

        private void GetDownloadUsage(AppSettings appSettings)
        {
            if (appSettings.DownloadUsage != null)
            {
                ShowDownloadUsage(appSettings);
            }
            else
            {
                HideDownloadUsage();

                var downloadUsageDialog = new DownloadUsageDialog();
                downloadUsageDialog.ShowDialog();
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            //if (NewConfigFileIsValid())
            //{
            //    _appSettings.ConfigFiles.Add(NewConfigFile());
            //    _appSettings.DownloadDirectory = FolderPickerDialogBox.DirectoryPath;
            //    _appSettings.LogDirectory = FolderPickerDialogBoxLog.DirectoryPath;
            //}
            //else
            //{
            //    return;
            //}

            //ApplicationService.WriteToAppSettingsFile(_appSettings);

            this.Close();
        }

        private ConfigFile NewConfigFile()
        {
            return new ConfigFile()
            {
                DownloadDirectory = FolderPickerDialogBox.DirectoryPath,
                LogDirectory = FolderPickerDialogBoxLog.DirectoryPath,
                Name = ConfigNameTextBox.Text,
                FilePath = ApplicationService.GetDownloadFilePath(ConfigNameTextBox.Text)
            };
        }

        private bool NewConfigFileIsValid()
        {
            bool valid = true;

            if (!NameIsValid())
            {
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(FolderPickerDialogBox.DirectoryPath))
            {
                // Feilmelding
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(FolderPickerDialogBoxLog.DirectoryPath))
            {
                // Feilmelding
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(ConfigNameTextBox.Text))
            {
                // Feilmelding
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(ConfigNameTextBox.Text))
            {
                // Feilmelding
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(DownloadUsageGroup.Text))
            {
                // Feilmelding
                valid = false;
            }

            return valid;

        }

        private void BtnNewConfigFile_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationService.WriteToAppSettingsFile(_appSettings);

            DownloadDirectory = string.Empty;
            LogDirectory = string.Empty;
            FolderPickerDialogBox.DirectoryPath = null;
            FolderPickerDialogBoxLog.DirectoryPath = null;
            InitializeComponent();

            HideDownloadUsage();
        }

        private void BtnEditDownloadUsage_OnClick(object sender, RoutedEventArgs e)
        {
            var downloadUsageDialog = new DownloadUsageDialog();
            downloadUsageDialog.ShowDialog();
            GetDownloadUsage(ApplicationService.GetAppSettings());
        }

        private void BtnDialogSave_Click(object sender, RoutedEventArgs e)
        {
            if (NewConfigFileIsValid())
            {
                var configFile = NewConfigFile();
                _appSettings.ConfigFiles.Add(configFile);
                _appSettings.DownloadDirectory = FolderPickerDialogBox.DirectoryPath;
                _appSettings.LogDirectory = FolderPickerDialogBoxLog.DirectoryPath;
                _appSettings.LastOpendConfigFile = configFile;
            }
            else
            {
                return;
            }

            ApplicationService.WriteToAppSettingsFile(_appSettings);
            ConfigFilesList.ItemsSource = ApplicationService.NameConfigFiles();
            ConfigFilesList.SelectedItem = _appSettings.LastOpendConfigFile.Name;
        }


        private void ShowDownloadUsage(AppSettings appSettings)
        {
            DownloadUsageGroupLayout.Visibility = Visibility.Visible;
            DownloadUsagePurposeLayout.Visibility = Visibility.Visible;
            DownloadUsagePurpose.ItemsSource = appSettings.DownloadUsage.Purpose;
            DownloadUsageGroup.Text = appSettings.DownloadUsage.Group;
        }

        private void HideDownloadUsage()
        {
            ConfigNameTextBox.Text = "";
            DownloadUsageGroupLayout.Visibility = Visibility.Hidden;
            DownloadUsageGroup.Text = "";
            DownloadUsagePurposeLayout.Visibility = Visibility.Hidden;
            DownloadUsagePurpose.ItemsSource = null;
        }

        private bool NameIsValid()
        {
            foreach (var config in _appSettings.ConfigFiles)
            {
                if (config.Name == ConfigNameTextBox.Text)
                {
                    return false;
                }
            }
            return true;
        }

    }

}
