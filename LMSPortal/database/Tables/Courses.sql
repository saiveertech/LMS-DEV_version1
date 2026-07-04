CREATE TABLE LMS.Courses
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    Title NVARCHAR(200) NOT NULL,

    Description NVARCHAR(MAX) NULL,

    IntroVideoUrl NVARCHAR(MAX) NULL,

    SlidesJson NVARCHAR(MAX) NULL,

    CompletionTimeSeconds INT NOT NULL,

    PassPercentage DECIMAL(5,2) NOT NULL,

    WwEnvClientId NVARCHAR(100) NULL,

    CourseIconUrl NVARCHAR(MAX) NULL,

    Tags NVARCHAR(MAX) NULL,

    CourseStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',

    CreatedBy NVARCHAR(200) NOT NULL,

    CreatedByRole NVARCHAR(50) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    UpdatedDate DATETIME2 NULL
);
GO
