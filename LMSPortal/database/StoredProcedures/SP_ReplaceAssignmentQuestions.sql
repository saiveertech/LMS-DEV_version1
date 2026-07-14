-- Called automatically when a trainer uploads/re-uploads an assignment's
-- QuestionsCsv (at create or update time) — replaces the whole question
-- bank for that assignment with the freshly parsed CSV content.
CREATE PROCEDURE LMS.SP_ReplaceAssignmentQuestions
(
    @AssignmentId  INT,
    @QuestionsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Once a student has answered against the current question set, the
    -- answer key is locked — replacing it would orphan (or, without this
    -- guard, fail to delete via FK) their submitted StudentAnswers rows.
    -- Trainers must create a new assignment instead of editing this one.
    IF EXISTS (
        SELECT 1 FROM LMS.StudentAnswers WHERE AssignmentId = @AssignmentId
    )
    BEGIN
        RAISERROR('Cannot change the answer key — students have already submitted answers for this assignment.', 16, 1);
        RETURN;
    END

    DELETE FROM LMS.AssignmentQuestions WHERE AssignmentId = @AssignmentId;

    INSERT INTO LMS.AssignmentQuestions
    (
        AssignmentId, QuestionText, OptionA, OptionB, OptionC, OptionD,
        CorrectOption, Marks, SortOrder
    )
    SELECT
        @AssignmentId,
        JSON_VALUE(value, '$.QuestionText'),
        JSON_VALUE(value, '$.OptionA'),
        JSON_VALUE(value, '$.OptionB'),
        JSON_VALUE(value, '$.OptionC'),
        JSON_VALUE(value, '$.OptionD'),
        JSON_VALUE(value, '$.CorrectOption'),
        CAST(JSON_VALUE(value, '$.Marks') AS DECIMAL(5,2)),
        CAST([key] AS INT)
    FROM OPENJSON(@QuestionsJson);

    SELECT COUNT(*) AS ImportedCount FROM LMS.AssignmentQuestions WHERE AssignmentId = @AssignmentId;

END
GO
