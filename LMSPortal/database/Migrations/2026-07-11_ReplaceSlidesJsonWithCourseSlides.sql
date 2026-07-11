-- Replaces the free-text LMS.Courses.SlidesJson blob column with a proper
-- LMS.CourseSlides table (one row per slide: Title, Description, MediaType, MediaUrl).
-- Safe to run once per database (guarded with IF NOT EXISTS / IF EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'CourseSlides'
)
BEGIN
    CREATE TABLE LMS.CourseSlides
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CourseId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        MediaType NVARCHAR(20) NOT NULL,
        MediaUrl NVARCHAR(MAX) NOT NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate DATETIME2 NULL,

        CONSTRAINT FK_CourseSlides_Course
            FOREIGN KEY (CourseId)
            REFERENCES LMS.Courses(Id)
    );
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.Courses') AND name = 'SlidesJson'
)
BEGIN
    ALTER TABLE LMS.Courses
    DROP COLUMN SlidesJson;
END
GO
