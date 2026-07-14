-- The parsed answer key for an assignment — populated automatically from
-- the trainer's uploaded QuestionsCsv at assignment create/update time.
-- Single-answer multiple choice only (A/B/C/D).
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
    -- Allowed values: A | B | C | D

    Marks DECIMAL(5,2) NOT NULL DEFAULT 1,

    SortOrder INT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_AssignmentQuestions_Assignment
        FOREIGN KEY (AssignmentId)
        REFERENCES LMS.Assignments(AssignmentId),

    CONSTRAINT CK_AssignmentQuestions_CorrectOption
        CHECK (CorrectOption IN ('A', 'B', 'C', 'D'))
);
GO
