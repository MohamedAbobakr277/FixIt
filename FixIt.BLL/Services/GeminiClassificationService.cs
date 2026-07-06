using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using FixIt.Common.Enums;
using FixIt.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FixIt.BLL.Services;

public class GeminiClassificationService : IAiClassificationService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiClassificationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiClassificationService(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiClassificationService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiClassificationResultDto?> ClassifyIssueFromImageAsync(byte[] imageData, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey == "YOUR_GEMINI_API_KEY")
        {
            _logger.LogWarning("Gemini API key is not configured. Skipping AI classification.");
            return null;
        }

        try
        {
            var base64Image = Convert.ToBase64String(imageData);
            var prompt = BuildImagePrompt();

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                inlineData = new
                                {
                                    mimeType = mimeType,
                                    data = base64Image
                                }
                            },
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 2000,
                    responseMimeType = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", _settings.ApiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("===== GEMINI API ERROR =====");
                _logger.LogError("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogError("Response: {Body}", responseBody);
                _logger.LogError("============================");
                return null;
            }

            return ParseGeminiResponse(responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API for image classification.");
            return null;
        }
    }

    private static string BuildImagePrompt()
    {
        return """
            You are an AI assistant for a municipal issue reporting system called "FixIt".
            A citizen has uploaded a photo of an issue they want to report.

            Analyze this image carefully and determine:
            1. A concise, descriptive title for the issue (max 100 characters)
            2. A detailed description of what you see in the image and what the issue is (2-3 sentences)
            3. The most appropriate category
            4. The priority level

            Available Categories:
            - Electrical: Issues related to electricity, power outages, broken streetlights, exposed wiring, etc.
            - Water: Issues related to water supply, water leaks, broken pipes, water contamination, etc.
            - Plumbing: Issues related to drainage, sewage, blocked drains, toilet/sink issues, etc.
            - Carpentry: Issues related to broken furniture, doors, windows, wooden structures, etc.
            - GeneralMaintenance: Issues related to painting, cleaning, general repairs, road damage, potholes, etc.
            - Other: Issues that don't fit any of the above categories.

            Available Priority Levels:
            - High: Urgent issues that pose safety risks, affect many people, or could cause significant damage if not addressed immediately.
            - Medium: Important issues that need attention soon but are not immediately dangerous.
            - Low: Minor issues that can be addressed during routine maintenance.

            Respond with a JSON object containing exactly these fields:
            {
                "title": "<a concise, descriptive title for the issue>",
                "description": "<a detailed 2-3 sentence description of the issue visible in the image>",
                "category": "<one of: Electrical, Water, Plumbing, Carpentry, GeneralMaintenance, Other>",
                "priority": "<one of: High, Medium, Low>",
                "categoryReason": "<brief 1-sentence explanation for why this category was chosen>",
                "priorityReason": "<brief 1-sentence explanation for why this priority was chosen>",
                "confidence": <number between 0.0 and 1.0 representing your confidence in the classification>
            }
            """;
    }

    private AiClassificationResultDto? ParseGeminiResponse(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Navigate: candidates[0].content.parts[0].text
            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                return null;

            // Clean the text — remove markdown code fences if present
            text = text.Trim();
            if (text.StartsWith("```"))
            {
                var firstNewline = text.IndexOf('\n');
                if (firstNewline > 0)
                    text = text[(firstNewline + 1)..];
                if (text.EndsWith("```"))
                    text = text[..^3];
                text = text.Trim();
            }

            using var parsed = JsonDocument.Parse(text);
            var obj = parsed.RootElement;

            var title = obj.GetProperty("title").GetString() ?? "";
            var description = obj.GetProperty("description").GetString() ?? "";
            var categoryStr = obj.GetProperty("category").GetString() ?? "Other";
            var priorityStr = obj.GetProperty("priority").GetString() ?? "Medium";
            var categoryReason = obj.GetProperty("categoryReason").GetString() ?? "";
            var priorityReason = obj.GetProperty("priorityReason").GetString() ?? "";
            var confidence = obj.GetProperty("confidence").GetDouble();

            if (!Enum.TryParse<IssueCategory>(categoryStr, ignoreCase: true, out var category))
                category = IssueCategory.Other;

            if (!Enum.TryParse<IssuePriority>(priorityStr, ignoreCase: true, out var priority))
                priority = IssuePriority.Medium;

            return new AiClassificationResultDto
            {
                SuggestedTitle = title,
                SuggestedDescription = description,
                SuggestedCategory = category,
                SuggestedPriority = priority,
                CategoryReason = categoryReason,
                PriorityReason = priorityReason,
                Confidence = Math.Clamp(confidence, 0.0, 1.0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response.");
            return null;
        }
    }
}
