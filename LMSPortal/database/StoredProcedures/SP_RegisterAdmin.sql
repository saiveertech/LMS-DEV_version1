CREATE PROCEDURE LMS.SP_RegisterAdmin
(
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(200),
    @PhoneNumber NVARCHAR(50),
    @Password NVARCHAR(MAX),
    @ExperienceYears INT,
    @Skills NVARCHAR(MAX),
    @Bio NVARCHAR(MAX),
    @AdminId NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Placeholder NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
    DECLARE @NewId INT;

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
        @Placeholder,
        @FirstName,
        @LastName,
        @Email,
        @PhoneNumber,
        @Password,
        @ExperienceYears,
        @Skills,
        @Bio
    );

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

    SET @AdminId =
        'SK' +
        UPPER(LEFT(@FirstName, 1)) +
        UPPER(LEFT(@LastName, 1)) +
        RIGHT('000' + CAST(@NewId AS VARCHAR(10)), 3) +
        'AD';

    UPDATE LMS.Admin
    SET AdminId = @AdminId
    WHERE AdminId = @Placeholder;
END
GO
