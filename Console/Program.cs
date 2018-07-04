using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.MassivNedlasting;

namespace Geonorge.Nedlaster
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Geonorge - nedlaster");
            Console.WriteLine("--------------------");
            DeleteOldLogs();
            StartDownloadAsync().Wait();
        }

        private static void DeleteOldLogs()
        {
            string[] files = Directory.GetFiles(ApplicationService.GetLogAppDirectory().ToString());

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddMonths(-1))
                    fi.Delete();
            }
        }

        private static async Task StartDownloadAsync()
        {
            var datasetService = new DatasetService();
            List<DatasetFile> datasetToDownload = datasetService.GetSelectedFiles();

            List<DatasetFile> updatedDatasetToDownload = new List<DatasetFile>();
            DownloadLog downloadLog = new DownloadLog();
            downloadLog.TotalDatasetsToDownload = datasetToDownload.Count;
            var appSettings = ApplicationService.GetAppSettings();

            var downloader = new FileDownloader();
            foreach (var localDataset in datasetToDownload)
            {
                var fileLog = new DatasetFileLog(localDataset);

                try
                {
                    Console.WriteLine(localDataset.DatasetId + " - " + localDataset.Title);

                    DirectoryInfo downloadDirectory = GetDownloadDirectory(appSettings, localDataset);

                    DatasetFile datasetFromFeed = datasetService.GetDatasetFile(localDataset);

                    DownloadHistory downloadHistory = datasetService.GetFileDownloaHistory(datasetFromFeed.Url);

                    bool newDatasetAvailable = NewDatasetAvailable(downloadHistory, datasetFromFeed, downloadDirectory);
                    if (newDatasetAvailable)
                        Console.WriteLine("Updated version of dataset is available.");

                    if (newDatasetAvailable)
                    {
                        Console.WriteLine("Starting download process.");
                        downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                        {
                            Console.CursorLeft = 0;
                            Console.Write($"{progressPercentage}% ({HumanReadableBytes(totalBytesDownloaded)}/{HumanReadableBytes(totalFileSize.Value)})                "); // add som extra whitespace to blank out previous updates
                        };


                        var downloadRequest = new DownloadRequest(localDataset.Url, downloadDirectory, localDataset.IsRestricted());
                        localDataset.FilePath = await downloader.StartDownload(downloadRequest, appSettings);

                        if (localDataset.FilePath == null)
                        {
                            fileLog.Message = "Download failed: You need to authorize access before downloading file";
                            downloadLog.Faild.Add(fileLog);
                        }
                        else
                        {
                            downloadLog.Updated.Add(fileLog);
                        }
                        Console.WriteLine();

                        updatedDatasetToDownload.Add(localDataset);

                    }
                    else
                    {
                        fileLog.Message = "Not necessary to download dataset.";
                        downloadLog.NotUpdated.Add(fileLog);
                        Console.WriteLine("Not necessary to download dataset.");
                        localDataset.FilePath = downloadHistory.FilePath;
                        updatedDatasetToDownload.Add(localDataset);
                    }
                }
                catch (Exception e)
                {
                    updatedDatasetToDownload.Add(localDataset);
                    fileLog.Message = "Error while downloading dataset: " + e.Message;
                    downloadLog.Faild.Add(fileLog);
                    Console.WriteLine("Error while downloading dataset: " + e.Message);
                }

                Console.WriteLine("-------------");
            }

            datasetService.WriteToDownloadFile(updatedDatasetToDownload);
            datasetService.WriteToDownloadHistoryFile(updatedDatasetToDownload);
            datasetService.WriteToDownloadLogFile(downloadLog);
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

        private static bool IsRunningAsBackgroundTask(string[] args)
        {
            return args != null && args.Any() && args.First() == "-background";
        }

        private static DirectoryInfo GetDownloadDirectory(AppSettings appSettings, DatasetFile dataset)
        {
            var downloadDirectory = new DirectoryInfo(Path.Combine(appSettings.DownloadDirectory, dataset.DatasetId));
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

    }
}