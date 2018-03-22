using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Geonorge.MassivNedlasting;

namespace Geonorge.Nedlaster
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Geonorge - nedlaster");

            var datasetService = new DatasetService();
            List<DatasetFile> datasetToDownload = datasetService.GetSelectedFiles();

            List<DatasetFile> updatedDatasetToDownload = new List<DatasetFile>();

            var appSettings = ApplicationService.GetAppSettings();

            var downloader = new FileDownloader();
            foreach (var localDataset in datasetToDownload)
            {
                try
                {
                    DirectoryInfo downloadDirectory = GetDownloadDirectory(appSettings, localDataset);

                    DatasetFile datasetFromFeed = datasetService.GetDatasetFile(localDataset);

                    
                    Console.WriteLine(localDataset.DatasetId + " - " + localDataset.Title);

                    bool newDatasetAvailable = NewDatasetAvailable(localDataset, datasetFromFeed);
                    if (newDatasetAvailable)
                        Console.WriteLine("Updated version of dataset is available.");

                    bool localFileExists = LocalFileExists(downloadDirectory, localDataset);
                    if (localFileExists)
                        Console.WriteLine("Local copy of file already exists.");

                    if (newDatasetAvailable || !localFileExists)
                    {
                        Console.WriteLine("Starting download process.");
                        downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                        {
                            Console.CursorLeft = 0;
                            /*
                            Console.Write(new string(' ', Console.WindowWidth)); 
                            Console.CursorLeft = 0;*/
                            Console.Write($"{progressPercentage}% ({HumanReadableBytes(totalBytesDownloaded)}/{HumanReadableBytes(totalFileSize.Value)})");
                        };

                        var downloadRequest = new DownloadRequest(localDataset.Url, downloadDirectory, localDataset.IsRestricted());
                        downloader.StartDownload(downloadRequest, appSettings).Wait();
                        Console.WriteLine();
                        Console.WriteLine("-------------");

                        updatedDatasetToDownload.Add(datasetFromFeed);
                    }
                    else
                    {
                        Console.WriteLine("Not necessary to download dataset.");
                        updatedDatasetToDownload.Add(localDataset);
                    }
                }
                catch (Exception e)
                {
                    updatedDatasetToDownload.Add(localDataset);
                    Console.WriteLine("Error while downloading dataset: " + e.Message);
                }
            }

            datasetService.WriteToDownloadFile(updatedDatasetToDownload);

            if (!IsRunningAsBackgroundTask(args))
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static bool LocalFileExists(DirectoryInfo downloadDirectory, DatasetFile dataset)
        {
            if (!dataset.HasLocalFileName())
                return false;

            var filePath = new FileInfo(Path.Combine(downloadDirectory.FullName, dataset.LocalFileName()));

            return filePath.Exists;
        }


        private static string HumanReadableBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len = len/1024;
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
                Console.WriteLine($"Download directory [{downloadDirectory}] does not exist, creating it now.");
                downloadDirectory.Create();
            }
            return downloadDirectory;
        }

        private static bool NewDatasetAvailable(DatasetFile localDataset, DatasetFile datasetFromFeed)
        {
            var originalDatasetLastUpdated = DateTime.Parse(localDataset.LastUpdated);
            var datasetFromFeedLastUpdated = DateTime.Parse(datasetFromFeed.LastUpdated);

            Console.WriteLine("local file last updated:" + originalDatasetLastUpdated);
            Console.WriteLine("atom feed last updated:" + datasetFromFeedLastUpdated);

            var updatedDatasetAvailable = originalDatasetLastUpdated < datasetFromFeedLastUpdated;
            Console.WriteLine("Updated dataset available: " + updatedDatasetAvailable);
            return updatedDatasetAvailable;
        }
    }
}