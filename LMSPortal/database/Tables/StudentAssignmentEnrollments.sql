CREATE TABLE LMS.StudentAssignmentEnrollments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    EnrollmentId NVARCHAR(50) NOT NULL UNIQUE,

    StudentId NVARCHAR(50) NOT NULL,

    AssignmentId INT NOT NULL,

    EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    AssignmentStatus NVARCHAR(50) NOT NULL DEFAULT 'Enrolled',
    -- Allowed values: Enrolled | In Progress | Completed

    AssessmentScore DECIMAL(5,2) NULL,

    -- Number of times the student has submitted this assignment.
    -- AssessmentScore always holds the best score across all attempts.
    Attempts INT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    UpdatedDate DATETIME2 NULL,

    CONSTRAINT FK_AssignmentEnrollment_Student
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_AssignmentEnrollment_Assignment
        FOREIGN KEY (AssignmentId)
        REFERENCES LMS.Assignments(AssignmentId),

    CONSTRAINT UQ_StudentAssignment
        UNIQUE (StudentId, AssignmentId)
);
GO
