CREATE PROCEDURE LMS.SP_GetStudentEnrolledAssignments
(
    @StudentId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EnrollmentId,
        e.StudentId,
        e.AssignmentId,
        a.Title                    AS AssignmentTitle,
        a.Description              AS AssignmentDescription,
        a.AssessmentIconUrl,
        a.Tags,
        a.CompletionTimeSeconds,
        a.PassPercentage,
        e.EnrollmentDate,
        e.AssignmentStatus,
        e.AssessmentScore,
        e.CreatedAt,
        e.UpdatedDate
    FROM LMS.StudentAssignmentEnrollments e
    INNER JOIN LMS.Assignments a
        ON e.AssignmentId = a.AssignmentId
    WHERE e.StudentId = @StudentId
    ORDER BY e.EnrollmentDate DESC;

END
GO
