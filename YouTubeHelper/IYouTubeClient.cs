using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

public interface IYouTubeClient
{
    Task<Video> GetVideoAsync(string videoUrl);
    Task<ClosedCaptionManifest> GetClosedCaptionManifestAsync(string videoUrl);
    Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo trackInfo);
}
