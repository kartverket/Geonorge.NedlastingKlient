using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

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


        private void ShowFiles(object sender, RoutedEventArgs e)
        {
            // TODO, må sjekke om noen av filene er valg. 

            if (sender is ListBox listBoxItem)
            {
                Dataset selectedDataset = (Dataset)listBoxItem.SelectedItems[0];

                if (selectedDataset != null)
                {
                    var selectedDatasetFiles = new DatasetService().GetSelectedFilesAsViewModel(selectedDataset.Title);
                    _datasetfiles = new DatasetService().GetDatasetFiles(selectedDataset);

                    foreach (var selectedDatasetFile in selectedDatasetFiles)
                    {
                        foreach (var datasetFile in _datasetfiles)
                        {
                            if (selectedDatasetFile.Id == datasetFile.Id)
                            {
                                datasetFile.SelectedForDownload = true;
                                break;
                            }
                        }
                    }

                    //RemoveSelectedFilesFromFiles();


                    if (_datasetfiles.Count == 0)
                    {
                        MessageBox.Show("Ingen filer for dette datasettet");
                    }
                    LbSelectedDatasetFiles.ItemsSource = _datasetfiles;
                }
            }
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
    }

}
