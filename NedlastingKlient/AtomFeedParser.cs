using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NedlastingKlient
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

        public List<DatasetFile> ParseDatasetFile(string xml, Dataset dataset)
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
                datasetFile.Url = childrenNode.SelectSingleNode("a:link", nsmgr).InnerXml;
                datasetFile.Url = childrenNode.SelectSingleNode("a:link", nsmgr).Attributes[1].Value;
                datasetFile.LastUpdated = childrenNode.SelectSingleNode("a:updated", nsmgr).InnerXml;
                datasetFile.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr).InnerXml;
                datasetFile.Category = childrenNode.SelectSingleNode("a:category", nsmgr).Attributes[0].Value;
                datasetFile.DatasetId = dataset.Title;

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
