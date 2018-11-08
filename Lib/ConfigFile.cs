using System;
using System.Collections.Generic;
using System.Text;

namespace Geonorge.MassivNedlasting
{
    public class ConfigFile
    {
        public const string Default = "Default";

        public string Name { get; set; }
        public string FilePath { get; set; }
        public string DownloadDirectory { get; set; }
        public string LogDirectory { get; set; }


        public static ConfigFile GetDefaultConfigFile()
        {
            return new ConfigFile()
            {
                Name = Default,
                FilePath = ApplicationService.GetDownloadFilePath(),
                DownloadDirectory = ApplicationService.GetDefaultDownloadDirectory(),
                LogDirectory = ApplicationService.GetDefaultDownloadDirectory(),
            };
        }
    }

    public class ConfigFileViewModel
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string DownloadDirectory { get; set; }

        public static ConfigFile GetDefaultConfigFile()
        {
            return new ConfigFile()
            {
                Name = "Default",
                FilePath = ApplicationService.GetDownloadFilePath(),
                DownloadDirectory = ApplicationService.GetDefaultDownloadDirectory(),
            };
        }
    }
}
