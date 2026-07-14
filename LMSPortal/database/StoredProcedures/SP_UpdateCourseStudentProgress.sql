CREATE PROCEDURE LMS.SP_UpdateCourseStudentProgress
(
    @CourseId        INT,
    @StudentId       NVARCHAR(50),
    @CompletedLessons INT,
    @TotalLessons    INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM LMS.StudentCourseEnrollments
        WHERE StudentId = @StudentId AND CourseId = @CourseId
    )
    BEGIN
        RAISERROR('Enrollment not found for this student and course.', 16, 1);
        RETURN;
    END

    DECLARE @EffectiveTotalLessons INT = ISNULL(
        @TotalLessons,
        (SELECT TotalLessons FROM LMS.StudentCourseEnrollments
         WHERE StudentId = @StudentId AND CourseId = @CourseId));

    IF @CompletedLessons < 0 OR @CompletedLessons > @EffectiveTotalLessons
    BEGIN
        RAISERROR('CompletedLessons must be between 0 and TotalLessons.', 16, 1);
        RETURN;
    END

    -- CourseStatus/RegistrationStatus are derived, never set directly:
    -- 100% complete flips both to Completed automatically.
    UPDATE LMS.StudentCourseEnrollments
    SET CompletedLessons    = @CompletedLessons,
        TotalLessons        = @EffectiveTotalLessons,
        CourseStatus         = CASE
                                    WHEN @EffectiveTotalLessons > 0 AND @CompletedLessons >= @EffectiveTotalLessons THEN 'Completed'
                                    WHEN @CompletedLessons > 0 THEN 'In Progress'
                                    ELSE 'Enrolled'
                                END,
        RegistrationStatus   = CASE
                                    WHEN @EffectiveTotalLessons > 0 AND @CompletedLessons >= @EffectiveTotalLessons THEN 'Completed'
                                    WHEN @CompletedLessons > 0 THEN 'InProgress'
                                    ELSE 'Registered'
                                END,
        UpdatedDate          = SYSUTCDATETIME()
    WHERE StudentId = @StudentId
      AND CourseId  = @CourseId;

    -- Return the updated row in the same shape as SP_GetCourseStudentTracking
    SELECT
        e.StudentId,
        s.FirstName + ' ' + s.LastName                             AS StudentName,
        s.Email,
        e.CourseId,
        co.Title                                                   AS CourseTitle,
        e.EnrollmentDate,
        CASE WHEN ca.Id IS NOT NULL THEN 'TrainerAssigned' ELSE 'Self' END AS EnrollmentSource,
        ca.AssignedByName,
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
       AND cert.CertificateType = 'Course'
    LEFT JOIN LMS.CourseAssignments ca
        ON ca.StudentId = e.StudentId
       AND ca.CourseId = e.CourseId
    WHERE e.StudentId = @StudentId
      AND e.CourseId  = @CourseId;

END
GO
