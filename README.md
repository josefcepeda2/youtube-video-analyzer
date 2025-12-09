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

2.  **Configure your API Key (Recommended for Development):**
    Use the .NET Secret Manager to securely store your API key. This prevents you from accidentally checking it into source control.
    ```bash
    dotnet user-secrets init --project YouTubeHelper/YouTubeHelper.csproj
    dotnet user-secrets set "GeminiApiKey" "YOUR_API_KEY_HERE" --project YouTubeHelper/YouTubeHelper.csproj
    ```
    Replace `"YOUR_API_KEY_HERE"` with your actual Google Gemini API key.

3.  **Configure Application Settings:**
    In the `YouTubeHelper` directory, open the `appsettings.json` file. You can configure the model and file paths here.
    ```json
    {
      "GeminiModel": "gemini-2.5-pro",
      "UrlsFilePath": "urls.txt",
      "MaxRetries": 5,
      "InitialDelaySeconds": 5,
      "RequestDelaySeconds": 60
    }
    ```

4.  **Create URLs File:**
    Create a file named `urls.txt` (or the name you specified in `appsettings.json`) in the `YouTubeHelper` directory. Add one YouTube video URL per line.
    ```
    https://www.youtube.com/watch?v=your_video_id_1
    https://www.youtube.com/watch?v=your_video_id_2
    ```

5.  **Restore Dependencies:**
    Run the following command in the root directory to restore the necessary NuGet packages.
    ```bash
    dotnet restore
    ```

## Usage

1.  **Run the Application (Development):**
    To ensure your user secrets are loaded, run the application with the `DOTNET_ENVIRONMENT` variable set to `Development`.
    ```powershell
    # For PowerShell
    $env:DOTNET_ENVIRONMENT="Development"; dotnet run --project YouTubeHelper/YouTubeHelper.csproj
    ```
    ```bash
    # For Bash
    export DOTNET_ENVIRONMENT=Development; dotnet run --project YouTubeHelper/YouTubeHelper.csproj
    ```

2.  **View Results:**
    The application will create a `results` directory within the build output folder (e.g., `YouTubeHelper/bin/Debug/net10.0/results`). Inside, you will find a markdown file for each analyzed video.

## Project Structure

-   `YouTubeHelper/`: The main console application project.
    -   `Program.cs`: The composition root for DI and configuration.
    -   `AnalysisService.cs`: Orchestrates the main application workflow.
    -   `VideoAnalyzer.cs`: Handles the interaction with the Gemini API.
    -   `TranscriptService.cs`: Uses `YoutubeExplode` to fetch video transcripts.
    -   `IYouTubeClient.cs` / `YouTubeClientWrapper.cs`: Abstraction over `YoutubeExplode` for testability.
    -   `appsettings.json`: Configuration file for the application.
    -   `urls.txt`: Default file for listing video URLs.
-   `YouTubeHelper.Tests/`: The xUnit test project for the application.
    -   `VideoAnalyzerTests.cs`: Contains unit tests for the `VideoAnalyzer` service.
    -   `TranscriptServiceTests.cs`: Contains unit tests for the `TranscriptService`.
-   `Spec.md`: The detailed technical specification for the project.
