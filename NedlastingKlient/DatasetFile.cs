using System;

namespace NedlastingKlient
{

    public class DatasetFile
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LastUpdated { get; set; }
        public string Organization { get; set; }
        public string Category { get; set; }
        public string DatasetId { get; set; }

        public string GetId()
        {
            return Title + "_" + Category;
        }
    }
}
