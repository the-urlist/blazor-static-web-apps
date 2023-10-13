using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using System.Text;
using Api.Functions;
using BlazorApp.Shared;
using System.Text.Json;
using NSubstitute.Extensions;

namespace Api.Test
{
    public class SaveLinkTests
    {
        //TODO: This test is failing because the SaveLinks function is not returning a response.
        // SaveLinks creates a response from the request object and can't be mocked.
        // We need to refactor SaveLinks to use a response object that can be mocked or
        // create a testable service.
        //[Fact]
        public async Task SaveLink_ValidLinkBundle_ReturnsOk()
        {
            // Arrange
            var linkBundle = new LinkBundle
            {
                Id = Guid.NewGuid().ToString(),
                Links = new List<Link>
                {
                    new Link
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = "https://www.example.com",
                        Description = "Example website"
                    }
                }
            };
            var dataService = new MockDataService();
            var body = new MemoryStream(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(linkBundle)));
            var context = Substitute.For<FunctionContext>();
            var req = Substitute.ForPartsOf<FakeHttpRequestData>(context, new Uri("https://www.google.com"), body);
            req.Configure().CreateResponse().Returns(new FakeHttpResponseData(context));
            var logger = Substitute.For<ILoggerFactory>();
            var saveLink = new SaveLinks(logger,dataService);

            // Act
            var result = await saveLink.Run(req, context);
            result.Body.Position = 0;

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}