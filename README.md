# YouTube Video Analyzer

This C# console application automates the process of analyzing YouTube video transcripts using the Google Gemini API. It fetches the transcript of a given YouTube video, sends it to the Gemini API for analysis, and saves the returned analysis into a markdown file.

## Features

- Fetches YouTube video transcripts using the `YoutubeExplode` library.
- Integrates with the Google Gemini API to generate video analysis.
- Implements a robust retry mechanism with exponential backoff for handling API rate limits.
- Saves the analysis for each video into a separate markdown file, named after the video's title.
- Configuration is managed through an `appsettings.json` file.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A Google Gemini API Key. You can obtain one from [Google AI Studio](https://aistudio.google.com/).

### Installation & Configuration

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd <repository-directory>
    ```

2.  **Configure your API Key:**
    Create a file named `appsettings.json` in the `YouTubeHelper` directory with the following content:

    ```json
    {
      "GeminiApiKey": "YOUR_API_KEY_HERE",
      "GeminiModel": "gemini-2.5-pro"
    }
    ```

    Replace `"YOUR_API_KEY_HERE"` with your actual Google Gemini API key.

3.  **Restore Dependencies:**
    Run the following command in the root directory to restore the necessary NuGet packages.
    ```bash
    dotnet restore
    ```

## Usage

1.  **Add Video URLs:**
    Open the `YouTubeHelper/Program.cs` file and modify the `videoUrls` list to include the YouTube videos you want to analyze.

    ```csharp
    // ...
    var videoUrls = new List<string>
    {
        "https://www.youtube.com/watch?v=your_video_id_1",
        "https://www.youtube.com/watch?v=your_video_id_2"
    };
    // ...
    ```

2.  **Run the Application:**
    Execute the following command from the `YouTubeHelper` directory:
    ```bash
    dotnet run
    ```

3.  **View Results:**
    The application will create a `results` directory within the `YouTubeHelper` project folder. Inside, you will find a markdown file for each analyzed video, containing the summary provided by the Gemini API.

## Project Structure

-   `YouTubeHelper/`: The main console application project.
    -   `Program.cs`: The entry point of the application. Contains the list of videos to be analyzed.
    -   `VideoAnalyzer.cs`: Handles the interaction with the Gemini API.
    -   `TranscriptService.cs`: Uses `YoutubeExplode` to fetch video transcripts.
    -   `appsettings.json`: Configuration file for the API key and model.
    -   `results/`: Directory where the analysis markdown files are saved.
-   `YouTubeHelper.Tests/`: The xUnit test project for the application.
    -   `VideoAnalyzerTests.cs`: Contains unit tests for the `VideoAnalyzer` service.
    -   `TranscriptServiceTests.cs`: Contains placeholder unit tests for the `TranscriptService`.
