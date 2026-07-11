CREATE PROCEDURE LMS.SP_DeleteCourseSlidesByCourse
(
    @CourseId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM LMS.CourseSlides
    WHERE CourseId = @CourseId;
END
GO
