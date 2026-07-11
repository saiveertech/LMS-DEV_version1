ALTER PROCEDURE LMS.SP_UpdateCourse
(
    @Id INT,
    @Title NVARCHAR(200) = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @IntroVideoUrl NVARCHAR(MAX) = NULL,
    @CompletionTimeSeconds INT = NULL,
    @CourseIconUrl NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(MAX) = NULL,
    @CourseStatus NVARCHAR(50) = NULL,
    @IsActive BIT = NULL,
    @EditedById NVARCHAR(50),
    @EditedByName NVARCHAR(200),
    @EditedByRole NVARCHAR(50)
)
AS
BEGIN

SET NOCOUNT ON;

UPDATE LMS.Courses
SET
    Title = ISNULL(@Title, Title),
    Description = ISNULL(@Description, Description),
    IntroVideoUrl = ISNULL(@IntroVideoUrl, IntroVideoUrl),
    CompletionTimeSeconds = ISNULL(@CompletionTimeSeconds, CompletionTimeSeconds),
    CourseIconUrl = ISNULL(@CourseIconUrl, CourseIconUrl),
    Tags = ISNULL(@Tags, Tags),
    CourseStatus = ISNULL(@CourseStatus, CourseStatus),
    IsActive = ISNULL(@IsActive, IsActive),
    EditedById = @EditedById,
    EditedByName = @EditedByName,
    EditedByRole = @EditedByRole,
    UpdatedDate = SYSUTCDATETIME()
WHERE Id = @Id
    AND IsDeleted = 0;

SELECT @@ROWCOUNT;

END
GO
