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
        public const string StatisticsToken = "";
        public const string TestStatisticsToken = "tu7Szvs2Lej8yVXtiu3IVogke3TaN5GmSmNmLuSZDTvYtSYzrSG9VUgW9LE4XHiRfrbZmEqN42WwP7uLzfUAhSZnzR3ZBiF9JvI7VHwEyz7vaUdaa5BAxpDUqx2QDUu8";

        public const string NedlatingsApiDownloadUsage = "";
        public const string NedlatingsApiDownloadUsageDev = "https://nedlasting.dev.geonorge.no/api/internal/download-usage";
        /// <summary>
        /// Where the downloaded files will be written.
        /// </summary>

        public string Username { get; set; }
        public string Password { get; set; }

        public ConfigFile LastOpendConfigFile { get; set; }
        public ConfigFile TempConfigFile { get; set; }
        public List<ConfigFile> ConfigFiles { get; set; }

        public AppSettings()
        {
            ConfigFiles = new List<ConfigFile>();
        }

        public bool LastOpendConfigFileIsSet()
        {
            return LastOpendConfigFile != null;
        }

        public ConfigFile GetConfigByName(string name)
        {
            foreach (var configFile in ConfigFiles)
            {
                if (configFile.Name == name)
                {
                    return configFile;
                }
            }
            return null;
        }
    }


}