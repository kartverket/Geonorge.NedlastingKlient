using System;
using System.Collections.Generic;
using System.Reflection;

namespace Geonorge.MassivNedlasting
{
    /// <summary>
    /// Holds various application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Where the downloaded files will be written.
        /// </summary>
        public string DownloadDirectory { get; set; }
        public string LogDirectory { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }

        public DownloadUsage DownloadUsage { get; set; }

    }

    public class DownloadUsage
    {
        public string Group { get; set; }
        public List<string> Purpose { get; set; }
        public string SoftwareClient => "Geonorge.MassivNedlasting";
        public string SoftwareClientVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public List<DownloadUsageEntries> Entries { get; set; }

        public DownloadUsage()
        {
            Purpose = new List<string>();
            Entries = new List<DownloadUsageEntries>();
        }

    }

    public class DownloadUsageEntries
    {
        public string MetadataUuid { get; set; }
        public string AreaCode { get; set; }
        public string AreaName { get; set; }
        public string Format{ get; set; }
        public string Projection{ get; set; }

        public DownloadUsageEntries(DatasetFile datasetFile)
        {
            MetadataUuid = datasetFile.DatasetId; // TODO legg til metadata uuid i dataset file..
            var title = datasetFile.Title.Split(',');
            if (title.Length == 3)
            {
                AreaCode = title[1].Trim();
                AreaName = title[2];
                Format = title[0];
            }
            else if(title.Length == 2)
            {
                AreaName = title[1];
                Format = title[0];
            }
            else
            {
               
            }
            
            Projection = datasetFile.Projection;
        }
    }
}