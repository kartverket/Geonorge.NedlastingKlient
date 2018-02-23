using System;

namespace NedlastingKlient
{

    public class DatasetFile
    {
        private DatasetFileViewModel datasetFileViewModel;

        public DatasetFile(DatasetFileViewModel datasetFileViewModel)
        {
            Title = datasetFileViewModel.Title;
            Description = datasetFileViewModel.Description;
            Url = datasetFileViewModel.Url;
            LastUpdated = datasetFileViewModel.LastUpdated;
            Organization = datasetFileViewModel.Organization;
            Category = datasetFileViewModel.Category;
            DatasetId = datasetFileViewModel.DatasetId;
            DatasetUrl = datasetFileViewModel.DatasetUrl;
        }

        public DatasetFile()
        {
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LastUpdated { get; set; }
        public string Organization { get; set; }
        public string Category { get; set; }
        public string DatasetId { get; set; }
        public string DatasetUrl { get; set; }

        public string GetId()
        {
            return Title + "_" + Category;
        }
    }

    public class DatasetFileViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LastUpdated { get; set; }
        public string Organization { get; set; }
        public string Category { get; set; }
        public string DatasetId { get; set; }
        public string DatasetUrl { get; set; }
        public bool SelectedForDownload { get; set; }

        public string GetId()
        {
            return Title + "_" + Category;
        }

        public DatasetFileViewModel(DatasetFile datasetFile, bool selectedForDownload = false)
        {
            Title = datasetFile.Title;
            Description = datasetFile.Description;
            Url = datasetFile.Url;
            LastUpdated = datasetFile.LastUpdated;
            Organization = datasetFile.Organization;
            Category = datasetFile.Category;
            DatasetId = datasetFile.DatasetId;
            DatasetUrl = datasetFile.DatasetUrl;
            Id = GetId();
            SelectedForDownload = selectedForDownload;
        }
    }
}
