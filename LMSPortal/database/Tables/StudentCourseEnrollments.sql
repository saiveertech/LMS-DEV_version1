CREATE TABLE LMS.StudentCourseEnrollments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    EnrollmentId NVARCHAR(50) NOT NULL UNIQUE,

    StudentId NVARCHAR(50) NOT NULL,

    CourseId INT NOT NULL,

    EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CourseStatus NVARCHAR(50) NOT NULL DEFAULT 'Enrolled',
    -- Allowed values: Enrolled | In Progress | Completed

    CertificateStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    -- Allowed values: Pending | Issued

    CertificateIssueDate DATETIME2 NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    UpdatedDate DATETIME2 NULL,

    CONSTRAINT FK_Enrollment_Student
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_Enrollment_Course
        FOREIGN KEY (CourseId)
        REFERENCES LMS.Courses(Id),

    CONSTRAINT UQ_StudentCourse
        UNIQUE (StudentId, CourseId)
);
GO
