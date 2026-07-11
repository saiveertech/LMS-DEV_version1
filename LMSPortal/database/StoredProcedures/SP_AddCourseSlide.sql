CREATE PROCEDURE LMS.SP_AddCourseSlide
(
    @CourseId INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @MediaType NVARCHAR(20),
    @MediaUrl NVARCHAR(MAX),
    @SortOrder INT
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LMS.CourseSlides
    (
        CourseId,
        Title,
        Description,
        MediaType,
        MediaUrl,
        SortOrder
    )
    VALUES
    (
        @CourseId,
        @Title,
        @Description,
        @MediaType,
        @MediaUrl,
        @SortOrder
    );
END
GO
