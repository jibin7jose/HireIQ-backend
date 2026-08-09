using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CareerConnect.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CareerConnect.Infrastructure.Services;

public class GeminiAiService : IGeminiAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
    }

    public async Task<string> ParseResumeAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Return an empty JSON object matching the expected schema if no API key is present,
            // to avoid breaking the flow during local development without a key.
            return "{\"Skills\": [], \"ExperienceSummary\": \"\", \"Education\": \"\"}";
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

        var prompt = @"
You are an expert HR AI assistant. Parse the following resume text and extract the candidate's skills, experience summary, and education.
Return ONLY a raw JSON object with no markdown formatting, using this exact schema:
{
  ""Skills"": [""Skill 1"", ""Skill 2""],
  ""ExperienceSummary"": ""A brief 2-3 sentence summary of their work experience."",
  ""Education"": ""A brief summary of their highest education or degrees.""
}

Resume Text:
" + resumeText;

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
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Parse the Gemini response format to extract the actual generated text
        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;
        
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var content) && 
                content.TryGetProperty("parts", out var parts) && 
                parts.GetArrayLength() > 0)
            {
                var text = parts[0].GetProperty("text").GetString();
                return text ?? "{}";
            }
        }

        return "{}";
    }

    public async Task<int> CalculateMatchScoreAsync(string jobDescription, string candidateSkills, string candidateExperience, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return 50; // Fallback score if no API key
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

        var prompt = $@"
You are an expert HR AI assistant evaluating a candidate against a job description.
Calculate a match score from 0 to 100 based on how well the candidate's skills and experience match the job requirements.
Return ONLY the raw integer score, with no formatting or other text.

Job Description:
{jobDescription}

Candidate Skills:
{candidateSkills}

Candidate Experience:
{candidateExperience}
";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.1 }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;
        
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var content) && 
                content.TryGetProperty("parts", out var parts) && 
                parts.GetArrayLength() > 0)
            {
                var text = parts[0].GetProperty("text").GetString()?.Trim();
                if (int.TryParse(text, out int score))
                {
                    return Math.Clamp(score, 0, 100);
                }
            }
        }

        return 50; // default if parsing fails
    }

    public async Task<Dictionary<Guid, int>> BatchCalculateMatchScoresAsync(IEnumerable<CareerConnect.Domain.Entities.Job> jobs, string candidateSkills, string candidateExperience, CancellationToken cancellationToken = default)
    {
        var scores = new Dictionary<Guid, int>();
        if (string.IsNullOrEmpty(_apiKey) || !jobs.Any())
        {
            foreach (var job in jobs) scores[job.Id] = 50;
            return scores;
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

        var jobsJson = JsonSerializer.Serialize(jobs.Select(j => new { j.Id, j.Title, j.Description }));

        var prompt = $@"
You are an expert HR AI assistant evaluating a candidate against a list of job descriptions.
Calculate a match score from 0 to 100 based on how well the candidate's skills and experience match EACH job's requirements.
Return ONLY a raw JSON dictionary mapping the Job Id to the integer score, with no formatting or other text. Example: {{""00000000-0000-0000-0000-000000000000"": 85, ...}}

Candidate Skills:
{candidateSkills}

Candidate Experience:
{candidateExperience}

Jobs:
{jobsJson}
";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.1, responseMimeType = "application/json" }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            foreach (var job in jobs) scores[job.Id] = 50;
            return scores;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) && 
                    content.TryGetProperty("parts", out var parts) && 
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString()?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var parsedScores = JsonSerializer.Deserialize<Dictionary<Guid, int>>(text);
                        if (parsedScores != null)
                        {
                            foreach (var job in jobs)
                            {
                                scores[job.Id] = parsedScores.TryGetValue(job.Id, out var score) ? Math.Clamp(score, 0, 100) : 50;
                            }
                            return scores;
                        }
                    }
                }
            }
        }
        catch 
        {
            // fallback below
        }

        foreach (var job in jobs) scores[job.Id] = 50;
        return scores;
    }
}
