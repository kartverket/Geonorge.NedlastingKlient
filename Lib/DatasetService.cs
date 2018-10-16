﻿using System;
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
            List<DatasetFile> datasetFiles = new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, dataset.Title, dataset.Url).OrderBy(d => d.Title).ToList();

            return ConvertToViewModel(datasetFiles, propotions);
        }

        public List<DatasetFile> GetDatasetFiles(Download download)
        {
            var getFeedTask = HttpClient.GetStringAsync(download.DatasetUrl);
            List<DatasetFile> datasetFiles = new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, download.DatasetTitle, download.DatasetUrl).OrderBy(d => d.Title).ToList();

            return datasetFiles;
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
        public void WriteToDownloadLogFile(DownloadLog downloadLog)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            try
            {
                using (var w = new StreamWriter(ApplicationService.GetDownloadLogFilePath()))
                {
                    w.WriteLine("SELECTED FILES: " + downloadLog.TotalDatasetsToDownload);
                    w.WriteLine("-------------------------------");
                    w.WriteLine();

                    
                    w.WriteLine("UPDATED: " + downloadLog.Updated.Count());
                    
                    Log(downloadLog.Updated, w);

                    w.WriteLine();

                    w.WriteLine("NOT UPDATED: " + downloadLog.NotUpdated.Count());
                    Log(downloadLog.NotUpdated, w);

                    w.WriteLine();

                    w.WriteLine("FAILED: " + downloadLog.Faild.Count());
                    Log(downloadLog.Faild, w);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void Log(List<DatasetFileLog> datasetFileLogs, TextWriter w)
        {
            w.WriteLine("-------------------------------");
            foreach (var item in datasetFileLogs.OrderBy(d => d.DatasetId))
            {
                w.Write(item.DatasetId + ";");
                w.Write(item.Name.Replace(",",";") + ";" + item.Projection);
                //if (item.HumanReadableSize != null) w.WriteLine(" - " + item.HumanReadableSize);
                w.WriteLine(); 
                if (item.Message != null) w.WriteLine(" Message: " + item.Message);
                w.WriteLine();
            }
        }


        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        public void WriteToDownloadFile(List<Download> downloads)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetDownloadFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, downloads);
                writer.Close();
            }
        }

        public void WriteToDownloadFile(List<DownloadViewModel> selectedFilesToDownloadViewModel)
        {
            var selectedFilesToDownload = ConvertToModel(selectedFilesToDownloadViewModel);
            WriteToDownloadFile(selectedFilesToDownload);
        }

        

        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        /// <param name="datasetFilesViewModel"></param>
        public void WriteToDownloadHistoryFile(List<Download> downloads)
        {
            var downloadHistory = new List<DownloadHistory>();
            foreach (var dataset in downloads)
            {
                foreach (var datasetFile in dataset.Files)
                {
                    downloadHistory.Add(new DownloadHistory(datasetFile.Url, datasetFile.FilePath));
                }
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

       
        private List<Download> ConvertToModel(List<DownloadViewModel> selectedFilesForDownload)
        {
            var downloads = new List<Download>();
            foreach (var downloadViewModel in selectedFilesForDownload)
            {
                var download = new Download(downloadViewModel);
                downloads.Add(download);
            }
            return downloads;
        }


        /// <summary>
        /// Returns a list of dataset files to download. 
        /// </summary>
        /// <returns></returns>
        public List<DatasetFile> GetSelectedDatasetFiles()
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetDownloadFilePath()))
                {
                    var json = r.ReadToEnd();
                    var selecedForDownload = JsonConvert.DeserializeObject<List<DatasetFile>>(json);
                    r.Close();
                    return selecedForDownload;
                }
            }
            catch (Exception)
            {
                // TODO error handling
                return new List<DatasetFile>();
            }
        }

        /// <summary>
        /// Returns a list of dataset files to download. 
        /// </summary>
        /// <returns></returns>
        public List<Download> GetSelectedFilesToDownload()
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetDownloadFilePath()))
                {
                    var json = r.ReadToEnd();
                    var selecedForDownload = JsonConvert.DeserializeObject<List<Download>>(json);
                    r.Close();
                    selecedForDownload = RemoveDuplicatesIterative(selecedForDownload);
                    selecedForDownload = ConvertToNewVersionOfDownloadFile(selecedForDownload);

                    return selecedForDownload;
                }
            }
            catch (Exception)
            {
                // TODO error handling
                return new List<Download>();
            }
        }

        
        private List<Download> ConvertToNewVersionOfDownloadFile(List<Download> selecedForDownload)
        {
            var newListOfDatasetForDownload = new List<Download>();
            foreach (var dataset in selecedForDownload)
            {
                if (!dataset.Files.Any())
                {
                    dataset.Files = ConvertToNewVersionOfDownloadFile(dataset);
                    newListOfDatasetForDownload.Add(dataset);
                }
                else
                {
                    // if dataset file hase items, it is the new version of download file. 
                    return selecedForDownload;
                }
            }

            return newListOfDatasetForDownload;
        }

        public static List<Download> RemoveDuplicatesIterative(List<Download> items)
        {
            // Use HashSet to maintain table of duplicates encountered.
            var result = new List<Download>();
            var set = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                // If not duplicate, add to result.
                if (!set.Contains(items[i].DatasetTitle)) // TODO, bytte med id.
                {
                    result.Add(items[i]);
                    // Record as a future duplicate.
                    set.Add(items[i].DatasetTitle); // TODO, bytte med id.
                }
            }
            return result;
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

        

        public List<DownloadViewModel> GetSelectedFilesToDownloadAsViewModel(List<Projections> propotions)
        {
            List<Download> selectedFiles = GetSelectedFilesToDownload();
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

        private List<DownloadViewModel> ConvertToViewModel(List<Download> datasetFilesToDownload, List<Projections> projections, bool selectedForDownload = false)
        {
            var selectedFilesViewModel = new List<DownloadViewModel>();
            foreach (var dataset in datasetFilesToDownload)
            {
                DownloadViewModel selectedFileViewModel = new DownloadViewModel(dataset, projections, selectedForDownload);
                selectedFilesViewModel.Add(selectedFileViewModel);
            }
            return selectedFilesViewModel;
        }

        private List<DatasetFile> ConvertToNewVersionOfDownloadFile(Download dataset)
        {
            var datasetFiles = GetSelectedDatasetFiles();
            foreach (var file in datasetFiles)
            {
                // TODO gamle dewnload filer har ikke dataset uuid.. 
                if ((file.DatasetId == dataset.DatasetTitle) || (file.DatasetId == dataset.DatasetId))
                {
                    dataset.Files.Add(file);
                }
            }
            return dataset.Files;
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

        private static string GetEpsgName(List<Projections> projections, DatasetFile selectedFile)
        {
            var projection = projections.FirstOrDefault(p => p.Epsg == selectedFile.Projection);
            return projection != null ? projection.Name : selectedFile.Projection;
        }
    }
}