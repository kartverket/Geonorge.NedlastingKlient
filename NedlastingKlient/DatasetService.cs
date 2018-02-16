using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Formatting = System.Xml.Formatting;


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
            return new AtomFeedParser().ParseDatasetFile(getFeedTask.Result, dataset).OrderBy(d => d.Title).ToList();
        }

        public void WriteToJason(List<DatasetFile> selectedFiles)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            using (StreamWriter outputFile = new StreamWriter(mydocpath + @"\downloadfiles.txt", false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, selectedFiles);
                writer.Close();
            }
        }

        public List<DatasetFile> GetSelectedFiles(string datasetId = null)
        {
            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                using (StreamReader r = new StreamReader(mydocpath + @"\downloadfiles.txt"))
                {
                    string json = r.ReadToEnd();
                    List<DatasetFile> selecedFiles = JsonConvert.DeserializeObject<List<DatasetFile>>(json);

                    return datasetId != null ? selecedFiles.Where(f => f.DatasetId == datasetId).ToList() : selecedFiles;
                }
            }
            catch (Exception e)
            {
                return new List<DatasetFile>();
            }
        }
    }
}