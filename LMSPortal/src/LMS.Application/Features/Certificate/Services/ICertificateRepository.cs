using LMS.Application.Features.Certificate.DTOs;

namespace LMS.Application.Features.Certificate.Services;

public interface ICertificateRepository
{
    Task<CertificateResponse?> GenerateCertificate(
        GenerateCertificateRequest request,
        string certificateUrl,
        string studentName,
        string studentEmail,
        string courseName,
        decimal passPercentage,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<List<CertificateResponse>> GetStudentCertificates(string studentId);

    Task<CertificateResponse?> GetCertificateById(string certificateId);

    Task<VerifyCertificateResponse?> VerifyCertificate(string certificateId);
}
