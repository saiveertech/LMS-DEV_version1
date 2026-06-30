CREATE PROCEDURE LMS.SP_RegisterAdmin
(
    @AdminId NVARCHAR(50),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(200),
    @PhoneNumber NVARCHAR(50),
    @Password NVARCHAR(MAX),
    @ExperienceYears INT,
    @Skills NVARCHAR(MAX),
    @Bio NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LMS.Admin
    (
        AdminId,
        FirstName,
        LastName,
        Email,
        PhoneNumber,
        Password,
        ExperienceYears,
        Skills,
        Bio
    )
    VALUES
    (
        @AdminId,
        @FirstName,
        @LastName,
        @Email,
        @PhoneNumber,
        @Password,
        @ExperienceYears,
        @Skills,
        @Bio
    );
END
GO