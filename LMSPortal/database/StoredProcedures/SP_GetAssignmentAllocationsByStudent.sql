-- Trainer/admin-assigned assignments for one student, sourced from the
-- AssignmentAllocations audit table (never populated for self-registrations).
CREATE PROCEDURE LMS.SP_GetAssignmentAllocationsByStudent
(
    @StudentId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        aa.Id                        AS AllocationRecordId,
        aa.StudentId,
        aa.AssignmentId,
        a.Title                      AS AssignmentTitle,
        a.Description                AS AssignmentDescription,
        a.AssessmentIconUrl,
        a.Tags,
        a.CompletionTimeSeconds,
        a.PassPercentage,
        aa.AssignedById,
        aa.AssignedByName,
        aa.AssignedByRole,
        aa.AssignedDate
    FROM LMS.AssignmentAllocations aa
    INNER JOIN LMS.Assignments a
        ON aa.AssignmentId = a.AssignmentId
    WHERE aa.StudentId = @StudentId
    ORDER BY aa.AssignedDate DESC;

END
GO
