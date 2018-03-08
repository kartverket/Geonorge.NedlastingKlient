using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NedlastingKlient.Konsoll
{
    /// <summary>
    /// Download file with progress event
    /// 
    /// https://stackoverflow.com/a/43169927/725492
    /// </summary>
    public class FileDownloader
    {
        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded,
            double? progressPercentage);

        private static readonly HttpClient Client = new HttpClient();

        public event ProgressChangedHandler ProgressChanged;

        public async Task StartDownload(string downloadUrl, string destinationFilePath, AppSettings appSettings, bool isRestricted)
        {
            HttpClient client = SetClientRequestHeaders(appSettings, isRestricted);

            using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                await DownloadFileFromHttpResponseMessage(response, destinationFilePath);
            }
        }

        private static HttpClient SetClientRequestHeaders(AppSettings appSettings, bool isRestricted)
        {
           // ... Use HttpClient.            
            HttpClient client = new HttpClient();

            if (isRestricted)
            {
                var byteArray = Encoding.ASCII.GetBytes(appSettings.Username + ":" + ProtectionService.GetUnprotectedPassword(appSettings.Password));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            return client;
        }

        private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response, string destinationFilePath)
        {
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Nedlasting av fil krever brukernavn og passord");
            }
            else
            {
                var totalBytes = response.Content.Headers.ContentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    await ProcessContentStream(totalBytes, contentStream, destinationFilePath);
                }
            }
        }

        private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream, string destinationFilePath)
        {
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write,
                FileShare.None, 8192, true))
            {
                do
                {
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    if (readCount % 100 == 0)
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                } while (isMoreToRead);
            }
        }

        private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
        {
            if (ProgressChanged == null)
                return;

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double) totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }
    }
}