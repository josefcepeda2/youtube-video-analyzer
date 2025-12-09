using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class AnalysisService
{
    private readonly IConfiguration _configuration;
    private readonly TranscriptService _transcriptService;
    private readonly VideoAnalyzer _videoAnalyzer;

    public AnalysisService(IConfiguration configuration, TranscriptService transcriptService, VideoAnalyzer videoAnalyzer)
    {
        _configuration = configuration;
        _transcriptService = transcriptService;
        _videoAnalyzer = videoAnalyzer;
    }

    public async Task RunAsync()
    {
        int requestDelaySeconds = int.TryParse(_configuration["RequestDelaySeconds"], out var requestDelay) ? requestDelay : 60;
        string urlsFilePath = _configuration["UrlsFilePath"] ?? "urls.txt";
        var fullPath = Path.Combine(AppContext.BaseDirectory, urlsFilePath);

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"The specified URLs file was not found: {fullPath}");
            return;
        }

        var resultsDirectory = Path.Combine(AppContext.BaseDirectory, "results");
        Directory.CreateDirectory(resultsDirectory);

        var urls = File.ReadAllLines(fullPath);
        foreach (var url in urls)
        {
            var cleanedUrl = url.Trim('<', '>');
            if (!Uri.TryCreate(cleanedUrl, UriKind.Absolute, out _))
            {
                Console.WriteLine($"Skipping invalid URL: {cleanedUrl}");
                continue;
            }

            Console.WriteLine($"Processing {cleanedUrl}...");
            string fileName;
            string analysisResult;

            try
            {
                var video = await _transcriptService.GetVideoInfoAsync(cleanedUrl);
                fileName = video.Title.Replace(' ', '-') + ".md";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(c, '_');
                }

                var transcript = await _transcriptService.GetTranscriptAsync(cleanedUrl);

                if (transcript != null)
                {
                    Console.WriteLine("Found English transcript. Analyzing...");
                    analysisResult = await _videoAnalyzer.AnalyzeVideoAsync(transcript, cleanedUrl);
                }
                else
                {
                    analysisResult = "No English transcript found for this video.";
                    Console.WriteLine(analysisResult);
                }
            }
            catch (Exception ex)
            {
                analysisResult = $"Error processing video: {ex.Message}";
                fileName = Guid.NewGuid() + ".md"; // Fallback filename
                Console.WriteLine(analysisResult);
            }

            var resultPath = Path.Combine(resultsDirectory, fileName);
            await File.WriteAllTextAsync(resultPath, analysisResult);
            Console.WriteLine($"Saved analysis to {resultPath}");
            Console.WriteLine();

            Console.WriteLine($"Waiting for {requestDelaySeconds} seconds before processing the next URL...");
            await Task.Delay(TimeSpan.FromSeconds(requestDelaySeconds));
        }
    }
}
