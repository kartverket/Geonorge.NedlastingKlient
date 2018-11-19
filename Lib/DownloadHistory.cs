using System;
using System.IO;

namespace Geonorge.MassivNedlasting
{
    /// <summary>
    /// History of downloaded files. 
    /// </summary>
    public class DownloadHistory
    {

        public DownloadHistory(string url, string filePath)
        {
            Id = url;
            Downloaded = DateTime.Now.ToString();
            FilePath = filePath;
        }

        /// <summary>
        /// Download url
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date of last run "Nedlaster". 
        /// </summary>
        public string Downloaded { get; set; }

        /// <summary>
        /// File path to check if local file exists
        /// </summary>
        public string FilePath { get; set; }
    }

}
