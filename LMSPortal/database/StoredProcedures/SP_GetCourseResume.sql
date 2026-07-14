-- Returns every slide in the course, ordered by SortOrder, flagged with
-- whether this student has completed it. The frontend's "resume" point is
-- the first row with IsCompleted = 0.
CREATE PROCEDURE LMS.SP_GetCourseResume
(
    @StudentId NVARCHAR(50),
    @CourseId  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cs.Id            AS SlideId,
        cs.Title,
        cs.MediaType,
        cs.MediaUrl,
        cs.SortOrder,
        CAST(
            CASE WHEN sp.Id IS NOT NULL THEN 1 ELSE 0 END
        AS BIT)          AS IsCompleted,
        sp.CompletedAt
    FROM LMS.CourseSlides cs
    LEFT JOIN LMS.StudentSlideProgress sp
        ON sp.CourseSlideId = cs.Id
       AND sp.StudentId = @StudentId
    WHERE cs.CourseId = @CourseId
    ORDER BY cs.SortOrder;

END
GO
