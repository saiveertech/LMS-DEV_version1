CREATE PROCEDURE LMS.SP_GetStudentCertificates
(
    @StudentId NVARCHAR(50)
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
    WHERE StudentId = @StudentId
    ORDER BY IssuedDate DESC;

END
GO
