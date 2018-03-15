using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace NedlastingKlient.Konsoll
{
    public class DownloadRequest
    {
        public string DownloadUrl { get; }

        public DirectoryInfo DestinationDirectory { get; }

        public bool MustAuthenticate { get; }

        public DownloadRequest(string downloadUrl, DirectoryInfo destinationDirectory, bool mustAuthenticate)
        {
            DownloadUrl = downloadUrl;
            DestinationDirectory = destinationDirectory;
            MustAuthenticate = mustAuthenticate;
        }

        public string GetDestinationFileName(HttpResponseMessage response)
        {
            var contentDisposition = response.Content.Headers.ContentDisposition;
            if (contentDisposition != null)
            {
                return Path.Combine(DestinationDirectory.FullName, contentDisposition.FileName.Replace("\"", ""));
            }

            var filenameFromUrl = new Uri(DownloadUrl).LocalPath;
            return Path.Combine(DestinationDirectory.FullName, Path.GetFileName(filenameFromUrl));
        }
    }
}
