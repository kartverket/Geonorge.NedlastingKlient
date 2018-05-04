using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Geonorge.MassivNedlasting;
using Xunit;

namespace Geonorge.MassivNedlasting.Test
{
    public class DatasetServiceTest
    {

        [Fact]
        public void ShouldFetchDatasetFromGeonorge()
        {
            List<Dataset> datasets = new DatasetService().GetDatasets();
            datasets.Count.Should().BeGreaterThan(1);
        }

    }
}
