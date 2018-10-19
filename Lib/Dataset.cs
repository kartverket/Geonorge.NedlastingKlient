using System.Collections.Generic;

namespace Geonorge.MassivNedlasting
{
    public class Dataset
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Uuid { get; set; }
        public string Url { get; set; }
        public string LastUpdated { get; set; }
        public string Organization { get; set; }
    }

    public class DatasetViewModel
    {
        private Dataset selectedDataset;

        public string DatasetId { get; set; }
        public bool Subscribe { get; set; }
        public bool AutoDeleteFiles { get; set; }
        public bool AutoAddFiles { get; set; }
        public List<DatasetFileViewModel> Files { get; set; }

        public DatasetViewModel()
        {
            Files = new List<DatasetFileViewModel>();
        }

        public DatasetViewModel(Dataset selectedDataset, List<DatasetFileViewModel> selectedDatasetFiles)
        {
            DatasetId = selectedDataset.Title;
        }

        public DatasetViewModel(Dataset selectedDataset, bool subscribe)
        {
            DatasetId = selectedDataset.Title;
            Subscribe = subscribe;
            AutoAddFiles = subscribe;
            AutoAddFiles = subscribe;
            Files = new List<DatasetFileViewModel>();
        }
    }

}
