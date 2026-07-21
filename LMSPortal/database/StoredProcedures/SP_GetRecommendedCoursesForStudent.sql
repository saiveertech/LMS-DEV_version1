-- Recommends active courses the student isn't already enrolled in / assigned
-- to, ranked by a tag-overlap score built from three signals:
--   * InterestScore  - overlap between the student's AreaOfInterest and the course's Tags
--   * HistoryScore   - overlap between the course's Tags and the tags of courses the
--                       student has previously enrolled in or been assigned (self + trainer)
-- MatchScore = InterestScore * 3 + HistoryScore weighting interest higher than history,
-- since history only signals "similar to what they've already done" while interest is
-- an explicit signal of what they want. Courses with no overlap at all are still
-- returned (MatchScore = 0) ordered behind matches, newest first, so the list never
-- comes back empty for a student with no interest/history data yet.
CREATE PROCEDURE LMS.SP_GetRecommendedCoursesForStudent
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

    ;WITH HistoryCourseIds AS (
        SELECT CourseId FROM LMS.StudentCourseEnrollments WHERE StudentId = @StudentId
        UNION
        SELECT CourseId FROM LMS.CourseAssignments WHERE StudentId = @StudentId
    ),
    InterestTags AS (
        SELECT DISTINCT LTRIM(RTRIM(value)) AS Tag
        FROM STRING_SPLIT(ISNULL(@Interest, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    HistoryTags AS (
        SELECT DISTINCT LTRIM(RTRIM(t.value)) AS Tag
        FROM LMS.Courses c
        INNER JOIN HistoryCourseIds h ON h.CourseId = c.Id
        CROSS APPLY STRING_SPLIT(ISNULL(c.Tags, ''), ',') t
        WHERE LTRIM(RTRIM(t.value)) <> ''
    ),
    CourseTags AS (
        SELECT c.Id AS CourseId, LTRIM(RTRIM(t.value)) AS Tag
        FROM LMS.Courses c
        CROSS APPLY STRING_SPLIT(ISNULL(c.Tags, ''), ',') t
        WHERE LTRIM(RTRIM(t.value)) <> ''
    ),
    Scored AS (
        SELECT
            ct.CourseId,
            COUNT(DISTINCT it.Tag) AS InterestScore,
            COUNT(DISTINCT ht.Tag) AS HistoryScore
        FROM CourseTags ct
        LEFT JOIN InterestTags it ON it.Tag = ct.Tag
        LEFT JOIN HistoryTags  ht ON ht.Tag = ct.Tag
        GROUP BY ct.CourseId
    )
    SELECT TOP (@TopN)
        c.Id                                AS CourseId,
        c.Title                             AS CourseTitle,
        c.Description                       AS CourseDescription,
        c.CourseIconUrl,
        c.Tags,
        c.CompletionTimeSeconds,
        ISNULL(s.InterestScore, 0)          AS InterestScore,
        ISNULL(s.HistoryScore, 0)           AS HistoryScore,
        (ISNULL(s.InterestScore, 0) * 3 + ISNULL(s.HistoryScore, 0)) AS MatchScore
    FROM LMS.Courses c
    LEFT JOIN Scored s ON s.CourseId = c.Id
    WHERE c.IsActive = 1
      AND c.IsDeleted = 0
      AND c.Id NOT IN (SELECT CourseId FROM HistoryCourseIds)
    ORDER BY MatchScore DESC, c.CreatedAt DESC;

END
GO
