using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

public class YouTubeClientWrapper : IYouTubeClient
{
    private readonly YoutubeClient _client;

    public YouTubeClientWrapper(YoutubeClient client)
    {
        _client = client;
    }

    public async Task<Video> GetVideoAsync(string videoUrl)
    {
        return await _client.Videos.GetAsync(videoUrl);
    }

    public async Task<ClosedCaptionManifest> GetClosedCaptionManifestAsync(string videoUrl)
    {
        return await _client.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
    }

    public async Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo trackInfo)
    {
        return await _client.Videos.ClosedCaptions.GetAsync(trackInfo);
    }
}
