using System;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

public class TranscriptService
{
    private readonly IYouTubeClient _youtube;

    public TranscriptService(IYouTubeClient youtube)
    {
        _youtube = youtube;
    }

    public async Task<Video> GetVideoInfoAsync(string videoUrl)
    {
        return await _youtube.GetVideoAsync(videoUrl);
    }

    public async Task<string?> GetTranscriptAsync(string videoUrl)
    {
        var trackManifest = await _youtube.GetClosedCaptionManifestAsync(videoUrl);
        
        try
        {
            var trackInfo = trackManifest.GetByLanguage("en");
            var transcript = await _youtube.GetClosedCaptionTrackAsync(trackInfo);
            var transcriptText = new StringBuilder();
            foreach (var caption in transcript.Captions)
            {
                transcriptText.Append(caption.Text).Append(' ');
            }
            return transcriptText.ToString();
        }
        catch (InvalidOperationException)
        {
            // This exception is thrown by GetByLanguage if the language is not found
            return null;
        }
    }
}
