CREATE TABLE LMS.CourseSlides
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CourseId INT NOT NULL,

    Title NVARCHAR(200) NOT NULL,

    Description NVARCHAR(MAX) NULL,

    MediaType NVARCHAR(20) NOT NULL,
    -- Allowed values: Video | Url

    MediaUrl NVARCHAR(MAX) NOT NULL,

    SortOrder INT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    UpdatedDate DATETIME2 NULL,

    CONSTRAINT FK_CourseSlides_Course
        FOREIGN KEY (CourseId)
        REFERENCES LMS.Courses(Id)
);
GO
