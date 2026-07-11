-- 1) Drops LMS.Courses.WwEnvClientId and LMS.Courses.PassPercentage (no longer
--    used — pass/fail is now decided by the linked Assignment's PassPercentage).
-- 2) Adds LMS.Certificates.AssignmentId so certificate generation can be gated
--    by an Assignment's pass percentage instead of the course's.
--    NOTE: added as NULL (not NOT NULL) here because existing certificate rows
--    have no assignment to backfill against. New rows always populate it —
--    enforced by SP_GenerateCertificate, not a NOT NULL constraint — until every
--    existing row has been backfilled and the column tightened by hand.
-- 3) Creates LMS.StudentAssignmentEnrollments (new: students enrolling directly
--    in an assignment by AssignmentId, mirroring StudentCourseEnrollments).
-- Safe to run once per database (guarded with IF EXISTS / IF NOT EXISTS checks).

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'WwEnvClientId'
)
BEGIN
    ALTER TABLE LMS.Courses
    DROP COLUMN WwEnvClientId;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'PassPercentage'
)
BEGIN
    ALTER TABLE LMS.Courses
    DROP COLUMN PassPercentage;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Certificates') AND name = 'AssignmentId'
)
BEGIN
    ALTER TABLE LMS.Certificates
    ADD AssignmentId INT NULL
        CONSTRAINT FK_Certificates_Assignment
            REFERENCES LMS.Assignments(AssignmentId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'StudentAssignmentEnrollments'
)
BEGIN
    CREATE TABLE LMS.StudentAssignmentEnrollments
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EnrollmentId NVARCHAR(50) NOT NULL UNIQUE,
        StudentId NVARCHAR(50) NOT NULL,
        AssignmentId INT NOT NULL,
        EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        AssignmentStatus NVARCHAR(50) NOT NULL DEFAULT 'Enrolled',
        AssessmentScore DECIMAL(5,2) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL,

        CONSTRAINT FK_AssignmentEnrollment_Student
            FOREIGN KEY (StudentId)
            REFERENCES LMS.Students(StudentId),

        CONSTRAINT FK_AssignmentEnrollment_Assignment
            FOREIGN KEY (AssignmentId)
            REFERENCES LMS.Assignments(AssignmentId),

        CONSTRAINT UQ_StudentAssignment
            UNIQUE (StudentId, AssignmentId)
    );
END
GO
