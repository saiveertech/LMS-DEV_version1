-- Recommends assignments the student isn't already enrolled in / assigned to,
-- ranked by the same tag-overlap scoring as SP_GetRecommendedCoursesForStudent:
--   * InterestScore  - overlap between the student's AreaOfInterest and the assignment's Tags
--   * HistoryScore   - overlap between the assignment's Tags and the tags of assignments the
--                       student has previously enrolled in or been assigned (self + trainer)
-- MatchScore = InterestScore * 3 + HistoryScore. Assignments with no overlap are still
-- returned (MatchScore = 0), ordered behind matches, newest first.
CREATE PROCEDURE LMS.SP_GetRecommendedAssignmentsForStudent
(
    @StudentId NVARCHAR(50),
    @TopN      INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Interest NVARCHAR(MAX);

    SELECT @Interest = AreaOfInterest
    FROM LMS.Students
    WHERE StudentId = @StudentId;

    ;WITH HistoryAssignmentIds AS (
        SELECT AssignmentId FROM LMS.StudentAssignmentEnrollments WHERE StudentId = @StudentId
        UNION
        SELECT AssignmentId FROM LMS.AssignmentAllocations WHERE StudentId = @StudentId
    ),
    InterestTags AS (
        SELECT DISTINCT LTRIM(RTRIM(value)) AS Tag
        FROM STRING_SPLIT(ISNULL(@Interest, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    HistoryTags AS (
        SELECT DISTINCT LTRIM(RTRIM(t.value)) AS Tag
        FROM LMS.Assignments a
        INNER JOIN HistoryAssignmentIds h ON h.AssignmentId = a.AssignmentId
        CROSS APPLY STRING_SPLIT(ISNULL(a.Tags, ''), ',') t
        WHERE LTRIM(RTRIM(t.value)) <> ''
    ),
    AssignmentTags AS (
        SELECT a.AssignmentId, LTRIM(RTRIM(t.value)) AS Tag
        FROM LMS.Assignments a
        CROSS APPLY STRING_SPLIT(ISNULL(a.Tags, ''), ',') t
        WHERE LTRIM(RTRIM(t.value)) <> ''
    ),
    Scored AS (
        SELECT
            at.AssignmentId,
            COUNT(DISTINCT it.Tag) AS InterestScore,
            COUNT(DISTINCT ht.Tag) AS HistoryScore
        FROM AssignmentTags at
        LEFT JOIN InterestTags it ON it.Tag = at.Tag
        LEFT JOIN HistoryTags  ht ON ht.Tag = at.Tag
        GROUP BY at.AssignmentId
    )
    SELECT TOP (@TopN)
        a.AssignmentId,
        a.Title                             AS AssignmentTitle,
        a.Description                       AS AssignmentDescription,
        a.AssessmentIconUrl,
        a.Tags,
        a.CompletionTimeSeconds,
        a.PassPercentage,
        ISNULL(s.InterestScore, 0)          AS InterestScore,
        ISNULL(s.HistoryScore, 0)           AS HistoryScore,
        (ISNULL(s.InterestScore, 0) * 3 + ISNULL(s.HistoryScore, 0)) AS MatchScore
    FROM LMS.Assignments a
    LEFT JOIN Scored s ON s.AssignmentId = a.AssignmentId
    WHERE a.IsDeleted = 0
      AND a.AssignmentId NOT IN (SELECT AssignmentId FROM HistoryAssignmentIds)
    ORDER BY MatchScore DESC, a.CreatedAt DESC;

END
GO
