using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Geonorge.MassivNedlasting.Gui;
using Geonorge.Nedlaster;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Geonorge.MassivNedlasting
{
    public class DatasetService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public List<Dataset> GetDatasets()
        {
            var getFeedTask = HttpClient.GetStringAsync("https://nedlasting.geonorge.no/geonorge/Tjenestefeed_daglig.xml");
            return new AtomFeedParser().ParseDatasets(getFeedTask.Result);
        }

        public List<DatasetFileViewModel> GetDatasetFiles(Dataset dataset, List<Projections> propotions)
        {
            var getFeedTask = HttpClient.GetStringAsync(dataset.Url);
            List<DatasetFile> datasetFiles = new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, dataset).OrderBy(d => d.Title).ToList();

            return ConvertToViewModel(datasetFiles, propotions);
        }

        public List<Projections> FetchProjections()
        {
            List<Projections> projections = new List<Projections>();

            var url = "https://register.geonorge.no/api/register/epsg-koder.json";
            var c = new System.Net.WebClient { Encoding = System.Text.Encoding.UTF8 };

            var json = c.DownloadString(url);

            dynamic data = JObject.Parse(json);
            if (data != null)
            {
                var result = data["containeditems"]; ;
                foreach (var item in result)
                {
                    projections.Add(new Projections(item));
                }
                Task.Run(() => WriteToProjectionFile(projections));
            }
            return projections;
        }

        /// <summary>
        /// Returns a list of projections. 
        /// </summary>
        /// <returns></returns>
        public List<Projections> ReadFromProjectionFile()
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetProjectionFilePath()))
                {
                    var json = r.ReadToEnd();
                    var selecedFiles = JsonConvert.DeserializeObject<List<Projections>>(json);
                    r.Close();
                    return selecedFiles;
                }
            }
            catch (Exception)
            {
                return new List<Projections>();
            }
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

        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        public void WriteToDownloadLogFile(DownloadLog downloadLog)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var w = new StreamWriter(ApplicationService.GetDownloadLogFilePath()))
            {
                w.WriteLine("SELECTED FILES: " + downloadLog.TotalDatasetsToDownload);
                w.WriteLine("-------------------------------");
                w.WriteLine();

                w.WriteLine("UPDATED: " + downloadLog.Updated.Count() + " TOTAL SIZE: " + downloadLog.TotalSizeOfDownloadedFiles);
                Log(downloadLog.Updated, w);

                w.WriteLine();

                w.WriteLine("NOT UPDATED: " + downloadLog.NotUpdated.Count());
                Log(downloadLog.NotUpdated, w);

                w.WriteLine();

                w.WriteLine("FAILED: " + downloadLog.Faild.Count());
                Log(downloadLog.Faild, w);
            }
        }

        private void Log(List<DatasetFileLog> datasetFileLogs, TextWriter w)
        {
            w.WriteLine("-------------------------------");
            foreach (var item in datasetFileLogs.OrderBy(d => d.DatasetId))
            {
                w.Write(item.DatasetId + " - ");
                w.Write(item.Name + " " + item.Projection);
                if (item.HumanReadableSize != null) w.WriteLine(" - " + item.HumanReadableSize);
                else { w.WriteLine(); }
                if (item.Message != null) w.WriteLine(" Message: " + item.Message);
                w.WriteLine();
            }
        }

        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        public void WriteToDownloadFile(List<DatasetFile> datasetFiles)
        {
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

        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        /// <param name="datasetFilesViewModel"></param>
        public void WriteToDownloadHistoryFile(List<DatasetFile> datasetFilesToDownload)
        {
            var downloadHistory = new List<DownloadHistory>();
            foreach (var datasetFile in datasetFilesToDownload)
            {
                downloadHistory.Add(new DownloadHistory(datasetFile.Url, datasetFile.FilePath));
            }

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetDownloadHistoryFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, downloadHistory);
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
        /// <returns></returns>
        public List<DatasetFile> GetSelectedFiles()
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetDownloadFilePath()))
                {
                    var json = r.ReadToEnd();
                    var selecedFiles = JsonConvert.DeserializeObject<List<DatasetFile>>(json);
                    r.Close();
                    return selecedFiles;
                }
            }
            catch (Exception)
            {
                // TODO error handling
                return new List<DatasetFile>();
            }
        }

        /// <summary>
        /// Returns a list of downloded datasets. 
        /// </summary>
        /// <returns></returns>
        public DownloadHistory GetFileDownloaHistory(string url)
        {
            var downloadHistoryFilePath = ApplicationService.GetDownloadHistoryFilePath();
            try
            {
                using (var r = new StreamReader(downloadHistoryFilePath))
                {
                    var json = r.ReadToEnd();
                    var downloadHistories = JsonConvert.DeserializeObject<List<DownloadHistory>>(json);
                    r.Close();
                    DownloadHistory downloadHistory = downloadHistories.FirstOrDefault(d => d.Id == url);
                    return downloadHistory;
                }
            }
            catch (FileNotFoundException)
            {
                return null;

            }
        }

        public List<DatasetFileViewModel> GetSelectedFilesAsViewModel(List<Projections> propotions)
        {
            List<DatasetFile> selectedFiles = GetSelectedFiles();
            return ConvertToViewModel(selectedFiles, propotions, true);
        }

        private List<DatasetFileViewModel> ConvertToViewModel(List<DatasetFile> datasetFiles, List<Projections> projections, bool selectedForDownload = false)
        {
            var selectedFilesViewModel = new List<DatasetFileViewModel>();
            foreach (var selectedFile in datasetFiles)
            {
                string epsgName = GetEpsgName(projections, selectedFile);
                DatasetFileViewModel selectedFileViewModel = new DatasetFileViewModel(selectedFile, epsgName, selectedForDownload);
                selectedFilesViewModel.Add(selectedFileViewModel);
            }
            return selectedFilesViewModel;
        }

        private static string GetEpsgName(List<Projections> projections, DatasetFile selectedFile)
        {
            var projection = projections.FirstOrDefault(p => p.Epsg == selectedFile.Projection);
            return projection != null ? projection.Name : selectedFile.Projection;
        }

        public void WriteToProjectionFile(List<Projections> projections)
        {
            var serializer = new JsonSerializer();

            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetProjectionFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, projections);
                writer.Close();
            }
        }
    }
}