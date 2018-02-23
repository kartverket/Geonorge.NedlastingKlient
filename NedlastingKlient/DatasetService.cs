using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NedlastingKlient
{
    public class DatasetService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public List<Dataset> GetDatasets()
        {
            var getFeedTask = HttpClient.GetStringAsync("https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml");
            return new AtomFeedParser().ParseDatasets(getFeedTask.Result);
        }

        public List<DatasetFileViewModel> GetDatasetFiles(Dataset dataset)
        {
            var getFeedTask = HttpClient.GetStringAsync(dataset.Url);
            List<DatasetFile> datasetFiles = new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, dataset).OrderBy(d => d.Title).ToList();

            return ConvertToViewModel(datasetFiles);
        }

        public DatasetFile GetDatasetFile(DatasetFile originalDatasetFile)
        {
            var getFeedTask = HttpClient.GetStringAsync(originalDatasetFile.DatasetUrl);
            return new AtomFeedParser().ParseDatasetFile(getFeedTask.Result, originalDatasetFile);
        }

        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        /// <param name="datasetFilesViewModel"></param>
        public void WriteToDownloadFile(List<DatasetFileViewModel> datasetFilesViewModel)
        {
            List<DatasetFile> datasetFiles = ConvertToModel(datasetFilesViewModel);
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetDownloadFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, datasetFiles);
                writer.Close();
            }
        }

        private List<DatasetFile> ConvertToModel(List<DatasetFileViewModel> datasetFilesViewModel)
        {
            var datasetFiles = new List<DatasetFile>();
            foreach (var datasetFileViewModel in datasetFilesViewModel)
            {
                var datasetFile = new DatasetFile(datasetFileViewModel);
                datasetFiles.Add(datasetFile);
            }
            return datasetFiles;
        }

        /// <summary>
        /// Returns a list of dataset files to download. 
        /// </summary>
        /// <param name="datasetTitle">search for dataset with given title. List will only return dataset that matches.</param>
        /// <returns></returns>
        public List<DatasetFileViewModel> GetSelectedFiles(string datasetTitle = null)
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetDownloadFilePath()))
                {
                    var json = r.ReadToEnd();
                    var selecedFiles = JsonConvert.DeserializeObject<List<DatasetFile>>(json);
                    r.Close();
                    List<DatasetFile> selectedFiles = datasetTitle != null
                        ? selecedFiles.Where(f => f.DatasetId == datasetTitle).ToList()
                        : selecedFiles;

                    return ConvertToViewModel(selectedFiles, true);
                }
            }
            catch (Exception)
            {
                // TODO error handling
                return new List<DatasetFileViewModel>();
            }
        }

        private List<DatasetFileViewModel> ConvertToViewModel(List<DatasetFile> datasetFiles, bool selectedForDownload = false)
        {
            var selectedFilesViewModel = new List<DatasetFileViewModel>();
            foreach (var selectedFile in datasetFiles)
            {
                DatasetFileViewModel selectedFileViewModel = new DatasetFileViewModel(selectedFile, selectedForDownload);
                selectedFilesViewModel.Add(selectedFileViewModel);
            }
            return selectedFilesViewModel;
        }
    }
}