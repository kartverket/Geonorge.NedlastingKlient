using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Formatting = System.Xml.Formatting;


namespace NedlastingKlient
{
    public class DatasetService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public List<Dataset> GetDatasets(string url = "https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml")
        {
            var getFeedTask = HttpClient.GetStringAsync(url);
            return new AtomFeedParser().Parse(getFeedTask.Result);
        }

        public void WriteToJason(List<Dataset> selectedFiles)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            using (StreamWriter outputFile = new StreamWriter(mydocpath + @"\downloadfiles.txt", false))
            using (JsonWriter writer = new JsonTextWriter(outputFile))
            {
                serializer.Serialize(writer, selectedFiles);
            }
        }

        public List<Dataset> GetSelectedFiles()
        {
            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                using (StreamReader r = new StreamReader(mydocpath + @"\downloadfiles.txt"))
                {
                    string json = r.ReadToEnd();
                    List<Dataset> selecedFiles = JsonConvert.DeserializeObject<List<Dataset>>(json);
                    return selecedFiles;
                }
            }
            catch (Exception e)
            {
                return new List<Dataset>();
            }
        }
    }
}