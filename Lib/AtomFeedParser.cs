using System;
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
            nsmgr.AddNamespace("gn", "http://geonorge.no/geonorge");

            var nodes = xmlDoc.SelectNodes(xpath, nsmgr);

            if (nodes != null)
                foreach (XmlNode childrenNode in nodes)
                {
                    var dataset = new Dataset();
                    var id = childrenNode.SelectSingleNode("a:id", nsmgr);
                    dataset.Title = childrenNode.SelectSingleNode("a:title", nsmgr).InnerXml;
                    var description = childrenNode.SelectSingleNode("a:content", nsmgr);
                        if(description != null)
                        dataset.Description = description.InnerXml;


                    XmlNode uriAlternate = null;
                    var alternate = childrenNode.SelectSingleNode("a:link[@rel='alternate']", nsmgr);
                    if(alternate != null)
                        uriAlternate = alternate.Attributes.GetNamedItem("href");


                    XmlNode url = childrenNode.SelectSingleNode("a:link", nsmgr);

                    if (uriAlternate != null)
                    {
                        dataset.Url = uriAlternate.Value;
                    }
                    
                    else if (!string.IsNullOrEmpty(url.InnerXml))
                        dataset.Url = url.InnerXml;
                    else
                    {
                        dataset.Url = id.InnerXml;
                    }
                    dataset.LastUpdated = childrenNode.SelectSingleNode("a:updated", nsmgr).InnerXml;

                    var organizationGN = childrenNode.SelectSingleNode("a:author/gn:organisation", nsmgr);

                    if (organizationGN != null)
                        dataset.Organization = organizationGN.InnerXml;
                    else
                    dataset.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr).InnerXml;

                    if (string.IsNullOrEmpty(dataset.Organization) && dataset.Url.Contains("ngu.no"))
                        dataset.Organization = "Norges geologiske undersøkelse";
                    else if (string.IsNullOrEmpty(dataset.Organization) && dataset.Url.Contains("nibio.no"))
                        dataset.Organization = "Norsk institutt for bioøkonomi";
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
                    datasetFileFromFeed.Description = GetDescription(childrenNode, nsmgr);
                    datasetFileFromFeed.Url = GetUrl(childrenNode, nsmgr);
                    datasetFileFromFeed.LastUpdated = GetLastUpdated(childrenNode, nsmgr);
                    datasetFileFromFeed.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr)?.InnerXml;
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
                if (node.Attributes["scheme"].Value == "http://www.opengis.net/def/crs/" ||
                    node.Attributes["scheme"].Value == "https://register.geonorge.no/api/epsg-koder.xml")
                    {
                    if (!string.IsNullOrEmpty(node.Attributes["term"]?.Value))
                    {
                        var term = node.Attributes["term"]?.Value;
                        if (!string.IsNullOrEmpty(term) && term.StartsWith("EPSG:"))
                            return node.Attributes["term"].Value;
                    }
                    if (!string.IsNullOrEmpty(node.Attributes["label"]?.Value)) {
                        var label = node.Attributes["label"]?.Value;
                        if (!string.IsNullOrEmpty(label) && !label.StartsWith("EPSG/"))
                            return node.Attributes["label"].Value;
                    }
                    return node.Attributes["term"].Value;
                }
            }
            return null;
        }
        private string GetFormat(XmlNode xmlNode, XmlNodeList xmlNodeList)
        {
            foreach (XmlNode node in xmlNodeList)
            {
                if (node.Attributes["scheme"] != null && node.Attributes["scheme"].Value == "https://register.geonorge.no/api/metadata-kodelister/vektorformater.xml")
                {
                    return node.Attributes["term"].Value;
                }
            }

            if(xmlNode != null)
            { 
                var format = xmlNode.InnerText;
                if (format.Contains(","))
                    return format.Split(',')[0];
            }

            return null;
        }

        public List<DatasetFile> ParseDatasetFiles(string xml, string datasetTitle, string datasetUrl)
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
                datasetFile.Description = GetDescription(childrenNode, nsmgr);
                datasetFile.Url = GetUrl(childrenNode, nsmgr); 
                datasetFile.LastUpdated = GetLastUpdated(childrenNode, nsmgr);
                datasetFile.Organization = childrenNode.SelectSingleNode("a:author/a:name", nsmgr)?.InnerXml;
                datasetFile.Projection = GetProjection(childrenNode.SelectNodes("a:category", nsmgr));
                datasetFile.Format = GetFormat(childrenNode.SelectSingleNode("a:title", nsmgr), childrenNode.SelectNodes("a:category", nsmgr));
                datasetFile.Restrictions = GetRestrictions(childrenNode.SelectNodes("a:category", nsmgr));
                datasetFile.DatasetId = datasetTitle;
                datasetFile.DatasetUrl = datasetUrl;

                datasetFiles.Add(datasetFile);
            }
            return datasetFiles;
        }

        private string GetUrl(XmlNode xmlNode, XmlNamespaceManager nsmgr)
        {
            string url = "";
            var urlNode = xmlNode.SelectSingleNode("a:link[@rel='describedby']", nsmgr);
            if(urlNode != null) { 
              var hrefNode = urlNode.Attributes?.GetNamedItem("href");
                if(hrefNode != null)
                {
                    url = hrefNode.Value;
                }

            }
            if (!string.IsNullOrEmpty(urlNode?.Value))
            {
                url = urlNode.Value;
            }

            if (string.IsNullOrEmpty(url))
                url = xmlNode.SelectSingleNode("a:link", nsmgr).Attributes[1]?.Value;

            return url;

        }

        private string GetLastUpdated(XmlNode xmlNode, XmlNamespaceManager nsmgr)
        {
            var lastUpdated = xmlNode.SelectSingleNode("a:updated", nsmgr)?.InnerXml;

            if (string.IsNullOrEmpty(lastUpdated))
            {
                var updated = xmlNode.SelectSingleNode("a:link[@rel='describedby']", nsmgr).Attributes.GetNamedItem("updated");
                if (updated != null)
                    lastUpdated = updated.InnerText;
            }

            if (lastUpdated == null)
                lastUpdated = "";

            return lastUpdated;
        }

        private string GetDescription(XmlNode xmlNode, XmlNamespaceManager nsmgr)
        {
            var description = xmlNode.SelectSingleNode("a:category", nsmgr)?.InnerXml;
            if (string.IsNullOrEmpty(description))
            {
                description = xmlNode.SelectSingleNode("a:content", nsmgr)?.InnerText;
            }

            if (description == null)
                description = "";

            return description;
        }
    }
}
