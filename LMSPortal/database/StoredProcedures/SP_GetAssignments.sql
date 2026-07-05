CREATE PROCEDURE LMS.SP_GetAssignments
(
@AssignmentId INT = NULL
)
AS
BEGIN

SET NOCOUNT ON;

SELECT *
FROM LMS.Assignments
WHERE (@AssignmentId IS NULL OR AssignmentId = @AssignmentId)
    AND IsDeleted = 0
ORDER BY CreatedAt DESC

END
GO
