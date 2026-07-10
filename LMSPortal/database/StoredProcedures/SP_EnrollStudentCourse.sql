CREATE PROCEDURE LMS.SP_EnrollStudentCourse
(
    @StudentId    NVARCHAR(50),
    @CourseId     INT,
    @EnrollmentId NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate Student exists
    IF NOT EXISTS (
        SELECT 1 FROM LMS.Students WHERE StudentId = @StudentId
    )
    BEGIN
        RAISERROR('Student Not Found.', 16, 1);
        RETURN;
    END

    -- Validate Course exists and is active
    IF NOT EXISTS (
        SELECT 1 FROM LMS.Courses WHERE Id = @CourseId AND IsActive = 1 AND IsDeleted = 0
    )
    BEGIN
        RAISERROR('Course Not Found or Inactive.', 16, 1);
        RETURN;
    END

    -- Prevent duplicate enrollment
    IF EXISTS (
        SELECT 1 FROM LMS.StudentCourseEnrollments
        WHERE StudentId = @StudentId AND CourseId = @CourseId
    )
    BEGIN
        RAISERROR('Student is already enrolled in this course.', 16, 1);
        RETURN;
    END

    -- Generate a placeholder for EnrollmentId
    DECLARE @Placeholder NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @NewId INT;

    INSERT INTO LMS.StudentCourseEnrollments
    (
        EnrollmentId,
        StudentId,
        CourseId,
        CourseStatus,
        CertificateStatus
    )
    VALUES
    (
        @Placeholder,
        @StudentId,
        @CourseId,
        'Enrolled',
        'Pending'
    );

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

    -- Build a readable EnrollmentId: EN + StudentId prefix + zero-padded Id
    SET @EnrollmentId =
        'EN' +
        UPPER(LEFT(@StudentId, 4)) +
        RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);

    -- Update with the real EnrollmentId
    UPDATE LMS.StudentCourseEnrollments
    SET EnrollmentId = @EnrollmentId
    WHERE Id = @NewId;

END
GO
