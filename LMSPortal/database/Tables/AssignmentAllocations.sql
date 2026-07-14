-- Audit-only: records that a trainer/admin assigned an Assignment
-- (assessment) to a student. Does NOT replace or duplicate
-- LMS.StudentAssignmentEnrollments — assigning still creates a normal row
-- there via the existing SP_EnrollStudentAssignment, unchanged. This table
-- only answers "who assigned this assignment, and when" for reporting
-- (self-registered assignments never get a row here).
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
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_AssignmentAllocations_Assignment
        FOREIGN KEY (AssignmentId)
        REFERENCES LMS.Assignments(AssignmentId),

    CONSTRAINT UQ_AssignmentAllocations_StudentAssignment
        UNIQUE (StudentId, AssignmentId)
);
GO
