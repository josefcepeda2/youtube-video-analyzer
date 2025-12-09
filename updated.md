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

The application is built using .NET 10 and is structured into several key components to ensure separation of concerns and testability.

### 3.1. Main Application (`Program.cs`)

-   **Role**: Entry point and orchestrator.
-   **Responsibilities**:
    -   Reads configuration from `appsettings.json`.
    -   Initializes `TranscriptService` and `VideoAnalyzer` services.
    -   Contains the hardcoded list of YouTube video URLs to be processed.
    -   Iterates through the list of URLs, calling the services to fetch, analyze, and save the results.
    -   Handles console logging for status updates and errors.

### 3.2. `TranscriptService.cs`

-   **Role**: Encapsulates all interaction with the `YoutubeExplode` library.
-   **Responsibilities**:
    -   Provides a method `GetVideoInfoAsync(string videoUrl)` to fetch the video's title.
    -   Provides a method `GetTranscriptAsync(string videoUrl)` to retrieve the full closed-caption text.
    -   Handles any errors related to fetching video data (e.g., video not found, no transcript available).

### 3.3. `VideoAnalyzer.cs`

-   **Role**: Encapsulates all interaction with the Google Gemini API.
-   **Responsibilities**:
    -   Provides a method `AnalyzeVideoAsync(string transcript)` that takes the transcript text as input.
    -   Constructs the JSON payload for the Gemini API request.
    -   Implements a retry mechanism with exponential backoff to handle `429 Too Many Requests` errors from the API.
    -   Parses the JSON response to extract the generated analysis text.
    -   Manages the `HttpClient` for making API calls.

## 4. Configuration (`appsettings.json`)

-   A JSON file will store configuration variables.
-   `GeminiApiKey`: The user's secret API key for accessing the Google Gemini service.
-   `GeminiModel`: The specific Gemini model to be used for the analysis (e.g., `gemini-2.5-pro`).

## 5. Testing (`YouTubeHelper.Tests`)

-   A dedicated xUnit test project will be used for unit testing.
-   **`VideoAnalyzerTests.cs`**: Tests the `VideoAnalyzer` service. It uses `Moq` to mock the `HttpMessageHandler` to simulate API responses (both success and failure scenarios) without making actual network calls.
-   **`TranscriptServiceTests.cs`**: Contains placeholder tests for the `TranscriptService`. Mocking the `YoutubeExplode` client is complex and is considered out of scope for the initial implementation.

## 6. Dependencies

-   `Microsoft.Extensions.Configuration.Json`: For loading configuration from `appsettings.json`.
-   `System.Net.Http`: For making requests to the Gemini API.
-   `YoutubeExplode`: For fetching video transcripts and metadata.
-   `xUnit`: The testing framework.
-   `Moq`: For creating mock objects in tests.
