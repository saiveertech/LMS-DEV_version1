ALTER PROCEDURE LMS.SP_GetCourseById
(
@Id INT = NULL
)
AS
BEGIN

SET NOCOUNT ON;

SELECT *
FROM LMS.Courses
WHERE (@Id IS NULL OR Id = @Id)
    AND IsDeleted = 0
ORDER BY CreatedAt DESC;

SELECT cs.*
FROM LMS.CourseSlides cs
INNER JOIN LMS.Courses c ON c.Id = cs.CourseId
WHERE (@Id IS NULL OR cs.CourseId = @Id)
    AND c.IsDeleted = 0
ORDER BY cs.CourseId, cs.SortOrder;

END
GO
