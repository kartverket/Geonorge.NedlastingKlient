using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace NedlastingKlient.Test
{
    public class AtomFeedParserTest
    {

        [Fact]
        public void ShouldParseAtomFeed()
        {
            List<Dataset> result = new AtomFeedParser().Parse(File.ReadAllText("Tjenestefeed.xml"));
            result.Count.Should().Be(3);
        }

    }
}
