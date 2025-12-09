using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YoutubeExplode;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string? apiKey = configuration["ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY")
        {
            Console.WriteLine("API key not found in appsettings.json. Please add it.");
            return;
        }
        string model = "gemini-2.5-pro";
        
        var youtube = new YoutubeClient();
        var transcriptService = new TranscriptService(youtube);

        var urls = File.ReadAllLines("urls.txt");
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
                var video = await transcriptService.GetVideoInfoAsync(cleanedUrl);
                fileName = video.Title.Replace(' ', '-') + ".md";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(c, '_');
                }

                var transcript = await transcriptService.GetTranscriptAsync(cleanedUrl);

                if (transcript != null)
                {
                    Console.WriteLine("Found English transcript. Analyzing...");
                    using var httpClient = new HttpClient();
                    var analyzer = new VideoAnalyzer(httpClient, apiKey, model);
                    analysisResult = await analyzer.AnalyzeVideoAsync(transcript);
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

            await File.WriteAllTextAsync(Path.Combine("results", fileName), analysisResult);
            Console.WriteLine($"Saved analysis to {Path.Combine("results", fileName)}");
            Console.WriteLine();

            Console.WriteLine("Waiting for 60 seconds before processing the next URL...");
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }
}


