using System;
using System.Collections.Generic;
using System.IO;

namespace NedlastingKlient.Konsoll
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Geonorge Nedlastingklient");

            var datasetService = new DatasetService();
            List<DatasetFile> datasetToDownload = datasetService.GetSelectedFiles();

            List<DatasetFile> UpdatedDatasetToDownload = new List<DatasetFile>();

            var appSettings = ApplicationService.GetAppSettings();

            var downloader = new FileDownloader();
            foreach (var localDataset in datasetToDownload)
            {
                try
                {
                    FileInfo downloadFilePath = GetDownloadFilePath(appSettings, localDataset);

                    DatasetFile datasetFromFeed = datasetService.GetDatasetFile(localDataset);

                    if (downloadFilePath.Exists && ShouldDownload(localDataset, datasetFromFeed))
                    {
                        Console.WriteLine("-------------");
                        Console.WriteLine(localDataset.DatasetId + " - " + localDataset.Title);

                        downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                        {
                            Console.CursorLeft = 0;
                            Console.Write($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                        };

                        downloader.StartDownload(localDataset.Url, downloadFilePath.FullName).Wait();

                        Console.WriteLine();
                        UpdatedDatasetToDownload.Add(datasetFromFeed);
                    }
                    else
                    {
                        UpdatedDatasetToDownload.Add(localDataset);
                    }
                }
                catch (Exception e)
                {
                    UpdatedDatasetToDownload.Add(localDataset);
                    Console.WriteLine("Error while downloading dataset: " + e.Message);
                }
            }

            datasetService.WriteToDownloadFile(datasetToDownload);
        }

        private static FileInfo GetDownloadFilePath(AppSettings appSettings, DatasetFile dataset)
        {
            var downloadDirectory = new DirectoryInfo(Path.Combine(appSettings.DownloadDirectory, dataset.DatasetId));
            if (!downloadDirectory.Exists)
            {
                Console.WriteLine($"Download directory [{downloadDirectory}] does not exist, creating it now.");
                downloadDirectory.Create();
            }

            var filenameFromUrl = new Uri(dataset.Url).LocalPath;
            var downloadFilePath =
                new FileInfo(Path.Combine(downloadDirectory.FullName, Path.GetFileName(filenameFromUrl)));
            return downloadFilePath;
        }

        private static bool ShouldDownload(DatasetFile localDataset, DatasetFile datasetFromFeed)
        {
            var originalDatasetLastUpdated = DateTime.Parse(localDataset.LastUpdated);
            var datasetFromFeedLastUpdated = DateTime.Parse(datasetFromFeed.LastUpdated);

            return originalDatasetLastUpdated < datasetFromFeedLastUpdated;
        }
    }
}