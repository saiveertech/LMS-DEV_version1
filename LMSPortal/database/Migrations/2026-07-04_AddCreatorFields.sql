-- Adds CreatedById / CreatedByName / CreatedByRole to LMS.Courses and LMS.Assignments.
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'CreatedById'
)
BEGIN
    ALTER TABLE LMS.Courses
    ADD CreatedById NVARCHAR(50) NOT NULL CONSTRAINT DF_Courses_CreatedById DEFAULT '';
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'CreatedBy'
)
AND NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'CreatedByName'
)
BEGIN
    EXEC sp_rename 'LMS.Courses.CreatedBy', 'CreatedByName', 'COLUMN';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Assignments') AND name = 'CreatedById'
)
BEGIN
    ALTER TABLE LMS.Assignments
    ADD CreatedById NVARCHAR(50) NOT NULL CONSTRAINT DF_Assignments_CreatedById DEFAULT '',
        CreatedByName NVARCHAR(200) NOT NULL CONSTRAINT DF_Assignments_CreatedByName DEFAULT '',
        CreatedByRole NVARCHAR(50) NOT NULL CONSTRAINT DF_Assignments_CreatedByRole DEFAULT '';
END
GO
