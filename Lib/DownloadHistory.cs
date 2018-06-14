using System;
using System.IO;

namespace Geonorge.MassivNedlasting
{

    public class DownloadHistory
    {

        public DownloadHistory(string url, string filePath)
        {
            Id = url;
            Downloaded = DateTime.Now.ToString();
            FilePath = filePath;
        }

        public string Id { get; set; }
        public string Downloaded { get; set; }
        public string FilePath { get; set; }
    }

}
