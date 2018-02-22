using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NedlastingKlient
{
    public class DatasetService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public List<Dataset> GetDatasets()
        {
            var getFeedTask = HttpClient.GetStringAsync("https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml");
            return new AtomFeedParser().ParseDatasets(getFeedTask.Result);
        }

        public List<DatasetFile> GetDatasetFiles(Dataset dataset)
        {
            var getFeedTask = HttpClient.GetStringAsync(dataset.Url);
            return new AtomFeedParser().ParseDatasetFiles(getFeedTask.Result, dataset).OrderBy(d => d.Title).ToList();
        }

        public DatasetFile GetDatasetFile(DatasetFile originalDatasetFile)
        {
            var getFeedTask = HttpClient.GetStringAsync(originalDatasetFile.DatasetUrl);
            return new AtomFeedParser().ParseDatasetFile(getFeedTask.Result, originalDatasetFile);
        }

        /// <summary>
        /// Writes the information about the selected files to the local download list. 
        /// </summary>
        /// <param name="datasetFiles"></param>
        public void WriteToDownloadFile(List<DatasetFile> datasetFiles)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(ApplicationService.GetDownloadFilePath(), false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, datasetFiles);
                writer.Close();
            }
        }

        /// <summary>
        /// Returns a list of dataset files to download. 
        /// </summary>
        /// <param name="datasetTitle">search for dataset with given title. List will only return dataset that matches.</param>
        /// <returns></returns>
        public List<DatasetFile> GetSelectedFiles(string datasetTitle = null)
        {
            try
            {
                using (var r = new StreamReader(ApplicationService.GetDownloadFilePath()))
                {
                    var json = r.ReadToEnd();
                    var selecedFiles = JsonConvert.DeserializeObject<List<DatasetFile>>(json);
                    r.Close();
                    return datasetTitle != null
                        ? selecedFiles.Where(f => f.DatasetId == datasetTitle).ToList()
                        : selecedFiles;
                }
            }
            catch (Exception)
            {
                // TODO error handling
                return new List<DatasetFile>();
            }
        }
    }
}