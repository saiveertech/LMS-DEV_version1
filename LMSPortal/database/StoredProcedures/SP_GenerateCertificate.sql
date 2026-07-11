CREATE PROCEDURE LMS.SP_GenerateCertificate
(
    @StudentId          NVARCHAR(50),
    @StudentName        NVARCHAR(200),
    @StudentEmail       NVARCHAR(200),
    @CourseId            INT,
    @CourseName         NVARCHAR(200),
    @AssignmentId       INT,
    @CompletionPercentage DECIMAL(5,2),
    @AssessmentScore    DECIMAL(5,2),
    @PassPercentage     DECIMAL(5,2),
    @CompletionDate     DATETIME2,
    @CertificateUrl     NVARCHAR(MAX),
    @CreatedById        NVARCHAR(50),
    @CreatedByName      NVARCHAR(200),
    @CreatedByRole      NVARCHAR(50),
    @CertificateId      NVARCHAR(50) OUTPUT,
    @CredentialId       NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate Student exists
    IF NOT EXISTS (
        SELECT 1 FROM LMS.Students WHERE StudentId = @StudentId
    )
    BEGIN
        RAISERROR('Student not found.', 16, 1);
        RETURN;
    END

    -- Validate Course exists and is active
    IF NOT EXISTS (
        SELECT 1 FROM LMS.Courses WHERE Id = @CourseId AND IsActive = 1 AND IsDeleted = 0
    )
    BEGIN
        RAISERROR('Course not found or inactive.', 16, 1);
        RETURN;
    END

    -- Validate Assignment exists
    IF NOT EXISTS (
        SELECT 1 FROM LMS.Assignments WHERE AssignmentId = @AssignmentId AND IsDeleted = 0
    )
    BEGIN
        RAISERROR('Assignment not found.', 16, 1);
        RETURN;
    END

    -- Prevent duplicate certificate for same student + course
    IF EXISTS (
        SELECT 1 FROM LMS.Certificates
        WHERE StudentId = @StudentId AND CourseId = @CourseId
    )
    BEGIN
        RAISERROR('Certificate already issued for this student and course.', 16, 1);
        RETURN;
    END

    -- Validate completion = 100%
    IF @CompletionPercentage < 100.00
    BEGIN
        RAISERROR('Course completion must be 100%%.', 16, 1);
        RETURN;
    END

    -- Validate assessment score >= pass percentage
    IF @AssessmentScore < @PassPercentage
    BEGIN
        RAISERROR('Assessment score is below the pass percentage.', 16, 1);
        RETURN;
    END

    -- Generate placeholder IDs
    DECLARE @Placeholder1 NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @Placeholder2 NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @NewId INT;

    INSERT INTO LMS.Certificates
    (
        CertificateId,
        CredentialId,
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
        CertificateUrl,
        CreatedById,
        CreatedByName,
        CreatedByRole
    )
    VALUES
    (
        @Placeholder1,
        @Placeholder2,
        @StudentId,
        @StudentName,
        @StudentEmail,
        @CourseId,
        @CourseName,
        @AssignmentId,
        @CompletionPercentage,
        @AssessmentScore,
        @PassPercentage,
        @CompletionDate,
        @CertificateUrl,
        @CreatedById,
        @CreatedByName,
        @CreatedByRole
    );

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

    -- Build readable CertificateId: CERT-0001
    SET @CertificateId =
        'CERT-' +
        RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);

    -- Build readable CredentialId: CRED-SK-0001
    SET @CredentialId =
        'CRED-SK-' +
        RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);

    -- Update with real IDs
    UPDATE LMS.Certificates
    SET CertificateId = @CertificateId,
        CredentialId  = @CredentialId
    WHERE Id = @NewId;

    -- Update enrollment certificate status
    UPDATE LMS.StudentCourseEnrollments
    SET CertificateStatus   = 'Issued',
        CertificateIssueDate = SYSUTCDATETIME(),
        CourseStatus         = 'Completed',
        RegistrationStatus   = 'Completed',
        AssessmentScore      = @AssessmentScore,
        UpdatedDate          = SYSUTCDATETIME()
    WHERE StudentId = @StudentId
      AND CourseId  = @CourseId;

    -- Return the created certificate
    SELECT
        Id,
        CertificateId,
        CredentialId,
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
    WHERE Id = @NewId;

END
GO
