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

        private List<DatasetFile> _selectedFiles;
        private List<DatasetFile> _datasetfiles;

        private DatasetFile _currentItem;
        private int _index = 0;

        public MainWindow()
        {
            InitializeComponent();

            DgDatasets.ItemsSource = new DatasetService().GetDatasets();

            _selectedFiles = new DatasetService().GetSelectedFiles();
            LbSelectedFiles.ItemsSource = _selectedFiles;

            _datasetfiles = new List<DatasetFile>();
        }


        private void ShowFiles(object sender, RoutedEventArgs e)
        {
            // TODO, må sjekke om noen av filene er valg. 

            if (sender is ListBox listBoxItem)
            {
                Dataset selectedDataset = (Dataset)listBoxItem.SelectedItem;

                _datasetfiles = new DatasetService().GetDatasetFiles(selectedDataset);

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
