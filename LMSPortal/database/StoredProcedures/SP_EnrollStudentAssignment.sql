CREATE PROCEDURE LMS.SP_EnrollStudentAssignment
(
    @StudentId    NVARCHAR(50),
    @AssignmentId INT,
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

    -- Validate Assignment exists
    IF NOT EXISTS (
        SELECT 1 FROM LMS.Assignments WHERE AssignmentId = @AssignmentId AND IsDeleted = 0
    )
    BEGIN
        RAISERROR('Assignment Not Found.', 16, 1);
        RETURN;
    END

    -- Prevent duplicate enrollment
    IF EXISTS (
        SELECT 1 FROM LMS.StudentAssignmentEnrollments
        WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId
    )
    BEGIN
        RAISERROR('Student is already enrolled in this assignment.', 16, 1);
        RETURN;
    END

    -- Generate a placeholder for EnrollmentId
    DECLARE @Placeholder NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @NewId INT;

    INSERT INTO LMS.StudentAssignmentEnrollments
    (
        EnrollmentId,
        StudentId,
        AssignmentId,
        AssignmentStatus
    )
    VALUES
    (
        @Placeholder,
        @StudentId,
        @AssignmentId,
        'Enrolled'
    );

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

    -- Build a readable EnrollmentId: EA + StudentId prefix + zero-padded Id
    SET @EnrollmentId =
        'EA' +
        UPPER(LEFT(@StudentId, 4)) +
        RIGHT('0000' + CAST(@NewId AS VARCHAR(10)), 4);

    -- Update with the real EnrollmentId
    UPDATE LMS.StudentAssignmentEnrollments
    SET EnrollmentId = @EnrollmentId
    WHERE Id = @NewId;

END
GO
