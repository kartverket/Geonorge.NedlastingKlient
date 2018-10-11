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

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DatasetService _datasetService;
        private List<Projections> _projections;
        private Dataset _selectedDataset;
        private List<DatasetFileViewModel> _selectedDatasetFiles;
        //private List<DatasetFileViewModel> _selectedFiles;
        private List<DownloadViewModel> _selectedFilesForDownload;
        public bool LoggedIn;

        public MainWindow()
        {
            InitializeComponent();

            BtnSelectAll.Visibility = Visibility.Hidden;
            BtnSelectAll.IsChecked = false;

            _datasetService = new DatasetService();

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
            var viewDatasets = (CollectionView) CollectionViewSource.GetDefaultView(LbDatasets.ItemsSource);
            if (viewDatasets != null) viewDatasets.Filter = UserDatasetFilter;

            _selectedFilesForDownload = _datasetService.GetSelectedFilesToDownloadAsViewModel(_projections);
            LbSelectedFilesForDownload.ItemsSource = _selectedFilesForDownload;

            _selectedDatasetFiles = new List<DatasetFileViewModel>();
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
                    var selectedDataset = (Dataset) listBoxItem.SelectedItems[0];
                    if (selectedDataset != null)
                    {
                        _selectedDataset = selectedDataset;
                        progressBar.IsIndeterminate = true;

                        LbSelectedDatasetFiles.ItemsSource = await Task.Run(() => GetFilesAsync(selectedDataset));
                        progressBar.IsIndeterminate = false;
                        var viewDatasetFiles =
                            (CollectionView) CollectionViewSource.GetDefaultView(LbSelectedDatasetFiles.ItemsSource);
                        if (viewDatasetFiles != null) viewDatasetFiles.Filter = UserDatasetFileFilter;
                    }
                }

                BtnSelectAll.Visibility = Visibility.Visible;
                BtnSelectAll.IsChecked = false;
            }
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
            var btn = (ToggleButton) sender;
            var datasetFile = (DatasetFileViewModel) btn.DataContext;

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
                var downloadViewModel = new DownloadViewModel(_selectedDataset, selectedFile);
                _selectedFilesForDownload.Add(downloadViewModel);
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
                //_selectedFilesForDownload.RemoveAll(f => f.Id == selectedFile.Id);
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
            var btn = (Button) sender;
            var selectedDatasetFile = (DatasetFileViewModel) btn.DataContext;
            UpdateSelectedDatasetFiles(selectedDatasetFile);

            foreach (var dataset in _selectedFilesForDownload)
            {
                if (dataset.DatasetId == selectedDatasetFile.DatasetId)
                {
                    dataset.Files.Remove(selectedDatasetFile);
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
        }

        private void SaveDownloadList()
        {
            _datasetService.WriteToDownloadFile(_selectedFilesForDownload);
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
                Process.Start(pathToConsoleApp);
            }
            catch (Exception)
            {
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
        }

        private void BtnHelp_OnClick(object sender, RoutedEventArgs e)
        {
            var helpDialog = new HelpDialog();
            helpDialog.ShowDialog();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveDownloadList();
        }
    }
}