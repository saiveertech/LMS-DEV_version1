-- Per-slide completion record, enabling real "resume where I left off"
-- tracking. StudentCourseEnrollments.CompletedLessons/TotalLessons are
-- derived FROM this table (count-based), not entered manually, once a
-- student progresses via slide completion.
CREATE TABLE LMS.StudentSlideProgress
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    StudentId NVARCHAR(50) NOT NULL,

    CourseSlideId INT NOT NULL,

    CompletedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_StudentSlideProgress_Student
        FOREIGN KEY (StudentId)
        REFERENCES LMS.Students(StudentId),

    CONSTRAINT FK_StudentSlideProgress_Slide
        FOREIGN KEY (CourseSlideId)
        REFERENCES LMS.CourseSlides(Id),

    CONSTRAINT UQ_StudentSlideProgress_StudentSlide
        UNIQUE (StudentId, CourseSlideId)
);
GO
