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

            var downloader = new FileDownloader();
            foreach (var dataset in datasetToDownload)
            {

                if (!ShouldDownload(dataset))
                    continue;

                Console.WriteLine("-------------");
                Console.WriteLine(dataset.DatasetId + " - " + dataset.Title);

                downloader.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    Console.CursorLeft = 0;
                    Console.Write($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                };

                var outputDirectory = new DirectoryInfo(@"c:\dev\tmp\geonorge\" + dataset.DatasetId);
                if (!outputDirectory.Exists)
                    outputDirectory.Create();

                var uri = new Uri(dataset.Url);
                var outputFile = outputDirectory.FullName + Path.DirectorySeparatorChar +
                                 Path.GetFileName(uri.LocalPath);

                downloader.StartDownload(dataset.Url, outputFile).Wait();

                Console.WriteLine();
            }
        }

        private static bool ShouldDownload(DatasetFile dataset)
        {
            // TODO: implement check of LastUpdated both in JSON-file and in Atom feed
            return true;
        }
    }
}