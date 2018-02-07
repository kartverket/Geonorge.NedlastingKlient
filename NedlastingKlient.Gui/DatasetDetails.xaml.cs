using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NedlastingKlient.Gui
{
    /// <summary>
    ///     Interaction logic for DatasetDetails.xaml
    /// </summary>
    public partial class DatasetDetails : Page
    {
        public DatasetDetails()
        {
            InitializeComponent();
            
        }

        public Dataset Dataset { get; set; }

    }
}