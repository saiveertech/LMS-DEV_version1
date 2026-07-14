-- Auto-issued by the system the moment a student passes an assignment
-- (see SP_SubmitAssignmentAttempt) — never called manually.
CREATE PROCEDURE LMS.SP_GenerateAssignmentCertificate
(
    @StudentId       NVARCHAR(50),
    @StudentName     NVARCHAR(200),
    @StudentEmail    NVARCHAR(200),
    @AssignmentId    INT,
    @AssignmentTitle NVARCHAR(200),
    @AssessmentScore DECIMAL(5,2),
    @PassPercentage  DECIMAL(5,2),
    @CompletionDate  DATETIME2,
    @CertificateUrl  NVARCHAR(MAX),
    @CreatedById     NVARCHAR(50),
    @CreatedByName   NVARCHAR(200),
    @CreatedByRole   NVARCHAR(50),
    @CertificateId   NVARCHAR(50) OUTPUT,
    @CredentialId    NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM LMS.Certificates
        WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId AND CertificateType = 'Assignment'
    )
    BEGIN
        RAISERROR('Assignment certificate already issued for this student.', 16, 1);
        RETURN;
    END

    IF @AssessmentScore < @PassPercentage
    BEGIN
        RAISERROR('Assessment score is below the pass percentage.', 16, 1);
        RETURN;
    END

    DECLARE @Placeholder1 NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @Placeholder2 NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @NewId INT;

    -- Reuses the CourseName column to hold the assignment's title — the
    -- column just backs the certificate's display text, not course-specific
    -- semantics; avoids a parallel AssignmentTitle column for one field.
    INSERT INTO LMS.Certificates
    (
        CertificateId,
        CredentialId,
        CertificateType,
        StudentId,
        StudentName,
        StudentEmail,
        AssignmentId,
        CourseName,
        AssessmentScore,
        PassPercentage,
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
        'Assignment',
        @StudentId,
        @StudentName,
        @StudentEmail,
        @AssignmentId,
        @AssignmentTitle,
        @AssessmentScore,
        @PassPercentage,
        @CompletionDate,
        @CertificateUrl,
        @CreatedById,
        @CreatedByName,
        @CreatedByRole
    );

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

    SET @CertificateId = 'CERT-' + RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);
    SET @CredentialId  = 'CRED-AS-' + RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);

    UPDATE LMS.Certificates
    SET CertificateId = @CertificateId,
        CredentialId  = @CredentialId
    WHERE Id = @NewId;

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
