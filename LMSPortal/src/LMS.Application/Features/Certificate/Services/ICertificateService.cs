using LMS.Application.Common;
using LMS.Application.Features.Certificate.DTOs;

namespace LMS.Application.Features.Certificate.Services;

public interface ICertificateService
{
    Task<ServiceResponse> GenerateCertificate(
        GenerateCertificateRequest request,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<List<CertificateResponse>> GetStudentCertificates(string studentId);

    Task<CertificateResponse?> GetCertificateById(string certificateId);

    Task<ServiceResponse> VerifyCertificate(string certificateId);
}
