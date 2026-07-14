-- Audit-only insert: called right after SP_EnrollStudentCourse when a
-- trainer/admin (not the student) initiated the enrollment.
CREATE PROCEDURE LMS.SP_RecordCourseAssignment
(
    @StudentId     NVARCHAR(50),
    @CourseId      INT,
    @AssignedById  NVARCHAR(50),
    @AssignedByName NVARCHAR(200),
    @AssignedByRole NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM LMS.CourseAssignments
        WHERE StudentId = @StudentId AND CourseId = @CourseId
    )
    BEGIN
        RAISERROR('This course assignment has already been recorded.', 16, 1);
        RETURN;
    END

    INSERT INTO LMS.CourseAssignments
    (
        StudentId, CourseId, AssignedById, AssignedByName, AssignedByRole
    )
    VALUES
    (
        @StudentId, @CourseId, @AssignedById, @AssignedByName, @AssignedByRole
    );

END
GO
