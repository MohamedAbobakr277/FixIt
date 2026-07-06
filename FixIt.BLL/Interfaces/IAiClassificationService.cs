using FixIt.Common.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IAiClassificationService
{
    /// <summary>
    /// Analyzes an uploaded image using Gemini Vision and returns
    /// suggested title, description, category, and priority.
    /// </summary>
    Task<AiClassificationResultDto?> ClassifyIssueFromImageAsync(byte[] imageData, string mimeType);
}
