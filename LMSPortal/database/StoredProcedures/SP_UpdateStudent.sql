CREATE PROCEDURE LMS.SP_UpdateStudent
(
@StudentId NVARCHAR(50),
@FirstName NVARCHAR(100),
@LastName NVARCHAR(100),
@Email NVARCHAR(200),
@PhoneNumber NVARCHAR(50),
@EducationDetails NVARCHAR(MAX),
@AreaOfInterest NVARCHAR(MAX)
)
AS
BEGIN


UPDATE LMS.Students
SET
    FirstName = @FirstName,
    LastName = @LastName,
    Email = @Email,
    PhoneNumber = @PhoneNumber,
    EducationDetails = @EducationDetails,
    AreaOfInterest = @AreaOfInterest
WHERE StudentId = @StudentId


END
GO
