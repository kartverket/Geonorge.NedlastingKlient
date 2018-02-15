using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace NedlastingKlient
{
    public class DatasetService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public List<Dataset> GetDatasets()
        {
            var getFeedTask = HttpClient.GetStringAsync("https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml");
            return new AtomFeedParser().Parse(getFeedTask.Result);
        }

        public List<Dataset> GetDatasetFiles(string selectedDatasetUrl)
        {
            var getFeedTask = HttpClient.GetStringAsync(selectedDatasetUrl);
            return new AtomFeedParser().Parse(getFeedTask.Result);
        }
    }
}