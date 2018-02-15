using System.Collections.Generic;
using System.Xml;

namespace NedlastingKlient
{
    public class AtomFeedParser
    {
        public List<Dataset> Parse(string xml)
        {
            var datasets = new List<Dataset>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string xpath = "//a:feed/a:entry";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("a", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("inspire_dls", "http://inspire.ec.europa.eu/schemas/inspire_dls/1.0");

            var nodes = xmlDoc.SelectNodes(xpath, nsmgr);

            foreach (XmlNode childrenNode in nodes)
            {
                var dataset = new Dataset();
                dataset.Title = childrenNode.SelectSingleNode("a:title", nsmgr).InnerXml;
                dataset.Description = childrenNode.SelectSingleNode("a:category", nsmgr).InnerXml;
                dataset.Url = childrenNode.SelectSingleNode("a:link", nsmgr).InnerXml;
                dataset.LastUpdated = childrenNode.SelectSingleNode("a:updated", nsmgr).InnerXml;
                dataset.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr).InnerXml;
                dataset.Uuid = childrenNode.SelectSingleNode("inspire_dls:spatial_dataset_identifier_code", nsmgr)?.InnerXml;

                datasets.Add(dataset);
            }
            return datasets;
        }

        public List<Dataset> ParseDataset(string xml)
        {
            throw new System.NotImplementedException();
        }
    }
}
