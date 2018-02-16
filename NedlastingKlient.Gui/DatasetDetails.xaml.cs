using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NedlastingKlient.Gui
{
    /// <inheritdoc cref="" />
    /// <summary>
    ///     Interaction logic for DatasetDetails.xaml
    /// </summary>
    public partial class DatasetDetails : Page
    {
        private readonly List<Dataset> _files;
        private readonly List<Dataset> _selectedFiles;

        private Dataset _currentItem;
        private int _index = 0;

        public DatasetDetails(Dataset selectedDataset)
        {
            InitializeComponent();
            if (selectedDataset != null)
            {
                _files = new DatasetService().GetDatasets(selectedDataset.Url);
                _selectedFiles = new DatasetService().GetSelectedFiles();
                RemoveSelectedFilesFromFiles();
                LbFiles.ItemsSource = _files;
                LbSelectedFiles.ItemsSource = _selectedFiles;
                
                LblDatasetName.Content = selectedDataset.Title;
                LblDatasetDescription.Content = selectedDataset.Description;
                LblDatasetOwner.Content = selectedDataset.Organization;
                LblDatasetUUid.Content = selectedDataset.Uuid;
                LblDatasetLastUpdated.Content = selectedDataset.LastUpdated;
            }
        }

        private void RemoveSelectedFilesFromFiles()
        {
            // TODO Her må vi bruke id.. eller url.. Kan være flere med samme navn..
            var remove = new List<Dataset>();
            foreach (var selectedFile in _selectedFiles)
            {
                var item = _files.FirstOrDefault(f => f.Title == selectedFile.Title);
                if (item != null) _files.Remove(item);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LbSelectedFiles.SelectedValue != null)
            {
                _currentItem = (Dataset)LbSelectedFiles.SelectedValue;
                _index = LbSelectedFiles.SelectedIndex;
                _files.Add(_currentItem);
                _selectedFiles?.RemoveAt(_index);

                BindNewList();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (LbFiles.SelectedValue != null)
            {
                _currentItem = (Dataset)LbFiles.SelectedValue;
                _index = LbFiles.SelectedIndex;

                _selectedFiles.Add(_currentItem);
                _files?.RemoveAt(_index);
                BindNewList();
            }
        }

        private void BindNewList()
        {
            LbFiles.ItemsSource = null;
            LbSelectedFiles.ItemsSource = null;
            LbFiles.ItemsSource = _files.OrderBy(o => o.Title);
            LbSelectedFiles.ItemsSource = _selectedFiles;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = new List<Dataset>();
            foreach (Dataset item in LbSelectedFiles.Items)
            {
                selectedFiles.Add(item);
            }
            new DatasetService().WriteToJason(selectedFiles);
        }
    }
}