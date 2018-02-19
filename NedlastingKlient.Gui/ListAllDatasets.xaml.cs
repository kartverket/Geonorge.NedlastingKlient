using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NedlastingKlient.Gui
{
    /// <summary>
    ///     Interaction logic for ListAllDatasets.xaml
    /// </summary>
    public partial class ListAllDatasets : Page
    {
        public ListAllDatasets()
        {
            InitializeComponent();
            DgDatasets.ItemsSource = new DatasetService().GetDatasets();
            LbSelectedFiles.ItemsSource = new DatasetService().GetSelectedFiles();
        }

        private void ShowDatasetClick(object sender, RoutedEventArgs e)
        {
            Dataset selectedDataset = DgDatasets.SelectedItem as Dataset;

            DatasetDetails detailsPage = new DatasetDetails(selectedDataset);

            NavigationService.Navigate(detailsPage, selectedDataset);
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != DgDatasets)
            {
                if (obj.GetType() == typeof(TextBlock))
                {
                    Dataset selectedDataset = DgDatasets.SelectedItem as Dataset;

                    DatasetDetails detailsPage = new DatasetDetails(selectedDataset);

                    NavigationService.Navigate(detailsPage, selectedDataset);
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }
    }
}