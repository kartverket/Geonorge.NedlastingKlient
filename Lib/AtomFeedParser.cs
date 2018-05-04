using System.Collections.Generic;
using System.Xml;

namespace Geonorge.MassivNedlasting
{
    public class AtomFeedParser
    {
        public List<Dataset> ParseDatasets(string xml)
        {
            var datasets = new List<Dataset>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string xpath = "//a:feed/a:entry";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("a", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("inspire_dls", "http://inspire.ec.europa.eu/schemas/inspire_dls/1.0");

            var nodes = xmlDoc.SelectNodes(xpath, nsmgr);

            if (nodes != null)
                foreach (XmlNode childrenNode in nodes)
                {
                    var dataset = new Dataset();
                    dataset.Title = childrenNode.SelectSingleNode("a:title", nsmgr).InnerXml;
                    dataset.Description = childrenNode.SelectSingleNode("a:content", nsmgr).InnerXml;
                    dataset.Url = childrenNode.SelectSingleNode("a:link", nsmgr).InnerXml;
                    dataset.LastUpdated = childrenNode.SelectSingleNode("a:updated", nsmgr).InnerXml;
                    dataset.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr).InnerXml;
                    dataset.Uuid = childrenNode.SelectSingleNode("inspire_dls:spatial_dataset_identifier_code", nsmgr)?.InnerXml;

                    datasets.Add(dataset);
                }
            return datasets;
        }

        internal DatasetFile ParseDatasetFile(string xml, DatasetFile originalDatasetFile)
        {
            var datasetFileFromFeed = new DatasetFile();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string xpath = "//a:feed/a:entry";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("a", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("inspire_dls", "http://inspire.ec.europa.eu/schemas/inspire_dls/1.0");

            var nodes = xmlDoc.SelectNodes(xpath, nsmgr);

            foreach (XmlNode childrenNode in nodes)
            {
                string title = childrenNode.SelectSingleNode("a:title", nsmgr).InnerXml;
                string projection = GetProjection(childrenNode.SelectNodes("a:category", nsmgr));

                if (originalDatasetFile.Title == title && originalDatasetFile.Projection == projection)
                {
                    datasetFileFromFeed.Title = title;
                    datasetFileFromFeed.Description = childrenNode.SelectSingleNode("a:category", nsmgr).InnerXml;
                    datasetFileFromFeed.Url = childrenNode.SelectSingleNode("a:link", nsmgr).Attributes[1].Value;
                    datasetFileFromFeed.LastUpdated = childrenNode.SelectSingleNode("a:updated", nsmgr).InnerXml;
                    datasetFileFromFeed.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr).InnerXml;
                    datasetFileFromFeed.Projection = projection;
                    datasetFileFromFeed.Restrictions = GetRestrictions(childrenNode.SelectNodes("a:category", nsmgr));
                    datasetFileFromFeed.DatasetId = originalDatasetFile.DatasetId;
                    datasetFileFromFeed.DatasetUrl = originalDatasetFile.DatasetUrl;
                }
            }
            return datasetFileFromFeed;
        }

        private string GetRestrictions(XmlNodeList selectNodes)
        {
            foreach (XmlNode node in selectNodes)
            {
                if (node.Attributes["scheme"]?.Value == "https://register.geonorge.no/subregister/metadata-kodelister/kartverket/tilgangsrestriksjoner/")
                {
                    return node.Attributes["term"].Value;
                }
            }
            return null;
        }

        private string GetProjection(XmlNodeList xmlNodeList)
        {
            foreach (XmlNode node in xmlNodeList)
            {
                if (node.Attributes["scheme"].Value == "http://www.opengis.net/def/crs/")
                {
                    return node.Attributes["term"].Value;
                }
            }
            return null;
        }

        public List<DatasetFile> ParseDatasetFiles(string xml, Dataset dataset)
        {
            var datasetFiles = new List<DatasetFile>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string xpath = "//a:feed/a:entry";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("a", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("inspire_dls", "http://inspire.ec.europa.eu/schemas/inspire_dls/1.0");

            var nodes = xmlDoc.SelectNodes(xpath, nsmgr);

            foreach (XmlNode childrenNode in nodes)
            {
                var datasetFile = new DatasetFile();
                datasetFile.Title = childrenNode.SelectSingleNode("a:title", nsmgr).InnerXml;
                datasetFile.Description = childrenNode.SelectSingleNode("a:category", nsmgr).InnerXml;
                datasetFile.Url = childrenNode.SelectSingleNode("a:link", nsmgr).Attributes[1].Value;
                //datasetFile.LastUpdated = childrenNode.SelectSingleNode("a:updated", nsmgr).InnerXml;
                datasetFile.LastUpdated = null;
                datasetFile.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr).InnerXml;
                datasetFile.Projection = GetProjection(childrenNode.SelectNodes("a:category", nsmgr));
                datasetFile.Restrictions = GetRestrictions(childrenNode.SelectNodes("a:category", nsmgr));
                datasetFile.DatasetId = dataset.Title;
                datasetFile.DatasetUrl = dataset.Url;

                datasetFiles.Add(datasetFile);
            }
            return datasetFiles;
        }

        public List<Dataset> ParseDataset(string xml)
        {
            throw new System.NotImplementedException();
        }
    }
}
