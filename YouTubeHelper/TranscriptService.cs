using System;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

public class TranscriptService
{
    private readonly YoutubeClient _youtube;

    public TranscriptService(YoutubeClient youtube)
    {
        _youtube = youtube;
    }

    public async Task<Video> GetVideoInfoAsync(string videoUrl)
    {
        return await _youtube.Videos.GetAsync(videoUrl);
    }

    public async Task<string?> GetTranscriptAsync(string videoUrl)
    {
        var trackManifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
        var trackInfo = trackManifest.GetByLanguage("en");

        if (trackInfo == null)
        {
            return null;
        }

        var transcript = await _youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
        var transcriptText = new StringBuilder();
        foreach (var caption in transcript.Captions)
        {
            transcriptText.Append(caption.Text).Append(' ');
        }

        return transcriptText.ToString();
    }
}
