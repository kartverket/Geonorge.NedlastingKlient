using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Geonorge.MassivNedlasting.Gui;

namespace Geonorge.MassivNedlasting
{
    public class Download
    {
        public string DatasetUrl { get; set; }
        public string DatasetTitle { get; set; }
        public string DatasetId { get; set; }
        public bool Subscribe  { get; set; }
        public List<DatasetFile> Files { get; set; }

        public Download()
        {
            Files = new List<DatasetFile>();
        }
    }

  

    public class DownloadViewModel
    {
        public string DatasetUrl { get; set; }
        public string DatasetId { get; set; }
        public string DatasetTitle { get; set; }
        public bool Subscribe { get; set; }
        public List<DatasetFileViewModel> Files { get; set; }

        public DownloadViewModel()
        {
            Files = new List<DatasetFileViewModel>();
        }

        public DownloadViewModel(Download download, List<Projections> projections, bool selectedForDownload = false)
        {
            DatasetUrl = download.DatasetUrl;
            DatasetTitle = download.DatasetTitle;
            DatasetId = download.DatasetId;
            if (DatasetTitle == null)
            {
                DatasetTitle = download.DatasetId;
            }


            Subscribe = download.Subscribe;
            Files = GetFiles(download, projections, selectedForDownload);
        }

        public DownloadViewModel(Dataset selectedDataset, DatasetFileViewModel selectedFile)
        {
            DatasetUrl = selectedDataset.Url;
            DatasetId = selectedDataset.Uuid;
            DatasetTitle = selectedDataset.Title;
            Subscribe = false; // TODO
            Files = AddSelectedFile(selectedFile);
        }




        // ****************

        private List<DatasetFileViewModel> AddSelectedFile(DatasetFileViewModel selectedFile)
        {
            if (Files == null)
            {
                Files = new List<DatasetFileViewModel>();
            }
            Files.Add(selectedFile);
            return Files;
        }

        private List<DatasetFileViewModel> GetFiles(Download download, List<Projections> projections, bool selectedForDownload)
        {
            var files = new List<DatasetFileViewModel>();
            foreach (var file in download.Files)
            {
                string epsgName = GetEpsgName(projections, file);
                files.Add(new DatasetFileViewModel(file, epsgName, selectedForDownload));
            }

            return files;
        }

        private static string GetEpsgName(List<Projections> projections, DatasetFile selectedFile)
        {
            var projection = projections.FirstOrDefault(p => p.Epsg == selectedFile.Projection);
            return projection != null ? projection.Name : selectedFile.Projection;
        }

    }
}
