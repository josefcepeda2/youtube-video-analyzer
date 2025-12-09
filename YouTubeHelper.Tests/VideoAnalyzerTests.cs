using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace YouTubeHelper.Tests
{
    public class VideoAnalyzerTests
    {
        [Fact]
        public async Task AnalyzeVideoAsync_SuccessfulResponse_ReturnsContent()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ ""candidates"": [ { ""content"": { ""parts"": [ { ""text"": ""Test analysis"" } ] } } ] }"),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var analyzer = new VideoAnalyzer(httpClient, "test_api_key", "test_model");
            var transcript = "This is a test transcript.";

            // Act
            var result = await analyzer.AnalyzeVideoAsync(transcript);

            // Assert
            Assert.Equal("Test analysis", result);
        }
    }
}

