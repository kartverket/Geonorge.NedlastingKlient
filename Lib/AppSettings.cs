using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool DownloadUsageIsSet()
        {
            return DownloadUsage != null && DownloadUsage.Group.Any() && DownloadUsage.Purpose != null;
        }
    }
}