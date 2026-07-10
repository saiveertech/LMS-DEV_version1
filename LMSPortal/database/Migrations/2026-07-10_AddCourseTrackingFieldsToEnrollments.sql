-- Adds RegistrationStatus, CompletedLessons, TotalLessons, AssessmentScore
-- to LMS.StudentCourseEnrollments to support the Course Student Tracking API.
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.StudentCourseEnrollments') AND name = 'RegistrationStatus'
)
BEGIN
    ALTER TABLE LMS.StudentCourseEnrollments
    ADD RegistrationStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Enrollments_RegistrationStatus DEFAULT 'Registered';
    -- Allowed values: Registered | InProgress | Completed
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.StudentCourseEnrollments') AND name = 'CompletedLessons'
)
BEGIN
    ALTER TABLE LMS.StudentCourseEnrollments
    ADD CompletedLessons INT NOT NULL CONSTRAINT DF_Enrollments_CompletedLessons DEFAULT 0,
        TotalLessons INT NOT NULL CONSTRAINT DF_Enrollments_TotalLessons DEFAULT 0,
        AssessmentScore DECIMAL(5,2) NULL;
END
GO
