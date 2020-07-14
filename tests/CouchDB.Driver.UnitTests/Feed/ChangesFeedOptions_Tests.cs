using System;
using System.Linq;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.Shared;
using Xunit;

namespace CouchDB.Driver.UnitTests.Feed
{
    public class ChangesFeedOptions_Tests
    {
        [Fact]
        public void ToQueryParameters_WhenDefault_Return_NoProperties()
        {
            // Arrange
            var options = new ChangesFeedOptions();

            // Act
            var parameters = OptionsHelper.ToQueryParameters(options);

            // Assert
            Assert.Empty(parameters);
        }

        [Fact]
        public void ToQueryParameters_WhenAllPropertyNotDefault_Return_AllProperties()
        {
            // Arrange
            var options = new ChangesFeedOptions
            {
                Conflicts = true,
                Descending = true,
                Filter = Guid.NewGuid().ToString(),
                Heartbeat = 1,
                IncludeDocs = true,
                Attachments = true,
                AttachEncodingInfo = true,
                Limit = 1,
                Since = Guid.NewGuid().ToString(),
                Style = ChangesFeedStyle.AllDocs,
                Timeout = 1,
                View = Guid.NewGuid().ToString(),
                SeqInterval = 1
            };

            // Act
            var parameters = OptionsHelper.ToQueryParameters(options);

            // Assert
            Assert.Equal(13, parameters.Count());
        }
    }
}
