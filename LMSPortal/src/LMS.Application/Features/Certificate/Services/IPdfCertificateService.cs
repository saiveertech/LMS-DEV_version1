namespace LMS.Application.Features.Certificate.Services;

public interface IPdfCertificateService
{
    byte[] GenerateCertificatePdf(
        string studentName,
        string courseName,
        DateTime completionDate,
        string credentialId,
        string certificateId);
}
