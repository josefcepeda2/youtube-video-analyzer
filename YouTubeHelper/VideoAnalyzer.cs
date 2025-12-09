using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class VideoAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public VideoAnalyzer(HttpClient httpClient, string apiKey, string model)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> AnalyzeVideoAsync(string transcript)
    {
        string prompt = $"Please provide a detailed analysis and summary of the following YouTube video transcript:\n\n{transcript}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        
        Console.WriteLine($"Sending transcript for analysis...");

        const int maxRetries = 5;
        int retryCount = 0;
        TimeSpan delay = TimeSpan.FromSeconds(5); // Initial delay

        while (retryCount < maxRetries)
        {
            try
            {
                // Recreate content for each attempt as it may be consumed
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}", content);

                Console.WriteLine($"Received analysis response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(responseContent))
                    {
                        // Safely parse the response
                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var firstCandidate = candidates[0];
                            if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                                contentElement.TryGetProperty("parts", out var parts) &&
                                parts.GetArrayLength() > 0 &&
                                parts[0].TryGetProperty("text", out var textElement))
                            {
                                return textElement.GetString() ?? "No content found.";
                            }
                        }
                    }
                    return "No content generated or content has an unexpected structure.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    retryCount++;
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (retryCount >= maxRetries)
                    {
                        return $"Error: {response.StatusCode} - Max retries reached. {errorContent}";
                    }

                    // Try to parse the suggested delay from the API response
                    try
                    {
                        using (JsonDocument errorDoc = JsonDocument.Parse(errorContent))
                        {
                            if (errorDoc.RootElement.TryGetProperty("error", out var errorElement) &&
                                errorElement.TryGetProperty("details", out var details))
                            {
                                foreach (var detail in details.EnumerateArray())
                                {
                                    if (detail.TryGetProperty("@type", out var type) && type.GetString() == "type.googleapis.com/google.rpc.RetryInfo" && detail.TryGetProperty("retryDelay", out var delayElement))
                                    {
                                        var delayString = delayElement.GetString();
                                        if (delayString != null && delayString.EndsWith("s") && double.TryParse(delayString.TrimEnd('s'), out double seconds))
                                        {
                                            delay = TimeSpan.FromSeconds(Math.Max(seconds, 1));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (JsonException) { /* Could not parse retry delay, use existing backoff */ }
                    
                    var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 1000));
                    var totalDelay = delay + jitter;

                    Console.WriteLine($"Rate limit hit. Retrying in {totalDelay.TotalSeconds:F1} seconds...");
                    await Task.Delay(totalDelay);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 120)); // Exponential backoff
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }
            }
            catch (HttpRequestException ex) // Catch network-related errors
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    return $"Error: HttpRequestException after max retries. {ex.Message}";
                }
                Console.WriteLine($"Network error. Retrying in {delay.TotalSeconds} seconds... ({ex.Message})");
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 120));
            }
        }
        return "Error: Max retries reached.";
    }
}
