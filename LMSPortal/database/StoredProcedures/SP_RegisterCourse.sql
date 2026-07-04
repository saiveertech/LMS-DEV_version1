CREATE PROCEDURE LMS.SP_RegisterCourse
(
@Title NVARCHAR(200),
@Description NVARCHAR(MAX),
@IntroVideoUrl NVARCHAR(MAX),
@SlidesJson NVARCHAR(MAX),
@CompletionTimeSeconds INT,
@PassPercentage DECIMAL(5,2),
@WwEnvClientId NVARCHAR(100),
@CourseIconUrl NVARCHAR(MAX),
@Tags NVARCHAR(MAX),
@CreatedBy NVARCHAR(200),
@CreatedByRole NVARCHAR(50)
)
AS
BEGIN
INSERT INTO LMS.Courses
(
Title,
Description,
IntroVideoUrl,
SlidesJson,
CompletionTimeSeconds,
PassPercentage,
WwEnvClientId,
CourseIconUrl,
Tags,
CreatedBy,
CreatedByRole
)
VALUES
(
@Title,
@Description,
@IntroVideoUrl,
@SlidesJson,
@CompletionTimeSeconds,
@PassPercentage,
@WwEnvClientId,
@CourseIconUrl,
@Tags,
@CreatedBy,
@CreatedByRole
)

SELECT SCOPE_IDENTITY() AS Id
END
GO
