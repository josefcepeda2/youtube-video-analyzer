# Project Specification: YouTube Video Analyzer

## 1. Overview

This document outlines the technical specification for a C# console application designed to analyze YouTube video transcripts using the Google Gemini API. The application fetches video transcripts, submits them to the Gemini API for analysis, and saves the results to local markdown files.

## 2. Core Functionality

-   **Video Transcript Fetching**: The application will accept a list of YouTube video URLs. For each URL, it will use the `YoutubeExplode` library to fetch the video's metadata (specifically the title) and the full closed-caption transcript.
-   **AI-Powered Analysis**: The fetched transcript text will be sent to the Google Gemini API. The prompt will instruct the API to provide a concise analysis or summary of the transcript content.
-   **Output Generation**: The analysis returned by the Gemini API will be saved into a new markdown file.
-   **File Naming**: Each output file will be named after the corresponding video's title, with spaces replaced by dashes to ensure valid filenames (e.g., `My-Video-Title.md`).
-   **Result Storage**: All generated markdown files will be stored in a `results` directory within the main project folder.

## 3. Architecture and Components

The application is built using .NET 10 and follows a modern, service-oriented architecture using Dependency Injection to promote separation of concerns and testability.

### 3.1. Composition Root (`Program.cs`)

-   **Role**: The application's entry point and composition root.
-   **Responsibilities**:
    -   Sets up the configuration providers (e.g., `appsettings.json`, user secrets).
    -   Configures the Dependency Injection container by registering all services.
    -   Resolves the main `AnalysisService` and executes it.
    -   Handles top-level exceptions.

### 3.2. `AnalysisService.cs`

-   **Role**: Orchestrates the main application workflow.
-   **Responsibilities**:
    -   Reads the path to the URLs file from configuration.
    -   Reads the list of video URLs from the specified file.
    -   Iterates through the URLs, delegating to other services to fetch, analyze, and save the results.
    -   Ensures the `results` directory exists.
    -   Handles console logging for status updates and errors during the process.

### 3.3. `TranscriptService.cs`

-   **Role**: Encapsulates all interaction with the `YoutubeExplode` library.
-   **Dependencies**: `IYouTubeClient`.
-   **Responsibilities**:
    -   Provides a method `GetVideoInfoAsync(string videoUrl)` to fetch video metadata.
    -   Provides a method `GetTranscriptAsync(string videoUrl)` to retrieve the full closed-caption text.

### 3.4. `VideoAnalyzer.cs`

-   **Role**: Encapsulates all interaction with the Google Gemini API.
-   **Responsibilities**:
    -   Provides a method `AnalyzeVideoAsync(string transcript, string videoUrl)` that takes the transcript and original URL as input.
    -   Constructs the JSON payload for the Gemini API request with a detailed prompt.
    -   Implements a retry mechanism with exponential backoff to handle rate-limiting errors.
    -   Parses the JSON response to extract the generated analysis text.

### 3.5. `IYouTubeClient` / `YouTubeClientWrapper.cs`

-   **Role**: An abstraction layer over `YoutubeExplode.YoutubeClient`.
-   **Responsibilities**:
    -   `IYouTubeClient` defines an interface for fetching video data and transcripts.
    -   `YouTubeClientWrapper` implements this interface by wrapping the concrete `YoutubeClient`, allowing it to be mocked in unit tests.

## 4. Configuration (`appsettings.json`)

The application uses a layered configuration approach, reading from `appsettings.json` and, in the Development environment, from User Secrets.

-   `GeminiApiKey`: The user's secret API key for the Google Gemini service. **It is strongly recommended to store this in User Secrets during development.**
-   `GeminiModel`: The specific Gemini model to be used (e.g., `gemini-2.5-pro`).
-   `UrlsFilePath`: The path to the text file containing the list of YouTube URLs to process (e.g., `urls.txt`).
-   `MaxRetries`: The maximum number of retry attempts for API calls.
-   `InitialDelaySeconds`: The initial delay in seconds before the first retry.
-   `RequestDelaySeconds`: The delay in seconds between processing each video URL to avoid rate-limiting.

## 5. Testing (`YouTubeHelper.Tests`)

-   A dedicated xUnit test project ensures the correctness of the application's components.
-   **`VideoAnalyzerTests.cs`**: Tests the `VideoAnalyzer` service. It uses `Moq` to mock the `HttpMessageHandler` to simulate various API responses, including success, rate-limiting errors, and other failures.
-   **`TranscriptServiceTests.cs`**: Tests the `TranscriptService`. It uses a mock of `IYouTubeClient` to test the service's logic without making calls to the actual YouTube API.

## 6. Dependencies

-   `Microsoft.Extensions.Configuration` and related packages for configuration.
-   `Microsoft.Extensions.DependencyInjection` for dependency injection.
-   `Microsoft.Extensions.Configuration.UserSecrets` for managing the API key in development.
-   `YoutubeExplode`: For fetching video transcripts and metadata.
-   `xUnit`: The testing framework.
-   `Moq`: For creating mock objects in tests.
