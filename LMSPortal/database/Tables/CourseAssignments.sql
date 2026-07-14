-- Audit-only: records that a trainer/admin assigned a course to a student.
-- Does NOT replace or duplicate LMS.StudentCourseEnrollments — assigning a
-- course still creates a normal row there via the existing
-- SP_EnrollStudentCourse, unchanged. This table only answers "who assigned
-- this course, and when" for reporting (self-registered courses never get
-- a row here).
CREATE TABLE LMS.CourseAssignments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    StudentId NVARCHAR(50) NOT NULL,

    CourseId INT NOT NULL,

    AssignedById NVARCHAR(50) NOT NULL,

    AssignedByName NVARCHAR(200) NOT NULL,

    AssignedByRole NVARCHAR(50) NOT NULL,

    AssignedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_CourseAssignments_Student
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_CourseAssignments_Course
        FOREIGN KEY (CourseId)
        REFERENCES LMS.Courses(Id),

    CONSTRAINT UQ_CourseAssignments_StudentCourse
        UNIQUE (StudentId, CourseId)
);
GO
