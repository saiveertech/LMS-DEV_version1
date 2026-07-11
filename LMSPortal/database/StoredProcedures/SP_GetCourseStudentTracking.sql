CREATE PROCEDURE LMS.SP_GetCourseStudentTracking
(
    @CourseId             INT,
    @Status               NVARCHAR(50) = NULL,   -- Registered | InProgress | Completed
    @StudentId            NVARCHAR(50) = NULL,
    @CertificateGenerated BIT          = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.StudentId,
        s.FirstName + ' ' + s.LastName                             AS StudentName,
        s.Email,
        e.CourseId,
        co.Title                                                   AS CourseTitle,
        e.EnrollmentDate,
        e.RegistrationStatus,
        e.CourseStatus,
        CASE
            WHEN e.TotalLessons > 0
                THEN CAST(ROUND((CAST(e.CompletedLessons AS DECIMAL(9,2)) / e.TotalLessons) * 100, 2) AS DECIMAL(5,2))
            ELSE CAST(0 AS DECIMAL(5,2))
        END                                                         AS CompletionPercentage,
        e.CompletedLessons,
        e.TotalLessons,
        e.AssessmentScore,
        CAST(
            CASE WHEN cert.CertificateId IS NOT NULL THEN 1 ELSE 0 END
        AS BIT)                                                     AS CertificateGenerated,
        cert.CertificateId,
        cert.IssuedDate                                             AS CertificateIssueDate,
        cert.CertificateUrl,
        e.CreatedAt,
        e.UpdatedDate
    FROM LMS.StudentCourseEnrollments e
    INNER JOIN LMS.Students s
        ON e.StudentId = s.StudentId
    INNER JOIN LMS.Courses co
        ON e.CourseId = co.Id
    LEFT JOIN LMS.Certificates cert
        ON cert.StudentId = e.StudentId
       AND cert.CourseId = e.CourseId
    WHERE e.CourseId = @CourseId
      AND (@Status IS NULL OR e.RegistrationStatus = @Status)
      AND (@StudentId IS NULL OR e.StudentId = @StudentId)
      AND (
            @CertificateGenerated IS NULL
            OR (@CertificateGenerated = 1 AND cert.CertificateId IS NOT NULL)
            OR (@CertificateGenerated = 0 AND cert.CertificateId IS NULL)
          )
    ORDER BY e.EnrollmentDate DESC;

END
GO
