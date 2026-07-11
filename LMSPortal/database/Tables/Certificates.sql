CREATE TABLE LMS.Certificates
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CertificateId NVARCHAR(50) NOT NULL UNIQUE,

    CredentialId NVARCHAR(50) NOT NULL UNIQUE,

    StudentId NVARCHAR(50) NOT NULL,

    StudentName NVARCHAR(200) NOT NULL,

    StudentEmail NVARCHAR(200) NOT NULL,

    CourseId INT NOT NULL,

    CourseName NVARCHAR(200) NOT NULL,

    AssignmentId INT NOT NULL,

    CompletionPercentage DECIMAL(5,2) NOT NULL,

    AssessmentScore DECIMAL(5,2) NOT NULL,

    PassPercentage DECIMAL(5,2) NOT NULL,

    CompletionDate DATETIME2 NOT NULL,

    IssuedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CertificateUrl NVARCHAR(MAX) NOT NULL,

    IsValid BIT NOT NULL DEFAULT 1,

    CreatedById NVARCHAR(50) NOT NULL,

    CreatedByName NVARCHAR(200) NOT NULL,

    CreatedByRole NVARCHAR(50) NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    UpdatedDate DATETIME2 NULL,

    CONSTRAINT FK_Certificates_Student
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_Certificates_Course
        FOREIGN KEY (CourseId)
        REFERENCES LMS.Courses(Id),

    CONSTRAINT FK_Certificates_Assignment
        FOREIGN KEY (AssignmentId)
        REFERENCES LMS.Assignments(AssignmentId),

    CONSTRAINT UQ_StudentCourse_Certificate
        UNIQUE (StudentId, CourseId)
);
GO
