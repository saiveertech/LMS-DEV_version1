ALTER PROCEDURE LMS.SP_RegisterCourse
(
@Title NVARCHAR(200),
@Description NVARCHAR(MAX),
@IntroVideoUrl NVARCHAR(MAX),
@CompletionTimeSeconds INT,
@CourseIconUrl NVARCHAR(MAX),
@Tags NVARCHAR(MAX),
@CreatedById NVARCHAR(50),
@CreatedByName NVARCHAR(200),
@CreatedByRole NVARCHAR(50)
)
AS
BEGIN
INSERT INTO LMS.Courses
(
Title,
Description,
IntroVideoUrl,
CompletionTimeSeconds,
CourseIconUrl,
Tags,
CreatedById,
CreatedByName,
CreatedByRole
)
VALUES
(
@Title,
@Description,
@IntroVideoUrl,
@CompletionTimeSeconds,
@CourseIconUrl,
@Tags,
@CreatedById,
@CreatedByName,
@CreatedByRole
)

SELECT SCOPE_IDENTITY() AS Id
END
GO
