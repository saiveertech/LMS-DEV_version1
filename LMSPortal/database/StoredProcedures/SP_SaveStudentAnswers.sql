-- Records one attempt's worth of answers (already evaluated in C# against
-- the answer key). Attempt number is self-determined here so the caller
-- doesn't need a separate round trip to figure it out.
CREATE PROCEDURE LMS.SP_SaveStudentAnswers
(
    @StudentId    NVARCHAR(50),
    @AssignmentId INT,
    @AnswersJson  NVARCHAR(MAX)
    -- Each element: { "QuestionId": int, "SelectedOption": "A", "IsCorrect": true/false }
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AttemptNumber INT = ISNULL(
        (SELECT MAX(AttemptNumber) FROM LMS.StudentAnswers
         WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId),
        0) + 1;

    INSERT INTO LMS.StudentAnswers
    (
        StudentId, AssignmentId, AttemptNumber, QuestionId, SelectedOption, IsCorrect
    )
    SELECT
        @StudentId,
        @AssignmentId,
        @AttemptNumber,
        CAST(JSON_VALUE(value, '$.QuestionId') AS INT),
        JSON_VALUE(value, '$.SelectedOption'),
        CAST(JSON_VALUE(value, '$.IsCorrect') AS BIT)
    FROM OPENJSON(@AnswersJson);

    SELECT @AttemptNumber AS AttemptNumber;

END
GO
