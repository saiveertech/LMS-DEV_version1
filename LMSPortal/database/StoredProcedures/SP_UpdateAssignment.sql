CREATE PROCEDURE LMS.SP_UpdateAssignment
(
    @AssignmentId INT,
    @Title NVARCHAR(200) = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @IntroVideoUrl NVARCHAR(MAX) = NULL,
    @QuestionsCsvUrl NVARCHAR(MAX) = NULL,
    @CompletionTimeSeconds INT = NULL,
    @PassPercentage DECIMAL(5,2) = NULL,
    @WwEnvClientId NVARCHAR(100) = NULL,
    @AssessmentIconUrl NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(MAX) = NULL
)
AS
BEGIN

SET NOCOUNT ON;

UPDATE LMS.Assignments
SET
    Title = ISNULL(@Title, Title),
    Description = ISNULL(@Description, Description),
    IntroVideoUrl = ISNULL(@IntroVideoUrl, IntroVideoUrl),
    QuestionsCsvUrl = ISNULL(@QuestionsCsvUrl, QuestionsCsvUrl),
    CompletionTimeSeconds = ISNULL(@CompletionTimeSeconds, CompletionTimeSeconds),
    PassPercentage = ISNULL(@PassPercentage, PassPercentage),
    WwEnvClientId = ISNULL(@WwEnvClientId, WwEnvClientId),
    AssessmentIconUrl = ISNULL(@AssessmentIconUrl, AssessmentIconUrl),
    Tags = ISNULL(@Tags, Tags),
    UpdatedAt = SYSUTCDATETIME()
WHERE AssignmentId = @AssignmentId
    AND IsDeleted = 0;

SELECT @@ROWCOUNT;

END
GO
