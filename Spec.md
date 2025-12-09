# Technical Spec YouTube Helper for re:Invent

1. **Objective:** I need an application that can be provided a list of urls from youtube. The application should then have gemini anayzle each video. This is the prompt I want to provide with each video to send over to the LLM:

    1. Can you watch this video, extract a transcript, then proceed to provide me with an overview and detailed analysis. Include ref to specific spots in the video if possible as source links - https://www.youtube.com/watch?v=qHvm3oFmRls\&t=61s - please make sure you don't miss any technical details or nuances.

2. The application should be written in c#

3. **Features**

    1. Ability to read in a file containing you tube urls
    2. Ability to send each url over to the gemini llm to be analyzed. A specific prompt should be used
    3. For each video analyzed, the results of the analysis should be saved in Goole docs if possible, otherwise save it locally as a markdown file
```
