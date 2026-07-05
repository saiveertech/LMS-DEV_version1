CREATE PROCEDURE LMS.SP_DeleteCourse
(
    @Id INT,
    @DeletedById NVARCHAR(50),
    @DeletedByName NVARCHAR(200),
    @DeletedByRole NVARCHAR(50)
)
AS
BEGIN

SET NOCOUNT ON;

UPDATE LMS.Courses
SET
    IsDeleted = 1,
    DeletedById = @DeletedById,
    DeletedByName = @DeletedByName,
    DeletedByRole = @DeletedByRole,
    DeletedAt = SYSUTCDATETIME()
WHERE Id = @Id
    AND IsDeleted = 0;

SELECT @@ROWCOUNT;

END
GO
