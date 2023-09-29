using Api;
using BlazorApp.Shared;
using AutoFixture.Xunit2;
using Microsoft.Azure.Functions.Worker.Http;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using FluentAssertions;
using NSubstitute.ReturnsExtensions;
using NSubstitute.ExceptionExtensions;
using Xunit;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using ApiIsolated;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos.Linq;
using System.Text;

namespace Api.Test
{
    public class GetLinkTests
    {
        [Fact]
        public async Task CanCallGetLinksFunction()
        {
            // Arrange
            var cosmosClient = Substitute.For<CosmosClient>();
            var body = new MemoryStream(Encoding.ASCII.GetBytes("{ \"test\": true }"));
            var context = Substitute.For<FunctionContext>();
            var req = Substitute.For<HttpRequestData>(context);
            var logger = Substitute.For<ILoggerFactory>();
            var getLinks = new GetLinks(logger, cosmosClient);

            // Act
            var res = await getLinks.Run(req, "vanityUrl");

            // Assert
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
