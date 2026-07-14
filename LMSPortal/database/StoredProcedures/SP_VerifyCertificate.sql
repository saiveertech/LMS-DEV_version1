CREATE PROCEDURE LMS.SP_VerifyCertificate
(
    @CertificateId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CertificateId,
        CredentialId,
        CertificateType,
        StudentName,
        CourseName,
        CompletionPercentage,
        AssessmentScore,
        PassPercentage,
        CompletionDate,
        IssuedDate,
        IsValid
    FROM LMS.Certificates
    WHERE CertificateId = @CertificateId;

END
GO
