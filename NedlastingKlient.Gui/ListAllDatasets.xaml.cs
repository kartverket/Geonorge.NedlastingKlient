using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace NedlastingKlient.Gui
{
    /// <summary>
    ///     Interaction logic for ListAllDatasets.xaml
    /// </summary>
    public partial class ListAllDatasets : Page
    {
        //public ICommand ShowProgressDialogCommand { get; }

        private List<DatasetFile> _selectedFiles;
        private List<DatasetFile> _datasetfiles;
        //private List<DatasetFile> _selectedDatasetFiles;

        private DatasetFile _currentItem;
        private int _index = 0;

        public ListAllDatasets()
        {
            InitializeComponent();

            DgDatasets.ItemsSource = new DatasetService().GetDatasets();

            _selectedFiles = new DatasetService().GetSelectedFiles();
            LbSelectedFiles.ItemsSource = _selectedFiles;

            _datasetfiles = new List<DatasetFile>();

            if (_selectedFiles == null)
            {
                BtnSave.IsEnabled = false;
            }
            //_selectedDatasetFiles = new List<DatasetFile>();
        }


        private void ShowFiles(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBoxItem)
            {
                Dataset selectedDataset = (Dataset)listBoxItem.SelectedItem;
                //MessageBox.Show("Henter filer...");

                _datasetfiles = new DatasetService().GetDatasetFiles(selectedDataset);
                //_selectedDatasetFiles = new DatasetService().GetSelectedFiles(selectedDataset.Title);

                if (_datasetfiles.Count == 0)
                {
                    MessageBox.Show("Ingen filer for dette datasettet");
                }
                LbSelectedDatasetFiles.ItemsSource = _datasetfiles;
            }
        }


        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            DatasetFile datasetFile = (DatasetFile)btn.DataContext;

            if (btn.IsChecked == true)
            {
                AddToList(datasetFile);
                btn.ToolTip = "Valgt for nedlasting";
            }
            else
            {
                RemoveFromList(datasetFile);
                btn.ToolTip = "Legg til";
            }
        }

        private void AddToList(DatasetFile selectedFile)
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

        private void RemoveFromList(DatasetFile selectedFile)
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
            DatasetFile datasetFile = (DatasetFile)btn.DataContext;

            _selectedFiles.Remove(datasetFile);
            BindNewList();
        }

        private void SaveList(object sender, RoutedEventArgs e)
        {
            new DatasetService().WriteToDownloadFile(_selectedFiles);
            MessageBox.Show("Lagring, OK!");
        }
    }
}