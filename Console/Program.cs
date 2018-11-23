using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.MassivNedlasting;
using Serilog;

namespace Geonorge.Nedlaster
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("log-.txt",
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            Log.Information("Start Geonorge - nedlaster");
            Console.WriteLine("Geonorge - nedlaster");
            Console.WriteLine("--------------------");
            var appSettings = ApplicationService.GetAppSettings();

            if (args != null)
            {
                Log.Debug("Selected config file(s): ");

                foreach (var configName in args)
                {
                    Log.Debug(configName);
                }

                foreach (var configName in args)
                {
                    Log.Debug("Download from selected config-file: " + configName);
                    var config = appSettings.GetConfigByName(configName);
                    if (config != null)
                    {
                        DeleteOldLogs(config.LogDirectory);
                        StartDownloadAsync(config).Wait();
                    }
                    else
                    {
                        Log.Error("Could not find config file: " + configName);
                        Console.WriteLine("Error: Could not find config file: " + configName);
                    }
                }
            }
            else
            {
                Log.Debug("No config file is selected. Download from all config-files");
                foreach (var config in appSettings.ConfigFiles)
                {
                    StartDownloadAsync(config).Wait();
                }
            }
        }

        private static async Task StartDownloadAsync(ConfigFile config)
        {
            var appSettings = ApplicationService.GetAppSettings();
            var datasetService = new DatasetService(config);
            var updatedDatasetToDownload = new List<Download>();
            var downloadLog = new DownloadLog();
            var downloader = new FileDownloader();
            var datasetToDownload = datasetService.GetSelectedFilesToDownload();
            var downloadUsage = config.DownloadUsage;

            downloadLog.TotalDatasetsToDownload = datasetToDownload.Count;

            foreach (var localDataset in datasetToDownload)
            {
                var updatedDatasetFileToDownload = new List<DatasetFile>();

                if (localDataset.Subscribe)
                {
                    Log.Information("Subscribe to Dataset files");
                    var datasetFilesFromFeed = datasetService.GetDatasetFiles(localDataset);

                    if (localDataset.AutoDeleteFiles)
                    {
                        Log.Debug("Delete files");
                        localDataset.Files = RemoveFiles(datasetFilesFromFeed, localDataset.Files, config);
                    }

                    if (localDataset.AutoAddFiles)
                    {
                        Log.Debug("Add new files");
                        localDataset.Files = AddFiles(datasetFilesFromFeed, localDataset.Files);
                    }
                }

                foreach (var datasetFile in localDataset.Files)
                {
                    var fileLog = new DatasetFileLog(datasetFile);

                    try
                    {
                        Console.WriteLine(datasetFile.DatasetId + " - " + datasetFile.Title);

                        DirectoryInfo downloadDirectory = GetDownloadDirectory(config, datasetFile);
                        DatasetFile datasetFromFeed = datasetService.GetDatasetFile(datasetFile);
                        DownloadHistory downloadHistory = datasetService.GetFileDownloaHistory(datasetFile.Url);
                        bool newDatasetAvailable = NewDatasetAvailable(downloadHistory, datasetFromFeed, downloadDirectory);

                        if (newDatasetAvailable)
                        {
                            Console.WriteLine("Updated version of dataset is available.");
                            Console.WriteLine("Starting download process.");
                            downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                            {
                                Console.CursorLeft = 0;
                                Console.Write($"{progressPercentage}% ({HumanReadableBytes(totalBytesDownloaded)}/{HumanReadableBytes(totalFileSize.Value)})                "); // add som extra whitespace to blank out previous updates
                            };

                            var downloadRequest = new DownloadRequest(datasetFile.Url, downloadDirectory, datasetFile.IsRestricted());
                            datasetFile.FilePath = await downloader.StartDownload(downloadRequest, appSettings);

                            downloadLog.Updated.Add(fileLog);

                            Console.WriteLine();

                            downloadUsage?.Entries.Add(new DownloadUsageEntries(datasetFile));
                            updatedDatasetFileToDownload.Add(datasetFile);

                        }
                        else
                        {
                            fileLog.Message = "Not necessary to download dataset." + datasetFromFeed.LastUpdated;
                            downloadLog.NotUpdated.Add(fileLog);
                            Console.WriteLine("Not necessary to download dataset.");
                            datasetFile.FilePath = downloadHistory.FilePath;
                            updatedDatasetFileToDownload.Add(datasetFile);
                        }
                    }

                    catch (Exception e)
                    {
                        Log.Error(e, "Error while downloading file " + datasetFile.Title);
                        updatedDatasetFileToDownload.Add(datasetFile);
                        fileLog.Message = "Error while downloading dataset: " + e.Message;
                        downloadLog.Faild.Add(fileLog);
                        Console.WriteLine("Error while downloading dataset: " + e.Message);
                    }

                    Console.WriteLine("-------------");
                }
                updatedDatasetToDownload.Add(localDataset);
            }

            Log.Information("Send download usage");
            datasetService.SendDownloadUsage(downloadUsage);
            Log.Information("Write to config file");
            datasetService.WriteToConfigFile(updatedDatasetToDownload);
            Log.Information("Write to download history file");
            datasetService.WriteToDownloadHistoryFile(updatedDatasetToDownload);
            Log.Information("Write to download log file");
            datasetService.WriteToDownloadLogFile(downloadLog);
        }


        private static List<DatasetFile> AddFiles(List<DatasetFile> datasetFilesFromFeed, List<DatasetFile> localDatasetFiles)
        {
            var datasetFiles = localDatasetFiles.ToList();
            foreach (var datasetFileFromFeed in datasetFilesFromFeed)
            {
                var datasetFile = datasetFiles.Find(d => datasetFileFromFeed.Title == d.Title && datasetFileFromFeed.DatasetId == d.DatasetId && datasetFileFromFeed.Projection == d.Projection);
                if (datasetFile == null)
                {
                    Log.Debug(datasetFileFromFeed.Title);
                    localDatasetFiles.Add(datasetFileFromFeed);
                }
            }
            return localDatasetFiles;
        }

        private static List<DatasetFile> RemoveFiles(List<DatasetFile> datasetFilesFromFeed, List<DatasetFile> datasetFiles, ConfigFile configFile)
        {
            var exists = false;
            var removeFiles = new List<DatasetFile>();

            foreach (var file in datasetFiles)
            {
                if (datasetFilesFromFeed.Any(fileFromFeed => fileFromFeed.Title == file.Title && fileFromFeed.DatasetId == file.DatasetId && fileFromFeed.Projection == file.Projection))
                {
                    exists = true;
                }
                if (!exists)
                {
                    Log.Debug(file.Title);
                    removeFiles.Add(file);
                }
                exists = false;
            }
            foreach (var fileToRemove in removeFiles)
            {
                DirectoryInfo downloadDirectory = GetDownloadDirectory(configFile, fileToRemove);
                string filePath = downloadDirectory + "\\" + fileToRemove.FilePath;

                File.Delete(filePath);
                datasetFiles.Remove(fileToRemove);
            }

            return datasetFiles;
        }

        private static string HumanReadableBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }


        private static DirectoryInfo GetDownloadDirectory(ConfigFile configFile, DatasetFile dataset)
        {
            var downloadDirectory = new DirectoryInfo(Path.Combine(configFile.DownloadDirectory, dataset.DatasetId));
            if (!downloadDirectory.Exists)
            {
                Console.WriteLine($"Creating directory: {downloadDirectory}");
                downloadDirectory.Create();
            }
            return downloadDirectory;
        }

        private static bool NewDatasetAvailable(DownloadHistory downloadHistory, DatasetFile datasetFromFeed, DirectoryInfo downloadDirectory)
        {
            if (downloadHistory == null) return true;
            if (!LocalFileExists(downloadHistory, downloadDirectory, datasetFromFeed)) return true;

            var originalDatasetLastUpdated = DateTime.Parse(downloadHistory.Downloaded);
            var datasetFromFeedLastUpdated = DateTime.Parse(datasetFromFeed.LastUpdated);

            var updatedDatasetAvailable = originalDatasetLastUpdated < datasetFromFeedLastUpdated;
            return updatedDatasetAvailable;
        }

        private static bool LocalFileExists(DownloadHistory downloadHistory, DirectoryInfo downloadDirectory, DatasetFile dataset)
        {
            if (downloadHistory.FilePath != null)
            {
                var filePath = new FileInfo(Path.Combine(downloadDirectory.FullName, downloadHistory.FilePath));
                return filePath.Exists;
            }
            else
            {
                return LocalFileExists(downloadDirectory, dataset);
            }
        }

        private static bool LocalFileExists(DirectoryInfo downloadDirectory, DatasetFile dataset)
        {
            if (!dataset.HasLocalFileName())
                return false;

            var filePath = new FileInfo(Path.Combine(downloadDirectory.FullName, dataset.LocalFileName()));

            return filePath.Exists;
        }

        private static void DeleteOldLogs(string logDirectory)
        {
            try
            {
                string[] files = Directory.GetFiles(logDirectory);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddMonths(-1))
                        fi.Delete();
                }
            }
            catch (Exception e)
            {
                // logge?
            }
        }

    }
}