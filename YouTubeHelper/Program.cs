using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true);

        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
        {
            builder.AddUserSecrets<Program>();
        }

        var configuration = builder.Build();

        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        string? apiKey = configuration["GeminiApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "Add Key Here")
        {
            Console.WriteLine("API key not found in appsettings.json. Please add it.");
            return;
        }

        var analysisService = serviceProvider.GetRequiredService<AnalysisService>();
        try
        {
            await analysisService.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex}");
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<HttpClient>();
        services.AddSingleton<YoutubeClient>();
        services.AddSingleton<IYouTubeClient, YouTubeClientWrapper>();
        services.AddSingleton<TranscriptService>();
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var httpClient = provider.GetRequiredService<HttpClient>();
            string apiKey = config["GeminiApiKey"] ?? throw new InvalidOperationException("API key not found.");
            string model = config["GeminiModel"] ?? "gemini-2.5-pro";
            int maxRetries = int.TryParse(config["MaxRetries"], out var retries) ? retries : 5;
            int initialDelaySeconds = int.TryParse(config["InitialDelaySeconds"], out var delay) ? delay : 5;
            return new VideoAnalyzer(httpClient, apiKey, model, maxRetries, TimeSpan.FromSeconds(initialDelaySeconds));
        });
        services.AddSingleton<AnalysisService>();
    }
}


