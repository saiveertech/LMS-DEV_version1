CREATE PROCEDURE LMS.SP_SubmitAssignmentAttempt
(
    @StudentId    NVARCHAR(50),
    @AssignmentId INT,
    @Score        DECIMAL(5,2)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM LMS.StudentAssignmentEnrollments
        WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId
    )
    BEGIN
        RAISERROR('Student is not enrolled in this assignment.', 16, 1);
        RETURN;
    END

    IF @Score < 0 OR @Score > 100
    BEGIN
        RAISERROR('Score must be between 0 and 100.', 16, 1);
        RETURN;
    END

    DECLARE @PassPercentage DECIMAL(5,2);
    DECLARE @BestScore      DECIMAL(5,2);

    SELECT @PassPercentage = PassPercentage
    FROM LMS.Assignments
    WHERE AssignmentId = @AssignmentId;

    SELECT @BestScore = AssessmentScore
    FROM LMS.StudentAssignmentEnrollments
    WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId;

    -- AssessmentScore always holds the best attempt ever submitted, and a
    -- pass, once achieved, is never downgraded by a later worse attempt.
    SET @BestScore = CASE
                          WHEN @BestScore IS NULL OR @Score > @BestScore THEN @Score
                          ELSE @BestScore
                      END;

    UPDATE LMS.StudentAssignmentEnrollments
    SET Attempts         = Attempts + 1,
        AssessmentScore  = @BestScore,
        AssignmentStatus = CASE
                                WHEN @BestScore >= @PassPercentage THEN 'Completed'
                                ELSE 'In Progress'
                            END,
        UpdatedDate      = SYSUTCDATETIME()
    WHERE StudentId = @StudentId
      AND AssignmentId = @AssignmentId;

    SELECT
        e.EnrollmentId,
        e.StudentId,
        e.AssignmentId,
        a.Title                    AS AssignmentTitle,
        a.PassPercentage,
        e.EnrollmentDate,
        e.AssignmentStatus,
        e.AssessmentScore,
        e.Attempts,
        e.CreatedAt,
        e.UpdatedDate
    FROM LMS.StudentAssignmentEnrollments e
    INNER JOIN LMS.Assignments a
        ON e.AssignmentId = a.AssignmentId
    WHERE e.StudentId = @StudentId
      AND e.AssignmentId = @AssignmentId;

END
GO
