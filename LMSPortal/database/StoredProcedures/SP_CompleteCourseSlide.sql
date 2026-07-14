CREATE PROCEDURE LMS.SP_CompleteCourseSlide
(
    @StudentId NVARCHAR(50),
    @SlideId   INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CourseId INT;
    SELECT @CourseId = CourseId FROM LMS.CourseSlides WHERE Id = @SlideId;

    IF @CourseId IS NULL
    BEGIN
        RAISERROR('Slide not found.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1 FROM LMS.StudentCourseEnrollments
        WHERE StudentId = @StudentId AND CourseId = @CourseId
    )
    BEGIN
        RAISERROR('Student is not enrolled in this course.', 16, 1);
        RETURN;
    END

    -- Idempotent: no-op if already marked complete
    IF NOT EXISTS (
        SELECT 1 FROM LMS.StudentSlideProgress
        WHERE StudentId = @StudentId AND CourseSlideId = @SlideId
    )
    BEGIN
        INSERT INTO LMS.StudentSlideProgress (StudentId, CourseSlideId)
        VALUES (@StudentId, @SlideId);
    END

    DECLARE @TotalLessons INT = (SELECT COUNT(*) FROM LMS.CourseSlides WHERE CourseId = @CourseId);

    DECLARE @CompletedLessons INT = (
        SELECT COUNT(*)
        FROM LMS.StudentSlideProgress sp
        INNER JOIN LMS.CourseSlides cs ON cs.Id = sp.CourseSlideId
        WHERE sp.StudentId = @StudentId AND cs.CourseId = @CourseId
    );

    -- CourseStatus/RegistrationStatus are derived, never set directly:
    -- 100% complete flips both to Completed automatically.
    UPDATE LMS.StudentCourseEnrollments
    SET CompletedLessons    = @CompletedLessons,
        TotalLessons        = @TotalLessons,
        CourseStatus         = CASE
                                    WHEN @TotalLessons > 0 AND @CompletedLessons >= @TotalLessons THEN 'Completed'
                                    WHEN @CompletedLessons > 0 THEN 'In Progress'
                                    ELSE 'Enrolled'
                                END,
        RegistrationStatus   = CASE
                                    WHEN @TotalLessons > 0 AND @CompletedLessons >= @TotalLessons THEN 'Completed'
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
