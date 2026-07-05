-- Adds EditedById/EditedByName/EditedByRole and DeletedById/DeletedByName/DeletedByRole/DeletedAt
-- to LMS.Courses and LMS.Assignments, plus IsDeleted on LMS.Courses.
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE LMS.Courses
    ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Courses_IsDeleted DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'EditedById'
)
BEGIN
    ALTER TABLE LMS.Courses
    ADD EditedById NVARCHAR(50) NULL,
        EditedByName NVARCHAR(200) NULL,
        EditedByRole NVARCHAR(50) NULL,
        DeletedById NVARCHAR(50) NULL,
        DeletedByName NVARCHAR(200) NULL,
        DeletedByRole NVARCHAR(50) NULL,
        DeletedAt DATETIME2 NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Assignments') AND name = 'EditedById'
)
BEGIN
    ALTER TABLE LMS.Assignments
    ADD EditedById NVARCHAR(50) NULL,
        EditedByName NVARCHAR(200) NULL,
        EditedByRole NVARCHAR(50) NULL,
        DeletedById NVARCHAR(50) NULL,
        DeletedByName NVARCHAR(200) NULL,
        DeletedByRole NVARCHAR(50) NULL,
        DeletedAt DATETIME2 NULL;
END
GO
