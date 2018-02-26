using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
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
        private List<DatasetFileViewModel> _datasetfiles;
        public List<Dataset> _Datasets;

        public MainWindow()
        {
            InitializeComponent();

            _Datasets = new DatasetService().GetDatasets();
            DgDatasets.ItemsSource = _Datasets;


            _selectedFiles = new DatasetService().GetSelectedFilesAsViewModel();
            LbSelectedFiles.ItemsSource = _selectedFiles;

            _datasetfiles = new List<DatasetFileViewModel>();
        }


        private async void ShowFiles(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBoxItem)
            {
                Dataset selectedDataset = (Dataset)listBoxItem.SelectedItems[0];
                if (selectedDataset != null)
                {
                    progressBar.IsIndeterminate = true;

                    LbSelectedDatasetFiles.ItemsSource = await Task.Run(() => GetFilesAsync(selectedDataset));
                    progressBar.IsIndeterminate = false;
                }
            }
        }

        private List<DatasetFileViewModel> GetFilesAsync(Dataset selctedDataset)
        {

            List<DatasetFileViewModel> selectedDatasetFiles = new DatasetService().GetDatasetFiles(selctedDataset);

            foreach (DatasetFileViewModel selectedDatasetFile in LbSelectedFiles.Items)
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
            _datasetfiles = selectedDatasetFiles;
            return selectedDatasetFiles;
        }


        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
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
                _selectedFiles.Remove(selectedFile);
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
        }

        private void RemoveFromDownloadList(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            DatasetFileViewModel datasetFile = (DatasetFileViewModel)btn.DataContext;

            _selectedFiles.Remove(datasetFile);
            BindNewList();
        }

        private void SaveList(object sender, RoutedEventArgs e)
        {
            new DatasetService().WriteToDownloadFile(_selectedFiles);
            MessageBox.Show("Lagring, OK!");
        }

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Vil du lagre før du avslutter?", "Lagre", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
                SaveList(null, null);
        }

        private void LoadedWindow(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (LbSelectedDatasetFiles.Items.IsEmpty) return;
            if (BtnSelectAll.IsChecked == true)
            {
                foreach (DatasetFileViewModel datasetFile in _datasetfiles)
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
                foreach (DatasetFileViewModel datasetFile in _datasetfiles)
                {
                    if (datasetFile.SelectedForDownload)
                    {
                        datasetFile.SelectedForDownload = false;
                        RemoveFromList(datasetFile);
                    }
                }
            }
            LbSelectedDatasetFiles.ItemsSource = null;
            LbSelectedDatasetFiles.ItemsSource = _datasetfiles;
        }

        private void BtnRemoveAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedFiles.Any())
            {
                MessageBoxResult result = MessageBox.Show("Er du sikker på at du vil slette alle", "Slett alle",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    _selectedFiles = new List<DatasetFileViewModel>();
                    BindNewList();
                    foreach (var datasetfile in _datasetfiles)
                    {
                        datasetfile.SelectedForDownload = false;
                    }
                    LbSelectedDatasetFiles.ItemsSource = _datasetfiles;
                    new DatasetService().WriteToDownloadFile(_selectedFiles);
                }
            }
        }
    }

}
