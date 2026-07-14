-- Returns the full question bank including CorrectOption. Callers that must
-- not leak the answer (the student-facing "take the quiz" view) strip
-- CorrectOption out in the C# service layer before returning it.
CREATE PROCEDURE LMS.SP_GetAssignmentQuestions
(
    @AssignmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id AS QuestionId,
        AssignmentId,
        QuestionText,
        OptionA,
        OptionB,
        OptionC,
        OptionD,
        CorrectOption,
        Marks,
        SortOrder
    FROM LMS.AssignmentQuestions
    WHERE AssignmentId = @AssignmentId
    ORDER BY SortOrder;

END
GO
