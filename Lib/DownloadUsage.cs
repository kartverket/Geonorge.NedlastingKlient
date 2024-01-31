using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Geonorge.MassivNedlasting
{
    public class DownloadUsage
    {
        public string Group { get; set; }
        public List<string> Purpose { get; set; }
        public string SoftwareClient => "Geonorge.MassivNedlasting";
        public string SoftwareClientVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();
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
        public string Format { get; set; }
        public string Projection { get; set; }

        public DownloadUsageEntries(DatasetFile datasetFile)
        {
            MetadataUuid = !string.IsNullOrEmpty(datasetFile.MetadataUuid) ? datasetFile.MetadataUuid : datasetFile.DatasetId;
            var title = datasetFile.Title.Split(',');
            if (title.Length == 3)
            {
                AreaCode = title[1].Trim();
                AreaName = title[2].Trim();
                Format = title[0].Replace("-format", "").Trim();
            }
            else if (title.Length == 2)
            {
                AreaName = title[1].Trim();
                Format = title[0].Replace("-format", "").Trim();
            }

            if(!string.IsNullOrEmpty(datasetFile.AreaCode))
                AreaCode = datasetFile.AreaCode;

            if (!string.IsNullOrEmpty(datasetFile.AreaLabel))
                AreaName = datasetFile.AreaLabel;

            if(!string.IsNullOrEmpty(datasetFile.Format))
                Format = datasetFile.Format.Replace("-format", "").Trim();

            Projection = datasetFile.Projection.Replace("EPSG:", "").Trim();
        }
    }

    public class PurposeViewModel
    {
        public string Purpose { get; set; }
        public bool IsSelected { get; set; }


        public PurposeViewModel(string item, List<string> selectedPurposes)
        {
            Purpose = item;
            IsSelected = false;
            if (!selectedPurposes.Any()) return;
            foreach (var selectedPurpose in selectedPurposes)
            {
                if (selectedPurpose == item)
                {
                    IsSelected = true;
                    break;
                }
            }
        }

    }
}
