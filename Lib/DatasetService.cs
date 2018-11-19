using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
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
        private ConfigFile _configFile = ConfigFile.GetDefaultConfigFile();

        public DatasetService()
        {
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"GeonorgeNedlastingsklient/{Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
        }

        

        /// <summary>
        /// Use selected config file when using download service. 
        /// </summary>
        /// <param name="configFile">Selected config file</param>
        public DatasetService(ConfigFile configFile)
        {
            _configFile = configFile;
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"GeonorgeNedlastingsklient/{Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
        }

        /// <summary>
        /// Return dataset from feed "https://nedlasting.geonorge.no/geonorge/Tjenestefeed_daglig.xml"
        /// </summary>
        /// <returns></returns>
        public List<Dataset> GetDatasets()
        {
            var getFeedTask = HttpClient.GetStringAsync("https://nedlasting.geonorge.no/geonorge/Tjenestefeed_daglig.xml");
            return new AtomFeedParser().ParseDatasets(getFeedTask.Result);
        }

        /// <summary>
        /// Parse dataset file from feed and return as DatasetFile
        /// </summary>
        /// <param name="originalDatasetFile">local dataset file</param>
        /// <returns></returns>
        public DatasetFile GetDatasetFile(DatasetFile originalDatasetFile)
        {
            var getFeedTask = HttpClient.GetStringAsync(originalDatasetFile.DatasetUrl);
            return new AtomFeedParser().ParseDatasetFile(getFeedTask.Result, originalDatasetFile);
        }

        /// <summary>
        /// Parse dataset files from feed
        /// </summary>
        /// <param name="download">Local dataset</param>
        /// <returns></returns>
        public List<DatasetFile> GetDatasetFiles(Download download)
        {
            var getFeedTask = HttpClient.GetStringAsync(download.DatasetUrl);
            List<DatasetFile> datasetFiles = new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, download.DatasetTitle, download.DatasetUrl).OrderBy(d => d.Title).ToList();

            return datasetFiles;
        }

        /// <summary>
        /// Parse dataset files and return as view model
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="propotions"></param>
        /// <returns></returns>
        public List<DatasetFileViewModel> GetDatasetFiles(Dataset dataset, List<Projections> propotions)
        {
            var getFeedTask = HttpClient.GetStringAsync(dataset.Url);
            List<DatasetFile> datasetFiles = new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, dataset.Title, dataset.Url).OrderBy(d => d.Title).ToList();

            return ConvertToViewModel(datasetFiles, propotions);
        }

        /// <summary>
        /// Fetch projections from epsg-registry - https://register.geonorge.no/register/epsg-koder
        /// </summary>
        /// <returns></returns>
        public List<Projections> FetchProjections()
        {
            List<Projections> projections = new List<Projections>();

            var url = "https://register.geonorge.no/api/epsg-koder.json";
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
        /// Fetch a list of download usage group from metadata-codelist registry - https://register.geonorge.no/api/metadata-kodelister/brukergrupper.json
        /// </summary>
        /// <returns></returns>
        public List<string> FetchDownloadUsageGroups()
        {
            List<string> groups = new List<string>();

            var url = "https://register.geonorge.no/api/metadata-kodelister/brukergrupper.json";
            var c = new System.Net.WebClient { Encoding = Encoding.UTF8 };

            var json = c.DownloadString(url);

            dynamic data = JObject.Parse(json);
            if (data != null)
            {
                var result = data["containeditems"]; ;
                foreach (var item in result)
                {
                    groups.Add(item.label.ToString());
                }
                Task.Run(() => WriteToUsageGroupFile(groups));
            }
            return groups;
        }

        /// <summary>
        /// Fetch a list of download puroses from metadata-codelist registry - https://register.geonorge.no/api/metadata-kodelister/formal.json
        /// </summary>
        /// <returns></returns>
        public List<string> FetchDownloadUsagePurposes()
        {
            List<string> purposes = new List<string>();

            var url = "https://register.geonorge.no/api/metadata-kodelister/formal.json";
            var c = new System.Net.WebClient { Encoding = Encoding.UTF8 };

            var json = c.DownloadString(url);

            dynamic data = JObject.Parse(json);
            if (data != null)
            {
                var result = data["containeditems"]; ;
                foreach (var item in result)
                {
                    purposes.Add(item.label.ToString());
                }
                Task.Run(() => WriteToUsagePurposeFile(purposes));
            }
            return purposes;
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
                    var propotions = JsonConvert.DeserializeObject<List<Projections>>(json);
                    r.Close();
                    return propotions;
                }
            }
            catch (Exception e)
            {
                return new List<Projections>();
            }
        }

        /// <summary>
        /// Returns a list of projections. 
        /// </summary>
        /// <returns></returns>
        public List<string> ReadFromDownloadUsageGroup()
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetUserGroupsFilePath()))
                {
                    var json = r.ReadToEnd();
                    var userGroups = JsonConvert.DeserializeObject<List<string>>(json);
                    r.Close();
                    return userGroups;
                }
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns a list of projections. 
        /// </summary>
        /// <returns></returns>
        public List<string> ReadFromDownloadUsagePurposes()
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetPurposesFilePath()))
                {
                    var json = r.ReadToEnd();
                    var upurposes = JsonConvert.DeserializeObject<List<string>>(json);
                    r.Close();
                    return upurposes;
                }
            }
            catch (Exception)
            {
                return new List<string>();
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
            try
            {
                using (var w = new StreamWriter(ApplicationService.GetDownloadLogFilePath(_configFile.LogDirectory)))
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


        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        public void WriteToConfigFile(List<Download> downloads)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(_configFile.FilePath, false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, downloads);
                writer.Close();
            }
        }

        /// <summary>
        /// Write selected files to download to config file. 
        /// </summary>
        /// <param name="selectedFilesToDownloadViewModel"></param>
        public void WriteToConfigFile(List<DownloadViewModel> selectedFilesToDownloadViewModel)
        {
            var selectedFilesToDownload = ConvertToModel(selectedFilesToDownloadViewModel);
            WriteToConfigFile(selectedFilesToDownload);
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

            using (var outputFile = new StreamWriter(ApplicationService.GetDownloadHistoryFilePath(_configFile.Name), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, downloadHistory);
                writer.Close();
            }
        }


        private void Log(List<DatasetFileLog> datasetFileLogs, TextWriter w)
        {
            w.WriteLine("-------------------------------");
            foreach (var item in datasetFileLogs.OrderBy(d => d.DatasetId))
            {
                w.Write(item.DatasetId + ";");
                w.Write(item.Name.Replace(",", ";") + ";" + item.Projection);
                w.WriteLine();
                if (item.Message != null) w.WriteLine(" Message: " + item.Message);
                w.WriteLine();
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
                using (var r = new StreamReader(_configFile.FilePath))
                {
                    var json = r.ReadToEnd();
                    var selecedForDownload = JsonConvert.DeserializeObject<List<DatasetFile>>(json);
                    r.Close();
                    return selecedForDownload;
                }
            }
            catch (Exception)
            {
                return new List<DatasetFile>();
            }
        }

        /// <summary>
        /// Returns a list of dataset files to download. 
        /// </summary>
        /// <returns></returns>
        public List<Download> GetSelectedFilesToDownload(ConfigFile configFile = null)
        {
            var downloadFilePath = _configFile != null ? _configFile.FilePath : ApplicationService.GetDownloadFilePath();
            try
            {
                using (var r = new StreamReader(downloadFilePath))
                {
                    var json = r.ReadToEnd();
                    var selecedForDownload = JsonConvert.DeserializeObject<List<Download>>(json);
                    r.Close();
                    selecedForDownload = RemoveDuplicatesIterative(selecedForDownload);
                    selecedForDownload = ConvertToNewVersionOfDownloadFile(selecedForDownload);

                    return selecedForDownload;
                }
            }
            catch (Exception e)
            {
                // TODO error handling
                return new List<Download>();
            }
        }


        
        private List<Download> ConvertToNewVersionOfDownloadFile(List<Download> downloads)
        {
            var newListOfDatasetForDownload = new List<Download>();
            var datasetFilesSelectedForDownload = GetSelectedDatasetFilesFromDownloadFile();
            foreach (var download in downloads)
            {
                if (!download.Files.Any() && !download.Subscribe)
                {
                    download.Files = ConvertToNewVersionOfDownloadFile(download, datasetFilesSelectedForDownload);
                    newListOfDatasetForDownload.Add(download);
                }
                else
                {
                    // if dataset file hase items, it is the new version of download file. 
                    return downloads;
                }
            }

            return newListOfDatasetForDownload;
        }

        private List<DatasetFile> GetSelectedDatasetFilesFromDownloadFile()
        {
            var downloadFileInfo = new FileInfo(ApplicationService.GetOldDownloadFilePath());
            if (downloadFileInfo.Exists)
            {
                try
                {
                    using (var r = new StreamReader(ApplicationService.GetOldDownloadFilePath()))
                    {
                        var json = r.ReadToEnd();
                        var selecedForDownload = JsonConvert.DeserializeObject<List<DatasetFile>>(json);
                        r.Close();
                        return selecedForDownload;
                    }
                }
                catch (Exception)
                {
                    return new List<DatasetFile>();
                }
            }
            return new List<DatasetFile>();
        }


        private List<Download> GetSelectedDatasetFilesFromDownloadFileAsDownloadModel()
        {
            var downloadFileInfo = new FileInfo(ApplicationService.GetOldDownloadFilePath());
            if (downloadFileInfo.Exists)
            {
                try
                {
                    using (var r = new StreamReader(ApplicationService.GetOldDownloadFilePath()))
                    {
                        var json = r.ReadToEnd();
                        var selecedForDownload = JsonConvert.DeserializeObject<List<Download>>(json);
                        r.Close();
                        return selecedForDownload;
                    }
                }
                catch (Exception)
                {
                    return new List<Download>();
                }
            }
            return new List<Download>();
        }


        /// <summary>
        /// Part of converting from old version of config file. 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<Download> RemoveDuplicatesIterative(List<Download> items)
        {
            var result = new List<Download>();
            var set = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                if (!set.Contains(items[i].DatasetTitle))
                {
                    result.Add(items[i]);
                    set.Add(items[i].DatasetTitle);
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
            var downloadHistoryFilePath = ApplicationService.GetDownloadHistoryFilePath(_configFile.Name);
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

        /// <summary>
        /// Returns selected files to download as downlaod view models
        /// </summary>
        /// <param name="propotions"></param>
        /// <returns></returns>
        public List<DownloadViewModel> GetSelectedFilesToDownloadAsViewModel(List<Projections> propotions)
        {
            List<Download> selectedFiles = GetSelectedFilesToDownload(_configFile);
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

        private List<DatasetFile> ConvertToNewVersionOfDownloadFile(Download dataset, List<DatasetFile> datasetFilesSelectedForDownload)
        {
            foreach (var file in datasetFilesSelectedForDownload)
            {
                // TODO gamle dewnload filer har ikke dataset uuid.. 
                if ((file.DatasetId == dataset.DatasetTitle) || (file.DatasetId == dataset.DatasetId))
                {
                    dataset.Files.Add(file);
                }
            }
            return dataset.Files;
        }


        /// <summary>
        /// Write list of projections to file in case epsg-registry won't respond
        /// </summary>
        /// <param name="projections"></param>
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

        /// <summary>
        /// Write list of download usage to file in case registry api won't respond
        /// </summary>
        /// <param name="userGroups">list of user groups</param>
        public void WriteToUsageGroupFile(List<string> userGroups)
        {
            var serializer = new JsonSerializer();

            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetUserGroupsFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, userGroups);
                writer.Close();
            }
        }

        /// <summary>
        /// /// Write list of download usage to file in case registry api won't respond
        /// </summary>
        /// <param name="userPurposes">list of purposes</param>
        public void WriteToUsagePurposeFile(List<string> userPurposes)
        {
            var serializer = new JsonSerializer();

            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetPurposesFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, userPurposes);
                writer.Close();
            }
        }



        private static string GetEpsgName(List<Projections> projections, DatasetFile selectedFile)
        {
            var projection = projections.FirstOrDefault(p => p.Epsg == selectedFile.Projection);
            return projection != null ? projection.Name : selectedFile.Projection;
        }

        /// <summary>
        /// Post selected download usage to "Nedlasting api" 
        /// </summary>
        /// <param name="downloadUsage"></param>
        public void SendDownloadUsage(DownloadUsage downloadUsage)
        {
            if (downloadUsage != null && downloadUsage.Entries.Any())
            {
                var json = JsonConvert.SerializeObject(downloadUsage);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                string token = !string.IsNullOrWhiteSpace(AppSettings.StatisticsToken) ? AppSettings.StatisticsToken : AppSettings.TestStatisticsToken;
                string downloadUsageUrl = !string.IsNullOrWhiteSpace(AppSettings.NedlatingsApiDownloadUsage) ? AppSettings.NedlatingsApiDownloadUsage : AppSettings.NedlatingsApiDownloadUsageDev;

                HttpClient hc = new HttpClient();
                hc.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var respons = hc.PostAsync(downloadUsageUrl, stringContent).Result;
            }
        }

        /// <summary>
        /// Move content from old config file to default config file. 
        /// </summary>
        public void ConvertDownloadToDefaultConfigFileIfExists()
        {
            var selecedForDownload = GetSelectedDatasetFilesFromDownloadFileAsDownloadModel();
            if (selecedForDownload.Any())
            {
                selecedForDownload = RemoveDuplicatesIterative(selecedForDownload);
                selecedForDownload = ConvertToNewVersionOfDownloadFile(selecedForDownload);

                if (_configFile.IsDefault())
                {
                    WriteToConfigFile(selecedForDownload);
                    File.Delete(ApplicationService.GetOldDownloadFilePath());
                }
            }
        }
    }
}