-- Adds three tables:
--   LMS.StudentSlideProgress   - per-slide completion, enables real "resume"
--   LMS.AssignmentQuestions    - parsed answer key (from trainer's CSV upload)
--   LMS.StudentAnswers         - student's submitted answers per attempt
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'StudentSlideProgress'
)
BEGIN
    CREATE TABLE LMS.StudentSlideProgress
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId NVARCHAR(50) NOT NULL,
        CourseSlideId INT NOT NULL,
        CompletedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_StudentSlideProgress_Student
            FOREIGN KEY (StudentId) REFERENCES LMS.Students(StudentId),

        CONSTRAINT FK_StudentSlideProgress_Slide
            FOREIGN KEY (CourseSlideId) REFERENCES LMS.CourseSlides(Id),

        CONSTRAINT UQ_StudentSlideProgress_StudentSlide
            UNIQUE (StudentId, CourseSlideId)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'AssignmentQuestions'
)
BEGIN
    CREATE TABLE LMS.AssignmentQuestions
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AssignmentId INT NOT NULL,
        QuestionText NVARCHAR(MAX) NOT NULL,
        OptionA NVARCHAR(500) NOT NULL,
        OptionB NVARCHAR(500) NOT NULL,
        OptionC NVARCHAR(500) NOT NULL,
        OptionD NVARCHAR(500) NOT NULL,
        CorrectOption CHAR(1) NOT NULL,
        Marks DECIMAL(5,2) NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_AssignmentQuestions_Assignment
            FOREIGN KEY (AssignmentId) REFERENCES LMS.Assignments(AssignmentId),

        CONSTRAINT CK_AssignmentQuestions_CorrectOption
            CHECK (CorrectOption IN ('A', 'B', 'C', 'D'))
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'StudentAnswers'
)
BEGIN
    CREATE TABLE LMS.StudentAnswers
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId NVARCHAR(50) NOT NULL,
        AssignmentId INT NOT NULL,
        AttemptNumber INT NOT NULL,
        QuestionId INT NOT NULL,
        SelectedOption CHAR(1) NOT NULL,
        IsCorrect BIT NOT NULL,
        AnsweredAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_StudentAnswers_Student
            FOREIGN KEY (StudentId) REFERENCES LMS.Students(StudentId),

        CONSTRAINT FK_StudentAnswers_Question
            FOREIGN KEY (QuestionId) REFERENCES LMS.AssignmentQuestions(Id),

        CONSTRAINT UQ_StudentAnswers_Attempt
            UNIQUE (StudentId, AssignmentId, AttemptNumber, QuestionId)
    );
END
GO
