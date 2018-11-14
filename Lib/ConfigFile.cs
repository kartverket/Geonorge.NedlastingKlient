using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geonorge.MassivNedlasting
{
    public class ConfigFile
    {
        public const string Default = "Default";

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string DownloadDirectory { get; set; }
        public string LogDirectory { get; set; }
        public DownloadUsage DownloadUsage { get; set; }


        public ConfigFile()
        {
            Id = Guid.NewGuid();
        }

        public static ConfigFile GetDefaultConfigFile()
        {
            return new ConfigFile()
            {
                Id = Guid.NewGuid(),
                Name = Default,
                FilePath = ApplicationService.GetDownloadFilePath(),
                DownloadDirectory = ApplicationService.GetDefaultDownloadDirectory(),
                LogDirectory = ApplicationService.GetDefaultLogDirectory(),
            };
        }

        public bool DownloadUsageIsSet()
        {
            return DownloadUsage != null && DownloadUsage.Group.Any() && DownloadUsage.Purpose != null;
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
