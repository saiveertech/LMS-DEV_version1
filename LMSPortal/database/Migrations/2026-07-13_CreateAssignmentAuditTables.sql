-- Adds LMS.CourseAssignments and LMS.AssignmentAllocations: audit-only
-- tables recording when a trainer/admin assigns a course or assignment to a
-- student. LMS.StudentCourseEnrollments / LMS.StudentAssignmentEnrollments
-- are NOT modified — assigning still creates a normal enrollment row via
-- the existing SP_EnrollStudentCourse / SP_EnrollStudentAssignment,
-- unchanged. A course/assignment with no matching row here was self-registered.
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'CourseAssignments'
)
BEGIN
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
            FOREIGN KEY (StudentId) REFERENCES LMS.Students(StudentId),

        CONSTRAINT FK_CourseAssignments_Course
            FOREIGN KEY (CourseId) REFERENCES LMS.Courses(Id),

        CONSTRAINT UQ_CourseAssignments_StudentCourse
            UNIQUE (StudentId, CourseId)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'AssignmentAllocations'
)
BEGIN
    CREATE TABLE LMS.AssignmentAllocations
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId NVARCHAR(50) NOT NULL,
        AssignmentId INT NOT NULL,
        AssignedById NVARCHAR(50) NOT NULL,
        AssignedByName NVARCHAR(200) NOT NULL,
        AssignedByRole NVARCHAR(50) NOT NULL,
        AssignedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_AssignmentAllocations_Student
            FOREIGN KEY (StudentId) REFERENCES LMS.Students(StudentId),

        CONSTRAINT FK_AssignmentAllocations_Assignment
            FOREIGN KEY (AssignmentId) REFERENCES LMS.Assignments(AssignmentId),

        CONSTRAINT UQ_AssignmentAllocations_StudentAssignment
            UNIQUE (StudentId, AssignmentId)
    );
END
GO
