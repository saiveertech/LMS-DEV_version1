CREATE PROCEDURE LMS.SP_GetStudentById
(
@StudentId NVARCHAR(50)
)
AS
BEGIN


SELECT *
FROM LMS.Students
WHERE StudentId = @StudentId


END
GO
