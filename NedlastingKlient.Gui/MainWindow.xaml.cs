using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;

namespace NedlastingKlient.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //public ICommand ShowProgressDialogCommand { get; }

        private List<DatasetFileViewModel> _selectedFiles;
        private List<DatasetFileViewModel> _selectedDatasetFiles;
        private List<Dataset> _datasets;
        public bool LoggedIn;
        private DatasetService _datasetService;

        public MainWindow()
        {
            InitializeComponent();

            BtnSelectAll.Visibility = Visibility.Hidden;
            BtnSelectAll.IsChecked = false;

            _datasetService = new DatasetService();

            _datasets = _datasetService.GetDatasets();
            LbDatasets.ItemsSource = _datasets;
            CollectionView viewDatasets = (CollectionView) CollectionViewSource.GetDefaultView(LbDatasets.ItemsSource);
            if (viewDatasets != null) viewDatasets.Filter = UserDatasetFilter;

            _selectedFiles = _datasetService.GetSelectedFilesAsViewModel();
            LbSelectedFiles.ItemsSource = _selectedFiles;

            _selectedDatasetFiles = new List<DatasetFileViewModel>();
        }

        private bool UserDatasetFilter(object item)
        {
            if (String.IsNullOrEmpty(SearchDataset.Text))
                return true;
            else
                return ((item as Dataset).Title.IndexOf(SearchDataset.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (item as Dataset).Organization.IndexOf(SearchDataset.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool UserDatasetFileFilter(object item)
        {
            if (String.IsNullOrEmpty(SearchDatasetFiles.Text))
                return true;
            else
                return ((item as DatasetFileViewModel).Title.IndexOf(SearchDatasetFiles.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (item as DatasetFileViewModel).Category.IndexOf(SearchDatasetFiles.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }


        private async void ShowFiles(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBoxItem)
            {
                if (listBoxItem.SelectedItems.Count > 0)
                {
                    Dataset selectedDataset = (Dataset)listBoxItem.SelectedItems[0];
                    if (selectedDataset != null)
                    {
                        progressBar.IsIndeterminate = true;

                        LbSelectedDatasetFiles.ItemsSource = await Task.Run(() => GetFilesAsync(selectedDataset));
                        progressBar.IsIndeterminate = false;
                        CollectionView viewDatasetFiles = (CollectionView)CollectionViewSource.GetDefaultView(LbSelectedDatasetFiles.ItemsSource);
                        if (viewDatasetFiles != null)
                        {
                            viewDatasetFiles.Filter = UserDatasetFileFilter;
                        }
                    }
                }

                BtnSelectAll.Visibility = Visibility.Visible;
                BtnSelectAll.IsChecked = false;
            }
        }

        private List<DatasetFileViewModel> GetFilesAsync(Dataset selctedDataset)
        {
            List<DatasetFileViewModel> selectedDatasetFiles = _datasetService.GetDatasetFiles(selctedDataset);

            foreach (DatasetFileViewModel selectedDatasetFile in _selectedFiles)
            {
                foreach (var datasetFile in selectedDatasetFiles)
                {
                    if (selectedDatasetFile.Id == datasetFile.Id)
                    {
                        datasetFile.SelectedForDownload = true;
                        break;
                    }
                }
            }

            if (selectedDatasetFiles.Count == 0)
            {
                MessageBox.Show("Ingen filer for dette datasettet");
            }
            _selectedDatasetFiles = selectedDatasetFiles;
            return selectedDatasetFiles;
        }

        private void AddRemove_OnChecked(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            DatasetFileViewModel datasetFile = (DatasetFileViewModel)btn.DataContext;

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
                _selectedFiles.Add(selectedFile);
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
                _selectedFiles.RemoveAll(f => f.Id == selectedFile.Id);
                BindNewList();
            }
            else
            {
                MessageBox.Show("Kunne ikke fjerne fil...");
            }
        }

        private void BindNewList()
        {
            LbSelectedFiles.ItemsSource = null;
            LbSelectedFiles.ItemsSource = _selectedFiles;
            LbSelectedDatasetFiles.ItemsSource = null;
            LbSelectedDatasetFiles.ItemsSource = _selectedDatasetFiles;
        }

        private void RemoveFromDownloadList_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            DatasetFileViewModel selectedDatasetFile = (DatasetFileViewModel)btn.DataContext;
            UpdateSelectedDatasetFiles(selectedDatasetFile);

            _selectedFiles.Remove(selectedDatasetFile);
            BindNewList();
        }



        private void UpdateSelectedDatasetFiles(DatasetFileViewModel selectedDatasetFile)
        {
            if (_selectedDatasetFiles.Any())
            {
                foreach (DatasetFileViewModel datasetFile in _selectedDatasetFiles)
                {
                    if (datasetFile.Id == selectedDatasetFile.Id)
                    {
                        datasetFile.SelectedForDownload = false;
                        break;
                    }
                }
            }
        }

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            SaveDownloadList();
        }
 
        private void SaveDownloadList()
        {
            _datasetService.WriteToDownloadFile(_selectedFiles);
        }

        private void BtnSelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (LbSelectedDatasetFiles.Items.IsEmpty) return;
            if (BtnSelectAll.IsChecked == true)
            {
                foreach (DatasetFileViewModel datasetFile in LbSelectedDatasetFiles.Items)
                {
                    if (!datasetFile.SelectedForDownload)
                    {
                        datasetFile.SelectedForDownload = true;
                        AddToList(datasetFile);
                    }
                }
            }
            else
            {
                foreach (DatasetFileViewModel datasetFile in _selectedDatasetFiles)
                {
                    if (datasetFile.SelectedForDownload)
                    {
                        datasetFile.SelectedForDownload = false;
                        RemoveFromList(datasetFile);
                    }
                }
            }

            LbSelectedDatasetFiles.ItemsSource = null;
            LbSelectedDatasetFiles.ItemsSource = _selectedDatasetFiles;
        }

        private void BtnRemoveAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedFiles.Any())
            {
                MessageBoxResult result = MessageBox.Show("Er du sikker på at du vil slette alle", "Slett alle", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    _selectedFiles = new List<DatasetFileViewModel>();
                    foreach (DatasetFileViewModel datasetfile in _selectedDatasetFiles)
                    {
                        datasetfile.SelectedForDownload = false;
                    }
                    BindNewList();
                }
                BtnSelectAll.IsChecked = false;
            }
        }

        private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            SaveDownloadList();

            string executingAssemblyDirectory = GetExecutingAssemblyDirectory();

            var pathToConsoleApp = Path.Combine(executingAssemblyDirectory, "..", "Console", "NedlastingKlient.Konsoll.exe");
            
            Process.Start(pathToConsoleApp);
        }

        private static string GetExecutingAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }


        private void SearchDataset_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(LbDatasets.ItemsSource).Refresh();
        }

        private void SearchDatasetFiles_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (LbSelectedDatasetFiles != null&& LbSelectedDatasetFiles.ItemsSource != null)
            {
                CollectionViewSource.GetDefaultView(LbSelectedDatasetFiles.ItemsSource).Refresh();
            }
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsDialog loginDialog = new SettingsDialog();
            loginDialog.ShowDialog();
        }

        private void BtnHelp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://www.geonorge.no/");
            }
            catch { }
        }
    }

}
