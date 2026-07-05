ALTER PROCEDURE LMS.SP_CreateAssignment
(
@Title NVARCHAR(200),
@Description NVARCHAR(MAX),
@IntroVideoUrl NVARCHAR(MAX),
@QuestionsCsvUrl NVARCHAR(MAX),
@CompletionTimeSeconds INT,
@PassPercentage DECIMAL(5,2),
@WwEnvClientId NVARCHAR(100),
@AssessmentIconUrl NVARCHAR(MAX),
@Tags NVARCHAR(MAX),
@CreatedById NVARCHAR(50),
@CreatedByName NVARCHAR(200),
@CreatedByRole NVARCHAR(50)
)
AS
BEGIN
SET NOCOUNT ON;

INSERT INTO LMS.Assignments
(
Title,
Description,
IntroVideoUrl,
QuestionsCsvUrl,
CompletionTimeSeconds,
PassPercentage,
WwEnvClientId,
AssessmentIconUrl,
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
@QuestionsCsvUrl,
@CompletionTimeSeconds,
@PassPercentage,
@WwEnvClientId,
@AssessmentIconUrl,
@Tags,
@CreatedById,
@CreatedByName,
@CreatedByRole
)

SELECT SCOPE_IDENTITY() AS AssignmentId
END
GO
