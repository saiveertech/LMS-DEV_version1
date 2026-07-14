-- Audit-only insert: called right after SP_EnrollStudentAssignment when a
-- trainer/admin (not the student) initiated the enrollment.
CREATE PROCEDURE LMS.SP_RecordAssignmentAllocation
(
    @StudentId      NVARCHAR(50),
    @AssignmentId   INT,
    @AssignedById   NVARCHAR(50),
    @AssignedByName NVARCHAR(200),
    @AssignedByRole NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM LMS.AssignmentAllocations
        WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId
    )
    BEGIN
        RAISERROR('This assignment allocation has already been recorded.', 16, 1);
        RETURN;
    END

    INSERT INTO LMS.AssignmentAllocations
    (
        StudentId, AssignmentId, AssignedById, AssignedByName, AssignedByRole
    )
    VALUES
    (
        @StudentId, @AssignmentId, @AssignedById, @AssignedByName, @AssignedByRole
    );

END
GO
