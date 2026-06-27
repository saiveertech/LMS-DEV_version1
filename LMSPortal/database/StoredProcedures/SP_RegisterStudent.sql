CREATE PROCEDURE LMS.SP_RegisterStudent
(
@StudentId NVARCHAR(50),
@FirstName NVARCHAR(100),
@LastName NVARCHAR(100),
@Email NVARCHAR(200),
@PhoneNumber NVARCHAR(50),
@Password NVARCHAR(MAX),
@EducationDetails NVARCHAR(MAX),
@AreaOfInterest NVARCHAR(MAX)
)
AS
BEGIN
INSERT INTO LMS.Students
(
StudentId,
FirstName,
LastName,
Email,
PhoneNumber,
Password,
EducationDetails,
AreaOfInterest
)
VALUES
(
@StudentId,
@FirstName,
@LastName,
@Email,
@PhoneNumber,
@Password,
@EducationDetails,
@AreaOfInterest
)
END
GO
