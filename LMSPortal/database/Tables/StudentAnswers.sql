-- One row per question per attempt — the student's actual submitted
-- answer, evaluated against LMS.AssignmentQuestions.CorrectOption.
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
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_StudentAnswers_Question
        FOREIGN KEY (QuestionId)
        REFERENCES LMS.AssignmentQuestions(Id),

    CONSTRAINT UQ_StudentAnswers_Attempt
        UNIQUE (StudentId, AssignmentId, AttemptNumber, QuestionId)
);
GO
