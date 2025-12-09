using System;
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
            var analyzer = new VideoAnalyzer(httpClient, "test_api_key", "test_model", 3, TimeSpan.FromSeconds(1));
            var transcript = "This is a test transcript.";
            var videoUrl = "https://www.youtube.com/watch?v=test";

            // Act
            var result = await analyzer.AnalyzeVideoAsync(transcript, videoUrl);

            // Assert
            Assert.Equal("Test analysis", result);
        }

        [Fact]
        public async Task AnalyzeVideoAsync_TooManyRequests_RetriesAndSucceeds()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var tooManyRequestsResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent(@"{ ""error"": { ""message"": ""Rate limit exceeded"" } }"),
            };
            var okResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ ""candidates"": [ { ""content"": { ""parts"": [ { ""text"": ""Successful analysis"" } ] } } ] }"),
            };

            handlerMock
               .Protected()
               .SetupSequence<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(tooManyRequestsResponse)
               .ReturnsAsync(okResponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var analyzer = new VideoAnalyzer(httpClient, "test_api_key", "test_model", 3, TimeSpan.FromMilliseconds(10)); // Use a short delay for testing
            var transcript = "This is a test transcript.";
            var videoUrl = "https://www.youtube.com/watch?v=test";

            // Act
            var result = await analyzer.AnalyzeVideoAsync(transcript, videoUrl);

            // Assert
            Assert.Equal("Successful analysis", result);
            handlerMock.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AnalyzeVideoAsync_TooManyRequests_FailsAfterMaxRetries()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var tooManyRequestsResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent(@"{ ""error"": { ""message"": ""Rate limit exceeded"" } }"),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(tooManyRequestsResponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var analyzer = new VideoAnalyzer(httpClient, "test_api_key", "test_model", 3, TimeSpan.FromMilliseconds(10));
            var transcript = "This is a test transcript.";
            var videoUrl = "https://www.youtube.com/watch?v=test";

            // Act
            var result = await analyzer.AnalyzeVideoAsync(transcript, videoUrl);

                        // Assert
            Assert.StartsWith("Error: TooManyRequests - Max retries reached.", result);
            handlerMock.Protected().Verify("SendAsync", Times.Exactly(3), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AnalyzeVideoAsync_ApiReturnsError_ReturnsErrorString()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var errorResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error"),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(errorResponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var analyzer = new VideoAnalyzer(httpClient, "test_api_key", "test_model", 3, TimeSpan.FromSeconds(1));
            var transcript = "This is a test transcript.";
            var videoUrl = "https://www.youtube.com/watch?v=test";

            // Act
            var result = await analyzer.AnalyzeVideoAsync(transcript, videoUrl);

            // Assert
            Assert.Equal("Error: InternalServerError. Details: Internal Server Error", result);
        }

        [Fact]
        public async Task AnalyzeVideoAsync_MalformedResponse_ReturnsNoContentFound()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var malformedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ ""invalid_json"": ""some_value"" }"),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(malformedResponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var analyzer = new VideoAnalyzer(httpClient, "test_api_key", "test_model", 3, TimeSpan.FromSeconds(1));
            var transcript = "This is a test transcript.";
            var videoUrl = "https://www.youtube.com/watch?v=test";

            // Act
            var result = await analyzer.AnalyzeVideoAsync(transcript, videoUrl);

            // Assert
            Assert.Equal("No content generated or content has an unexpected structure.", result);
        }
    }
}

