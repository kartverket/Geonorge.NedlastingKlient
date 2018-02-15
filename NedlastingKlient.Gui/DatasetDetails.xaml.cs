using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NedlastingKlient.Gui
{
    /// <inheritdoc cref="" />
    /// <summary>
    ///     Interaction logic for DatasetDetails.xaml
    /// </summary>
    public partial class DatasetDetails : Page
    {
        private readonly List<Dataset> _myList;
        private Dataset _currentItem;
        private int _index = 0;

        public DatasetDetails(Dataset selectedDataset)
        {
            InitializeComponent();
            if (selectedDataset != null)
            {
                _myList = new DatasetService().GetDatasetFiles(selectedDataset.Url);
                LbFiles.ItemsSource = _myList;
                LblDatasetName.Content = selectedDataset.Title;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LbSelectedFiles.SelectedValue != null)
            {
                _currentItem = (Dataset)LbSelectedFiles.SelectedValue;
                _index = LbSelectedFiles.SelectedIndex;
                _myList.Add(_currentItem);
                LbSelectedFiles.Items.RemoveAt(_index);
                BindNewList();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (LbFiles.SelectedValue != null)
            {
                _currentItem = (Dataset)LbFiles.SelectedValue;
                _index = LbFiles.SelectedIndex;

                LbSelectedFiles.Items.Add(_currentItem);
                _myList?.RemoveAt(_index);
                BindNewList();
            }
        }

        private void BindNewList()
        {
            LbFiles.ItemsSource = null;
            LbFiles.ItemsSource = _myList.OrderBy(o => o.Title);
        }
    }
}