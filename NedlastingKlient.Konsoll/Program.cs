using System;
using System.Globalization;
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

            AppSettings appSettings = ApplicationService.GetAppSettings();

            var downloader = new FileDownloader();
            foreach (var dataset in datasetToDownload)
            {

                var outputDirectory = new DirectoryInfo(Path.Combine(appSettings.DownloadDirectory, dataset.DatasetId));
                if (!outputDirectory.Exists)
                {
                    Console.WriteLine($"Download directory [{outputDirectory}] does not exist, creating it now.");
                    outputDirectory.Create();
                }

                var outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, Path.GetFileName(new Uri(dataset.Url).LocalPath)));

                if (outputFile.Exists && !ShouldDownload(dataset))
                    continue;

                Console.WriteLine("-------------");
                Console.WriteLine(dataset.DatasetId + " - " + dataset.Title);

                downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    Console.CursorLeft = 0;
                    Console.Write($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                };

                downloader.StartDownload(dataset.Url, outputFile.FullName).Wait();

                Console.WriteLine();
            }
        }

        private static bool ShouldDownload(DatasetFile originalDataset)
        {
            var datasetService = new DatasetService();
            DatasetFile datasetFromFeed = datasetService.GetDatasetFile(originalDataset);
            DateTime originalDatasetLastUpdated = DateTime.Parse(originalDataset.LastUpdated);
            DateTime datasetFromFeedLastUpdated = DateTime.Parse(datasetFromFeed.LastUpdated);

            return originalDatasetLastUpdated < datasetFromFeedLastUpdated;
        }
    }
}