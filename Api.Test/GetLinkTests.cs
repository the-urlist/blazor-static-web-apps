using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using System.Text;
using Api.Functions;

namespace Api.Test
{
    public class GetLinkTests
    {
        [Theory]
        //TODO: See notes on SaveLinkTests - same issue here where GetLinks is creating a response from the request object.
        //[InlineData("test1", HttpStatusCode.OK)]
        [InlineData("notfound", HttpStatusCode.NotFound)]
        public async Task CanCallGetLinksFunction(string vanityUrl, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var dataService = new MockDataService();
            var body = new MemoryStream(Encoding.ASCII.GetBytes("{ \"test\": true }"));
            var context = Substitute.For<FunctionContext>();
            var req = new FakeHttpRequestData(context, new Uri("https://www.google.com"), body);
            var logger = Substitute.For<ILoggerFactory>();
            var getLinks = new GetLinks(logger, dataService);

            // Act
            var result = await getLinks.Run(req, vanityUrl);

            // Assert
            result.StatusCode.Should().Be(expectedStatusCode);
        }
    }
}