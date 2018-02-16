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
        private readonly List<DatasetFile> _files;
        private readonly List<DatasetFile> _selectedFiles;

        private DatasetFile _currentItem;
        private int _index = 0;

        public DatasetDetails(Dataset selectedDataset)
        {
            InitializeComponent();
            if (selectedDataset != null)
            {
                _files = new DatasetService().GetDatasetFiles(selectedDataset);
                _selectedFiles = new DatasetService().GetSelectedFiles(selectedDataset.Uuid);
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
                _currentItem = (DatasetFile)LbSelectedFiles.SelectedValue;
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
                _currentItem = (DatasetFile)LbFiles.SelectedValue;
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
            var originalDownloadedFiles = new DatasetService().GetSelectedFiles();
            var downloadedFilesByDataset = new DatasetService().GetSelectedFiles(LblDatasetUUid.Content.ToString());
            var updatedDownloadedFilesList = originalDownloadedFiles;
            var selectedFiles = new List<DatasetFile>();

            foreach (DatasetFile item in LbSelectedFiles.Items)
            {
                selectedFiles.Add(item);
            }

            // Fjern filer for aktuelt datasett
            foreach (DatasetFile datasetFile in downloadedFilesByDataset)
            {
                foreach (var downloadedFile in originalDownloadedFiles)
                {
                    if (datasetFile.GetId() == downloadedFile.GetId())
                    {
                        updatedDownloadedFilesList.Remove(downloadedFile);
                    }
                }
            }

            // Legg til valgte filer for datasett
            foreach (DatasetFile selectedFile in selectedFiles)
            {
                updatedDownloadedFilesList.Add(selectedFile);
            }

            new DatasetService().WriteToJason(updatedDownloadedFilesList);
        }
    }
}