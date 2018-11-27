using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Serilog;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private AppSettings _appSettings;
        private DatasetService _datasetService;
        private List<Projections> _projections;
        private Dataset _selectedDataset;
        private List<DatasetFileViewModel> _selectedDatasetFiles;
        private List<DownloadViewModel> _selectedFilesForDownload;
        private ConfigFile _selectedConfigFile;
        private bool _versionStatusMessage = false;
        public bool LoggedIn;


        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("log-.txt",
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            Log.Information("Start application");
            InitializeComponent();

            BtnSelectAll.Visibility = Visibility.Hidden;
            BtnSelectAll.IsChecked = false;
            ToggleSubscribeSelectedDatasetFiles.Visibility = Visibility.Hidden;
            MenuSubscribe.Visibility = Visibility.Hidden;

            MassivNedlastingVersion massivNedlastingVersjon = new MassivNedlastingVersion(new GitHubReleaseInfoReader());
            string currentVersion = MassivNedlastingVersion.Current;
            bool VersionStatusMessage = massivNedlastingVersjon.UpdateIsAvailable();

            _appSettings = ApplicationService.GetAppSettings();
            _datasetService = new DatasetService(_appSettings.LastOpendConfigFile);
            _selectedConfigFile = _appSettings.LastOpendConfigFile;

            _datasetService.ConvertDownloadToDefaultConfigFileIfExists();


            try
            {
                LbDatasets.ItemsSource = _datasetService.GetDatasets();
            }
            catch (Exception)
            {
                MessageBox.Show("Klarer ikke hente datasett... Sjekk internett tilkoblingen din");
            }

            try
            {
                _projections = _datasetService.FetchProjections();
            }
            catch (Exception e)
            {
                _projections = _datasetService.ReadFromProjectionFile();
            }

            _selectedFilesForDownload = _datasetService.GetSelectedFilesToDownloadAsViewModel(_projections);
            _selectedDatasetFiles = new List<DatasetFileViewModel>();

            var viewDatasets = (CollectionView)CollectionViewSource.GetDefaultView(LbDatasets.ItemsSource);
            if (viewDatasets != null) viewDatasets.Filter = UserDatasetFilter;

            LbSelectedFilesForDownload.ItemsSource = _selectedFilesForDownload;

            cmbConfigFiles.ItemsSource = ApplicationService.NameConfigFiles();
            cmbConfigFiles.SelectedItem = _selectedConfigFile.Name;

            SetDownloadUsage();
        }

        private void SetDownloadUsage()
        {
            if (!_selectedConfigFile.DownloadUsageIsSet())
            {
                Log.Information("Set download usage");
                var downloadUsageDialog = new DownloadUsageDialog();
                downloadUsageDialog.ShowDialog();
            }
        }

        private bool UserDatasetFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchDataset.Text))
                return true;
            return (item as Dataset).Title.IndexOf(SearchDataset.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   (item as Dataset).Organization.IndexOf(SearchDataset.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool UserDatasetFileFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchDatasetFiles.Text))
                return true;
            return (item as DatasetFileViewModel).Title.IndexOf(SearchDatasetFiles.Text,
                       StringComparison.OrdinalIgnoreCase) >= 0 ||
                   (item as DatasetFileViewModel).Category.IndexOf(SearchDatasetFiles.Text,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private async void ShowFiles(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBoxItem)
            {
                if (listBoxItem.SelectedItems.Count > 0)
                {
                    var selectedDataset = (Dataset)listBoxItem.SelectedItems[0];
                    if (selectedDataset != null)
                    {
                        _selectedDataset = selectedDataset;
                        progressBar.IsIndeterminate = true;

                        LbSelectedDatasetFiles.ItemsSource = await Task.Run(() => GetFilesAsync(selectedDataset));
                        var viewDatasetFiles =
                            (CollectionView)CollectionViewSource.GetDefaultView(LbSelectedDatasetFiles.ItemsSource);
                        if (viewDatasetFiles != null) viewDatasetFiles.Filter = UserDatasetFileFilter;

                        SubscribeOnSelectedDataset(selectedDataset.Title);
                        progressBar.IsIndeterminate = false;

                    }
                }

                BtnSelectAll.Visibility = Visibility.Visible;
                ToggleSubscribeSelectedDatasetFiles.Visibility = Visibility.Visible;

                BtnSelectAll.IsChecked = false;
            }
        }

        private void SubscribeOnSelectedDataset(string selectedDatasetTitle)
        {
            var subscribe = false;
            var autoAddFiles = false;
            var autoDeleteFiles = false;

            foreach (var download in _selectedFilesForDownload)
            {
                if (download.DatasetTitle == selectedDatasetTitle)
                {
                    subscribe = download.Subscribe;
                    autoAddFiles = download.AutoAddFiles;
                    autoDeleteFiles = download.AutoDeleteFiles;
                }
            }

            ToggleSubscribeSelectedDatasetFiles.IsChecked = subscribe;
            BtnAutoDeleteFiles.IsChecked = autoDeleteFiles;
            BtnAutoAddFiles.IsChecked = autoAddFiles;
            MenuSubscribe.Visibility = subscribe ? Visibility.Visible : Visibility.Hidden;
        }

        private List<DatasetFileViewModel> GetFilesAsync(Dataset selctedDataset)
        {
            var selectedDatasetFiles = _datasetService.GetDatasetFiles(selctedDataset, _projections);

            foreach (var dataset in _selectedFilesForDownload)
            {
                foreach (var selectedDownloadFile in dataset.Files)
                {
                    foreach (var datasetFile in selectedDatasetFiles)
                        if (selectedDownloadFile.Id == datasetFile.Id)
                        {
                            datasetFile.SelectedForDownload = true;
                            break;
                        }
                }
            }

            if (selectedDatasetFiles.Count == 0) MessageBox.Show("Ingen filer for dette datasettet");
            _selectedDatasetFiles = selectedDatasetFiles;
            return selectedDatasetFiles;
        }

        private void AddRemove_OnChecked(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var datasetFile = (DatasetFileViewModel)btn.DataContext;

            if (btn.IsChecked == true)
            {
                datasetFile.SelectedForDownload = true;
                AddToList(datasetFile);
            }
            else
            {
                RemoveFromList(datasetFile);
                datasetFile.SelectedForDownload = false;
            }
        }

        private void AddToList(DatasetFileViewModel selectedFile)
        {
            if (selectedFile != null)
            {
                var datasetExists = false;
                foreach (var dataset in _selectedFilesForDownload)
                {
                    if ((dataset.DatasetId == selectedFile.DatasetId) || (dataset.DatasetTitle == selectedFile.DatasetId))
                    {
                        dataset.Files.Add(selectedFile);
                        datasetExists = true;
                        break;
                    }
                }

                if (!datasetExists)
                {
                    var downloadViewModel = new DownloadViewModel(_selectedDataset, selectedFile);
                    _selectedFilesForDownload.Add(downloadViewModel);
                }

                BindNewList();
            }
            else
            {
                MessageBox.Show("Kunne ikke legge til fil...");
            }
        }

        private void RemoveFromList(DatasetFileViewModel selectedFile)
        {
            if (selectedFile != null)
            {
                foreach (var dataset in _selectedFilesForDownload)
                {
                    dataset.Files.RemoveAll(f => f.Id == selectedFile.Id);
                }
                BindNewList();
            }
            else
            {
                MessageBox.Show("Kunne ikke fjerne fil...");
            }
        }

        private void BindNewList()
        {
            LbSelectedFilesForDownload.ItemsSource = null;
            LbSelectedFilesForDownload.ItemsSource = _selectedFilesForDownload;
            LbSelectedDatasetFiles.ItemsSource = null;
            LbSelectedDatasetFiles.ItemsSource = _selectedDatasetFiles;
        }

        private void RemoveFromDownloadList_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var selectedDatasetFile = (DatasetFileViewModel)btn.DataContext;
            UpdateSelectedDatasetFiles(selectedDatasetFile);

            foreach (var download in _selectedFilesForDownload)
            {
                if (download.DatasetTitle == selectedDatasetFile.DatasetId)
                {
                    download.Files.Remove(selectedDatasetFile);
                    if (!download.Files.Any())
                    {
                        _selectedFilesForDownload.Remove(download);
                    }
                    break;
                }
            }

            BindNewList();
        }


        private void UpdateSelectedDatasetFiles(DatasetFileViewModel selectedDatasetFile)
        {
            if (_selectedDatasetFiles.Any())
                foreach (var datasetFile in _selectedDatasetFiles)
                    if (datasetFile.Id == selectedDatasetFile.Id)
                    {
                        datasetFile.SelectedForDownload = false;
                        break;
                    }
        }

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            SaveDownloadList();
            Log.Information("Close application");

        }

        private void SaveDownloadList()
        {
            _datasetService.WriteToConfigFile(_selectedFilesForDownload);
        }

        private void BtnSelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (LbSelectedDatasetFiles.Items.IsEmpty) return;
            if (BtnSelectAll.IsChecked == true)
            {
                foreach (DatasetFileViewModel datasetFile in LbSelectedDatasetFiles.Items)
                    if (!datasetFile.SelectedForDownload)
                    {
                        datasetFile.SelectedForDownload = true;
                        AddToList(datasetFile);
                    }
            }
            else
            {
                foreach (var datasetFile in _selectedDatasetFiles)
                    if (datasetFile.SelectedForDownload)
                    {
                        datasetFile.SelectedForDownload = false;
                        RemoveFromList(datasetFile);
                    }
            }

            LbSelectedDatasetFiles.ItemsSource = null;
            LbSelectedDatasetFiles.ItemsSource = _selectedDatasetFiles;
        }

        private void BtnRemoveAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedFilesForDownload.Any())
            {
                var result = MessageBox.Show("Er du sikker på at du vil slette alle", "Slett alle",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    _selectedFilesForDownload = new List<DownloadViewModel>();
                    foreach (var datasetfile in _selectedDatasetFiles) datasetfile.SelectedForDownload = false;
                    BindNewList();
                }

                BtnSelectAll.IsChecked = false;
            }
        }

        private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            SaveDownloadList();

            var executingAssemblyDirectory = GetExecutingAssemblyDirectory();

            var pathToConsoleApp =
                Path.Combine(executingAssemblyDirectory, "..", "Nedlaster", "Geonorge.Nedlaster.exe");
            try
            {
                Log.Information("Start downloader");
                Process.Start(pathToConsoleApp, _appSettings.LastOpendConfigFile.Name);
            }
            catch (Exception er)
            {
                Log.Error(er, "Could not start downloader");
                MessageBox.Show("Finner ikke nedlaster...");
            }
        }

        private static string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }


        private void SearchDataset_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(LbDatasets.ItemsSource).Refresh();
        }

        private void SearchDatasetFiles_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (LbSelectedDatasetFiles != null && LbSelectedDatasetFiles.ItemsSource != null)
                CollectionViewSource.GetDefaultView(LbSelectedDatasetFiles.ItemsSource).Refresh();
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var loginDialog = new SettingsDialog();
            loginDialog.ShowDialog();
            _appSettings = ApplicationService.GetAppSettings();
            _selectedConfigFile = _appSettings.LastOpendConfigFile;
            cmbConfigFiles.ItemsSource = _appSettings.NameConfigFiles();
            cmbConfigFiles.SelectedItem = _selectedConfigFile.Name;
        }

        private void BtnHelp_OnClick(object sender, RoutedEventArgs e)
        {
            var helpDialog = new HelpDialog();
            helpDialog.ShowDialog();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveDownloadList();
            Log.Information("Save download list");
        }

        private void BtnSubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (btn == null) return;

            var subscribe = btn.IsChecked.Value;

            var existsInList = false;

            foreach (var download in _selectedFilesForDownload)
            {
                if (download.DatasetTitle == _selectedDataset.Title)
                {
                    existsInList = true;
                    download.Subscribe = subscribe;
                    download.AutoAddFiles = subscribe;
                    download.AutoDeleteFiles = subscribe;
                    if (!download.Files.Any())
                    {
                        _selectedFilesForDownload.Remove(download);
                    }
                    break;
                }
            }

            if (!existsInList && subscribe)
            {
                var download = new DownloadViewModel(_selectedDataset, subscribe);
                _selectedFilesForDownload.Add(download);
            }

            MenuSubscribe.Visibility = subscribe ? Visibility.Visible : Visibility.Hidden;
            BtnAutoDeleteFiles.IsChecked = subscribe;
            BtnAutoAddFiles.IsChecked = subscribe;

            BindNewList();
        }

        private void BtnAutoDeleteFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var cbDelete = (CheckBox)sender;
            if (cbDelete == null)
            {
                return;
            }

            foreach (var download in _selectedFilesForDownload)
            {
                if (download.DatasetTitle == _selectedDataset.Title)
                {
                    if (cbDelete.IsChecked != null) download.AutoDeleteFiles = cbDelete.IsChecked.Value;
                }
            }
        }

        private void BtnAutoAddFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var cbAdd = (CheckBox)sender;
            if (cbAdd == null)
            {
                return;
            }

            foreach (var download in _selectedFilesForDownload)
            {
                if (download.DatasetTitle == _selectedDataset.Title)
                {
                    if (cbAdd.IsChecked != null) download.AutoAddFiles = cbAdd.IsChecked.Value;
                }
            }
        }

        
        private void CmbConfigFiles_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbConfig = (ComboBox) sender;
            if (cmbConfig != null)
            {
                SaveDownloadList();
                _datasetService = new DatasetService(_appSettings.GetConfigByName(cmbConfig.SelectedItem.ToString()));
                _appSettings.LastOpendConfigFile = _appSettings.GetConfigByName(cmbConfig.SelectedItem.ToString());
                ApplicationService.WriteToAppSettingsFile(_appSettings);
                _selectedFilesForDownload = _datasetService.GetSelectedFilesToDownloadAsViewModel(_projections);
                LbSelectedFilesForDownload.ItemsSource = _selectedFilesForDownload;
            }

        }
    }
}