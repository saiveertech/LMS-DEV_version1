-- Trainer/admin-assigned courses for one student, sourced from the
-- CourseAssignments audit table (never populated for self-registrations).
CREATE PROCEDURE LMS.SP_GetCourseAssignmentsByStudent
(
    @StudentId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ca.Id                        AS AssignmentRecordId,
        ca.StudentId,
        ca.CourseId,
        c.Title                      AS CourseTitle,
        c.Description                AS CourseDescription,
        c.CourseIconUrl,
        c.Tags,
        c.CompletionTimeSeconds,
        ca.AssignedById,
        ca.AssignedByName,
        ca.AssignedByRole,
        ca.AssignedDate
    FROM LMS.CourseAssignments ca
    INNER JOIN LMS.Courses c
        ON ca.CourseId = c.Id
    WHERE ca.StudentId = @StudentId
    ORDER BY ca.AssignedDate DESC;

END
GO
