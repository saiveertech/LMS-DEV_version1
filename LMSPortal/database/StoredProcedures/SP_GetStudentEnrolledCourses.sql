CREATE PROCEDURE LMS.SP_GetStudentEnrolledCourses
(
    @StudentId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EnrollmentId,
        e.StudentId,
        e.CourseId,
        c.Title                     AS CourseTitle,
        c.Description               AS CourseDescription,
        c.CourseIconUrl,
        c.Tags,
        c.CompletionTimeSeconds,
        c.PassPercentage,
        e.EnrollmentDate,
        e.CourseStatus,
        e.CertificateStatus,
        e.CertificateIssueDate,
        e.CreatedAt,
        e.UpdatedDate
    FROM LMS.StudentCourseEnrollments e
    INNER JOIN LMS.Courses c
        ON e.CourseId = c.Id
    WHERE e.StudentId = @StudentId
    ORDER BY e.EnrollmentDate DESC;

END
GO
