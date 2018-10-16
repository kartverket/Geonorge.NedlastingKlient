using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Geonorge.MassivNedlasting;
using Xunit;

namespace Geonorge.MassivNedlasting.Test
{
    public class AtomFeedParserTest
    {

        [Fact]
        public void ShouldParseAtomFeed()
        {
            List<Dataset> result = new AtomFeedParser().ParseDatasets(File.ReadAllText("Tjenestefeed.xml"));
            result.Count.Should().Be(3);
        }

        [Fact]
        public void ShouldParseDatasetFileAtomFeed()
        {
            var dataset = NewDataset();
            List<DatasetFile> result = new AtomFeedParser().ParseDatasetFiles(File.ReadAllText("AdministrativeEnheterFylker_AtomFeedGEOJSON.fmw.xml"), dataset.Title, dataset.Url);
            result.Count.Should().Be(10);
        }


        private Dataset NewDataset()
        {
            var dataset = new Dataset();
            dataset.Title = "Administrative enheter fylker GEOJSON-format";
            dataset.Description = "Dataset description";
            dataset.LastUpdated = "2018-01-10T13:47:55";
            dataset.Organization = "Kartverket";
            dataset.Url = "http://nedlasting.geonorge.no/fmedatastreaming/ATOM-feeds/AdministrativeEnheterFylker_AtomFeedGEOJSON.fmw";
            dataset.Uuid = "6093c8a8-fa80-11e6-bc64-92361f002671";

            return dataset;
        }
    }
}
