-- Certificates can now be issued from EITHER a course completion OR an
-- assignment pass, independently (not both required together as before).
-- Adds CertificateType ('Course' | 'Assignment') and relaxes the columns
-- that only apply to one side to be nullable. Existing rows (which have
-- both CourseId and AssignmentId set, from the old combined flow) are
-- grandfathered in as CertificateType = 'Course' and are NOT re-validated
-- against the new either/or CHECK constraint (added WITH NOCHECK).
-- Safe to run once per database (guarded with IF EXISTS / IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Certificates') AND name = 'CertificateType'
)
BEGIN
    ALTER TABLE LMS.Certificates
    ADD CertificateType NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Certificates_CertificateType DEFAULT 'Course';
END
GO

-- Drop the FK on CourseId so the column can be made nullable, then recreate it.
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Certificates_Course'
)
BEGIN
    ALTER TABLE LMS.Certificates DROP CONSTRAINT FK_Certificates_Course;
END
GO

ALTER TABLE LMS.Certificates ALTER COLUMN CourseId INT NULL;
GO

ALTER TABLE LMS.Certificates
    ADD CONSTRAINT FK_Certificates_Course
        FOREIGN KEY (CourseId) REFERENCES LMS.Courses(Id);
GO

ALTER TABLE LMS.Certificates ALTER COLUMN CourseName NVARCHAR(200) NULL;
GO

ALTER TABLE LMS.Certificates ALTER COLUMN AssignmentId INT NULL;
GO

ALTER TABLE LMS.Certificates ALTER COLUMN CompletionPercentage DECIMAL(5,2) NULL;
GO

ALTER TABLE LMS.Certificates ALTER COLUMN AssessmentScore DECIMAL(5,2) NULL;
GO

ALTER TABLE LMS.Certificates ALTER COLUMN PassPercentage DECIMAL(5,2) NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Certificates_TypeConsistency'
)
BEGIN
    ALTER TABLE LMS.Certificates WITH NOCHECK
    ADD CONSTRAINT CK_Certificates_TypeConsistency
    CHECK (
        (CertificateType = 'Course'     AND CourseId IS NOT NULL)
     OR (CertificateType = 'Assignment' AND AssignmentId IS NOT NULL)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('LMS.Certificates') AND name = 'UQ_StudentAssignment_Certificate'
)
BEGIN
    ALTER TABLE LMS.Certificates
    ADD CONSTRAINT UQ_StudentAssignment_Certificate
        UNIQUE (StudentId, AssignmentId);
END
GO
