using Moq;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Common;

public class TranscriptServiceTests
{
    [Fact]
    public async Task GetTranscriptAsync_WhenEnglishTranscriptExists_ReturnsTranscript()
    {
        // Arrange
        var mockYouTubeClient = new Mock<IYouTubeClient>();
        var videoUrl = "https://www.youtube.com/watch?v=test";
        var trackInfo = new ClosedCaptionTrackInfo("https://example.com",  new Language("en", "English"), false);
        var manifest = new ClosedCaptionManifest(new[] { trackInfo });
        var captions = new List<ClosedCaption>
        {
            new ClosedCaption("Hello", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), Array.Empty<ClosedCaptionPart>()),
            new ClosedCaption("world", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), Array.Empty<ClosedCaptionPart>())
        };
        var track = new ClosedCaptionTrack(captions);

        mockYouTubeClient.Setup(c => c.GetClosedCaptionManifestAsync(It.IsAny<string>())).ReturnsAsync(manifest);
        mockYouTubeClient.Setup(c => c.GetClosedCaptionTrackAsync(trackInfo)).ReturnsAsync(track);

        var service = new TranscriptService(mockYouTubeClient.Object);

        // Act
        var result = await service.GetTranscriptAsync(videoUrl);

        // Assert
        Assert.Equal("Hello world ", result);
    }

    [Fact]
    public async Task GetTranscriptAsync_WhenNoEnglishTranscript_ReturnsNull()
    {
        // Arrange
        var mockYouTubeClient = new Mock<IYouTubeClient>();
        var videoUrl = "https://www.youtube.com/watch?v=test";
        var manifest = new ClosedCaptionManifest(new ClosedCaptionTrackInfo[0]);

        mockYouTubeClient.Setup(c => c.GetClosedCaptionManifestAsync(It.IsAny<string>())).ReturnsAsync(manifest);

        var service = new TranscriptService(mockYouTubeClient.Object);

        // Act
        var result = await service.GetTranscriptAsync(videoUrl);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetVideoInfoAsync_ReturnsVideoInfo()
    {
        // Arrange
        var mockYouTubeClient = new Mock<IYouTubeClient>();
        var videoUrl = "https://www.youtube.com/watch?v=test";
        var author = new Author("UC_x5XG1OV2P6uZZ5FSM9Ttw", "Test Author");
        var engagement = new Engagement(0, 0, 0);
        var video = new Video(new VideoId(videoUrl), "Test Title", author, DateTimeOffset.Now, "Test Description", TimeSpan.FromMinutes(5), new Thumbnail[] { }, new List<string>(), engagement);

        mockYouTubeClient.Setup(c => c.GetVideoAsync(It.IsAny<string>())).ReturnsAsync(video);

        var service = new TranscriptService(mockYouTubeClient.Object);

        // Act
        var result = await service.GetVideoInfoAsync(videoUrl);

        // Assert
        Assert.Equal("Test Title", result.Title);
    }
}
