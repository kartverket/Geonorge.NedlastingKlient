using System;
using System.IO;

namespace NedlastingKlient.Konsoll
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Geonorge Nedlastingklient");

            var datasetService = new DatasetService();
            var datasetToDownload = datasetService.GetSelectedFiles();

            var appSettings = ApplicationService.GetAppSettings();

            var downloader = new FileDownloader();
            foreach (var dataset in datasetToDownload)
            {
                var downloadFilePath = GetDownloadFilePath(appSettings, dataset);

                if (downloadFilePath.Exists && !ShouldDownload(dataset))
                    continue;

                Console.WriteLine("-------------");
                Console.WriteLine(dataset.DatasetId + " - " + dataset.Title);

                downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    Console.CursorLeft = 0;
                    Console.Write($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                };

                downloader.StartDownload(dataset.Url, downloadFilePath.FullName).Wait();

                Console.WriteLine();
            }
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

        private static bool ShouldDownload(DatasetFile originalDataset)
        {
            var datasetService = new DatasetService();
            var datasetFromFeed = datasetService.GetDatasetFile(originalDataset);
            var originalDatasetLastUpdated = DateTime.Parse(originalDataset.LastUpdated);
            var datasetFromFeedLastUpdated = DateTime.Parse(datasetFromFeed.LastUpdated);

            return originalDatasetLastUpdated < datasetFromFeedLastUpdated;
        }
    }
}