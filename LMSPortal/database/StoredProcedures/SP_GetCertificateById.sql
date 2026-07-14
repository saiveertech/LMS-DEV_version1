CREATE PROCEDURE LMS.SP_GetCertificateById
(
    @CertificateId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        CertificateId,
        CredentialId,
        CertificateType,
        StudentId,
        StudentName,
        StudentEmail,
        CourseId,
        CourseName,
        AssignmentId,
        CompletionPercentage,
        AssessmentScore,
        PassPercentage,
        CompletionDate,
        IssuedDate,
        CertificateUrl,
        IsValid,
        CreatedById,
        CreatedByName,
        CreatedByRole,
        CreatedAt,
        UpdatedDate
    FROM LMS.Certificates
    WHERE CertificateId = @CertificateId;

END
GO
