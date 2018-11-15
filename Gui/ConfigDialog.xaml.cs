
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class ConfigDialog
    {
        private AppSettings _appSettings;
        private bool _editConfig = true;
        private bool _newConfig = false;
        private ConfigFile _selectedConfigFile;

        public static readonly DependencyProperty DownloadDirectoryPathProperty = DependencyProperty.Register("DownloadDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty LogDirectoryPathProperty = DependencyProperty.Register("LogDirectoryPath", typeof(string), typeof(SettingsDialog),
            new FrameworkPropertyMetadata());

        public string DownloadDirectory { get { return (string)GetValue(DownloadDirectoryPathProperty) ?? string.Empty; } set { SetValue(DownloadDirectoryPathProperty, value); } }
        public string LogDirectory { get { return (string)GetValue(LogDirectoryPathProperty) ?? string.Empty; } set { SetValue(LogDirectoryPathProperty, value); } }


        public ConfigDialog()
        {
            _appSettings = ApplicationService.GetAppSettings();
            if (_appSettings.TempConfigFile != null)
            {
                _appSettings.TempConfigFile = null;
                ApplicationService.WriteToAppSettingsFile(_appSettings);
            }
            _selectedConfigFile = _appSettings.LastOpendConfigFile;
            _appSettings.TempConfigFile = null;

            DownloadDirectory = _selectedConfigFile.DownloadDirectory;
            LogDirectory = _selectedConfigFile.LogDirectory;

            InitializeComponent();

            ConfigFilesList.ItemsSource = ApplicationService.NameConfigFiles();
            ConfigFilesList.SelectedItem = _selectedConfigFile.Name;
            ConfigNameTextBox.Text = _selectedConfigFile.Name;

            GetDownloadUsage();
        }

        private void GetDownloadUsage()
        {
            if (_selectedConfigFile.DownloadUsage == null)
            {
                var downloadUsageDialog = new DownloadUsageDialog();
                downloadUsageDialog.ShowDialog();
                _appSettings = ApplicationService.GetAppSettings();
                _selectedConfigFile = _appSettings.LastOpendConfigFile;
            }
            ShowDownloadUsage();
        }

        private void ConfigFilesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbConfig = (ComboBox)sender;
            if (cmbConfig != null && !string.IsNullOrWhiteSpace(cmbConfig.Text) && cmbConfig.SelectedItem != null)
            {
                _appSettings.LastOpendConfigFile = _appSettings.GetConfigByName(cmbConfig.SelectedItem.ToString());
                ShowSelectedConfigFile();
            }
        }


        private void BtnNewConfigFile_OnClick(object sender, RoutedEventArgs e)
        {
            _appSettings.TempConfigFile = new ConfigFile();
            ApplicationService.WriteToAppSettingsFile(_appSettings);
            StatusNewConfigFile();

            _selectedConfigFile = new ConfigFile();
            DownloadDirectory = string.Empty;
            LogDirectory = string.Empty;
            FolderPickerDialogBox.DirectoryPath = null;
            FolderPickerDialogBoxLog.DirectoryPath = null;

            HideDownloadUsage();
        }

        private void BtnEditDownloadUsage_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationService.WriteToAppSettingsFile(_appSettings);
            var downloadUsageDialog = new DownloadUsageDialog();
            downloadUsageDialog.ShowDialog();
            _appSettings = ApplicationService.GetAppSettings();
            if (_newConfig && _appSettings.TempConfigFile != null)
            {
                _selectedConfigFile.DownloadUsage = _appSettings.TempConfigFile.DownloadUsage;
                //_appSettings.TempConfigFile = null;
            }
            else if (_editConfig)
            {
                _selectedConfigFile = _appSettings.LastOpendConfigFile;
            }
            GetDownloadUsage();
        }

        private void BtnDialogDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_appSettings.ConfigFiles.Count > 1)
            {
                foreach (var configFile in _appSettings.ConfigFiles)
                {
                    if (_selectedConfigFile.Id == configFile.Id)
                    {
                        var remove = _appSettings.ConfigFiles.Remove(_selectedConfigFile);
                        break;
                    }
                }
                _appSettings.LastOpendConfigFile = _appSettings.ConfigFiles.FirstOrDefault();
                ApplicationService.WriteToAppSettingsFile(_appSettings);
                ShowSelectedConfigFile();
            }
            StatusEditConfigFile();
        }

        private void BtnDialogSave_Click(object sender, RoutedEventArgs e)
        {
            if (NewConfigFileIsValid())
            {
                if (_editConfig)
                {
                    foreach (var config in _appSettings.ConfigFiles)
                    {
                        if (config.Id == _selectedConfigFile.Id)
                        {
                            config.Name = ConfigNameTextBox.Text;
                            config.FilePath = ApplicationService.GetDownloadFilePath(config.Name);
                            config.DownloadDirectory = _selectedConfigFile.DownloadDirectory;
                            config.LogDirectory = _selectedConfigFile.LogDirectory;
                            _appSettings.LastOpendConfigFile = config;
                            break;
                        }
                    }
                }
                else
                {
                    var configFile = NewConfigFile();
                    _appSettings.ConfigFiles.Add(configFile);
                    _appSettings.LastOpendConfigFile = configFile;
                    _selectedConfigFile = configFile;
                    _appSettings.TempConfigFile = null;
                }
            }
            else
            {
                return;
            }

            _appSettings.TempConfigFile = null;
            ApplicationService.WriteToAppSettingsFile(_appSettings);
            ShowSelectedConfigFile();
            StatusEditConfigFile();
        }

        private void StatusEditConfigFile()
        {
            _editConfig = true;
            _newConfig = false;
        }

        private void StatusNewConfigFile()
        {
            _editConfig = false;
            _newConfig = true;
        }

        private ConfigFile NewConfigFile()
        {
            return new ConfigFile()
            {
                DownloadDirectory = FolderPickerDialogBox.DirectoryPath,
                LogDirectory = FolderPickerDialogBoxLog.DirectoryPath,
                Name = ConfigNameTextBox.Text,
                FilePath = ApplicationService.GetDownloadFilePath(ConfigNameTextBox.Text),
                DownloadUsage = _selectedConfigFile.DownloadUsage
            };
        }


        private void ShowDownloadUsage()
        {
            DownloadUsageGroupLayout.Visibility = Visibility.Visible;
            DownloadUsagePurposeLayout.Visibility = Visibility.Visible;
            DownloadUsagePurpose.ItemsSource = _selectedConfigFile.DownloadUsage.Purpose;
            DownloadUsageGroup.Text = _selectedConfigFile.DownloadUsage.Group;
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
                if (config.Name == ConfigNameTextBox.Text && config.Id != _selectedConfigFile.Id)
                {
                    return false;
                }
            }
            return true;
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

        private void ShowSelectedConfigFile()
        {
            _selectedConfigFile = _appSettings.LastOpendConfigFile;
            ConfigFilesList.ItemsSource = ApplicationService.NameConfigFiles();
            FolderPickerDialogBoxLog.DirectoryPath = _selectedConfigFile.LogDirectory;
            FolderPickerDialogBox.DirectoryPath = _selectedConfigFile.DownloadDirectory;
            if (ConfigFilesList.SelectedItem != _selectedConfigFile.Name)
            {
                ConfigFilesList.SelectedItem = _selectedConfigFile.Name;
            }
            ConfigNameTextBox.Text = _selectedConfigFile.Name;
            GetDownloadUsage();
        }
    }
}
