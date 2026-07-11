namespace LMS.Application.Features.Certificate.Services;

public interface ICertificateTemplateRenderer
{
    Task<byte[]> RenderPngAsync(
        string studentName,
        string learningPathName,
        DateTime completionDate,
        string credentialId,
        string certificateNumber);

    byte[] WrapPngInPdf(byte[] pngBytes);
}
