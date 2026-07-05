CREATE PROCEDURE LMS.SP_DeleteAssignment
(
@AssignmentId INT
)
AS
BEGIN

SET NOCOUNT ON;

UPDATE LMS.Assignments
SET
    IsDeleted = 1,
    UpdatedAt = SYSUTCDATETIME()
WHERE AssignmentId = @AssignmentId
    AND IsDeleted = 0;

SELECT @@ROWCOUNT;

END
GO
