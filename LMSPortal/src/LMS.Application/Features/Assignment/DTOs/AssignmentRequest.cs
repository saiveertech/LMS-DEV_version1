
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Assignment.DTOs;

public class CreateAssignmentRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CompletionTimeSeconds { get; set; }

    public decimal PassPercentage { get; set; }

    public string? WwEnvClientId { get; set; }

    public string? Tags { get; set; }

    public IFormFile? IntroVideo { get; set; }

    public IFormFile? QuestionsCsv { get; set; }

    public IFormFile? AssessmentIcon { get; set; }
}

public class UpdateAssignmentRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? CompletionTimeSeconds { get; set; }

    public decimal? PassPercentage { get; set; }

    public string? WwEnvClientId { get; set; }

    public string? Tags { get; set; }

    public IFormFile? IntroVideo { get; set; }

    public IFormFile? QuestionsCsv { get; set; }

    public IFormFile? AssessmentIcon { get; set; }
}
