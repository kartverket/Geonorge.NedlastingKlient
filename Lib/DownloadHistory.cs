using System;
using System.IO;

namespace Geonorge.MassivNedlasting
{

    public class DownloadHistory
    {

        public DownloadHistory(string url)
        {
            Id = url;
            Downloaded = DateTime.Now.ToString();
        }

        public string Id { get; set; }
        public string Downloaded { get; set; }
    }

}
