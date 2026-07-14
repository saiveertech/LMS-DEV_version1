-- Auto-issued by the system the moment a course enrollment reaches 100%
-- completion (see SP_UpdateCourseStudentProgress) — never called manually.
CREATE PROCEDURE LMS.SP_GenerateCourseCertificate
(
    @StudentId            NVARCHAR(50),
    @StudentName          NVARCHAR(200),
    @StudentEmail         NVARCHAR(200),
    @CourseId             INT,
    @CourseName           NVARCHAR(200),
    @CompletionPercentage DECIMAL(5,2),
    @CompletionDate       DATETIME2,
    @CertificateUrl       NVARCHAR(MAX),
    @CreatedById          NVARCHAR(50),
    @CreatedByName        NVARCHAR(200),
    @CreatedByRole        NVARCHAR(50),
    @CertificateId        NVARCHAR(50) OUTPUT,
    @CredentialId         NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM LMS.Certificates
        WHERE StudentId = @StudentId AND CourseId = @CourseId AND CertificateType = 'Course'
    )
    BEGIN
        RAISERROR('Course certificate already issued for this student.', 16, 1);
        RETURN;
    END

    IF @CompletionPercentage < 100.00
    BEGIN
        RAISERROR('Course completion must be 100%% to issue a certificate.', 16, 1);
        RETURN;
    END

    DECLARE @Placeholder1 NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @Placeholder2 NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @NewId INT;

    INSERT INTO LMS.Certificates
    (
        CertificateId,
        CredentialId,
        CertificateType,
        StudentId,
        StudentName,
        StudentEmail,
        CourseId,
        CourseName,
        CompletionPercentage,
        CompletionDate,
        CertificateUrl,
        CreatedById,
        CreatedByName,
        CreatedByRole
    )
    VALUES
    (
        @Placeholder1,
        @Placeholder2,
        'Course',
        @StudentId,
        @StudentName,
        @StudentEmail,
        @CourseId,
        @CourseName,
        @CompletionPercentage,
        @CompletionDate,
        @CertificateUrl,
        @CreatedById,
        @CreatedByName,
        @CreatedByRole
    );

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

    SET @CertificateId = 'CERT-' + RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);
    SET @CredentialId  = 'CRED-CO-' + RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);

    UPDATE LMS.Certificates
    SET CertificateId = @CertificateId,
        CredentialId  = @CredentialId
    WHERE Id = @NewId;

    UPDATE LMS.StudentCourseEnrollments
    SET CertificateStatus    = 'Issued',
        CertificateIssueDate = SYSUTCDATETIME(),
        UpdatedDate          = SYSUTCDATETIME()
    WHERE StudentId = @StudentId
      AND CourseId  = @CourseId;

    SELECT
        Id, CertificateId, CredentialId, CertificateType,
        StudentId, StudentName, StudentEmail,
        CourseId, CourseName, AssignmentId,
        CompletionPercentage, AssessmentScore, PassPercentage,
        CompletionDate, IssuedDate, CertificateUrl, IsValid,
        CreatedById, CreatedByName, CreatedByRole, CreatedAt, UpdatedDate
    FROM LMS.Certificates
    WHERE Id = @NewId;

END
GO
