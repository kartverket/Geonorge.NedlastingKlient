using Geonorge.MassivNedlasting;

namespace Geonorge.Nedlaster
{
    /// <summary>
    /// Log when dataset file is downloaded
    /// </summary>
    public class DatasetFileLog
    {
        private DatasetFile localDataset;

        public string DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string Name { get; set; }
        public string Projection { get; set; }
        public int NumberOfFilesUpdated { get; set; }
        public int TotalNumberOfFiles { get; set; }
        public string HumanReadableSize { get; set; }
        public string Message { get; set; }

        public DatasetFileLog(DatasetFile localDataset, Download dataset)
        {
            DatasetId = localDataset.DatasetId;
            Name = localDataset.Title;
            DatasetName = dataset.DatasetTitle;
            Projection = localDataset.Projection;
        }

        public DatasetFileLog()
        {
            DatasetId = localDataset.DatasetId;
        }

    }
}