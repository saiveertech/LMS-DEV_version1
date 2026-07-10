namespace LMS.Application.Features.Certificate.DTOs;

public class VerifyCertificateResponse
{
    public string CertificateId { get; set; } = string.Empty;

    public string CredentialId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public decimal CompletionPercentage { get; set; }

    public decimal AssessmentScore { get; set; }

    public decimal PassPercentage { get; set; }

    public DateTime CompletionDate { get; set; }

    public DateTime IssuedDate { get; set; }

    public bool IsValid { get; set; }
}
