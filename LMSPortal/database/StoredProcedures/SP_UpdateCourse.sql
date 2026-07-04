CREATE PROCEDURE LMS.SP_UpdateCourse
(
    @Id INT,
    @Title NVARCHAR(200) = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @IntroVideoUrl NVARCHAR(MAX) = NULL,
    @SlidesJson NVARCHAR(MAX) = NULL,
    @CompletionTimeSeconds INT = NULL,
    @PassPercentage DECIMAL(5,2) = NULL,
    @WwEnvClientId NVARCHAR(100) = NULL,
    @CourseIconUrl NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(MAX) = NULL,
    @CourseStatus NVARCHAR(50) = NULL,
    @IsActive BIT = NULL
)
AS
BEGIN

SET NOCOUNT ON;

UPDATE LMS.Courses
SET
    Title = ISNULL(@Title, Title),
    Description = ISNULL(@Description, Description),
    IntroVideoUrl = ISNULL(@IntroVideoUrl, IntroVideoUrl),
    SlidesJson = ISNULL(@SlidesJson, SlidesJson),
    CompletionTimeSeconds = ISNULL(@CompletionTimeSeconds, CompletionTimeSeconds),
    PassPercentage = ISNULL(@PassPercentage, PassPercentage),
    WwEnvClientId = ISNULL(@WwEnvClientId, WwEnvClientId),
    CourseIconUrl = ISNULL(@CourseIconUrl, CourseIconUrl),
    Tags = ISNULL(@Tags, Tags),
    CourseStatus = ISNULL(@CourseStatus, CourseStatus),
    IsActive = ISNULL(@IsActive, IsActive),
    UpdatedDate = SYSUTCDATETIME()
WHERE Id = @Id;

SELECT @@ROWCOUNT;

END
GO
