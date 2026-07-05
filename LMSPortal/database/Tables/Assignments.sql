CREATE TABLE LMS.Assignments
(
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,

    Title NVARCHAR(200) NOT NULL,

    Description NVARCHAR(MAX) NULL,

    IntroVideoUrl NVARCHAR(MAX) NULL,

    QuestionsCsvUrl NVARCHAR(MAX) NULL,

    CompletionTimeSeconds INT NOT NULL,

    PassPercentage DECIMAL(5,2) NOT NULL,

    WwEnvClientId NVARCHAR(100) NULL,

    AssessmentIconUrl NVARCHAR(MAX) NULL,

    Tags NVARCHAR(MAX) NULL,

    CreatedById NVARCHAR(50) NOT NULL,

    CreatedByName NVARCHAR(200) NOT NULL,

    CreatedByRole NVARCHAR(50) NOT NULL,

    EditedById NVARCHAR(50) NULL,

    EditedByName NVARCHAR(200) NULL,

    EditedByRole NVARCHAR(50) NULL,

    DeletedById NVARCHAR(50) NULL,

    DeletedByName NVARCHAR(200) NULL,

    DeletedByRole NVARCHAR(50) NULL,

    DeletedAt DATETIME2 NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    UpdatedAt DATETIME2 NULL,

    IsDeleted BIT NOT NULL DEFAULT 0
);
GO
