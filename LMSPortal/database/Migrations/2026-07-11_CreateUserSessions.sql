-- Creates LMS.UserSessions to support single-device login: one active
-- session per user, tracked by SessionId (embedded in the JWT) so every
-- secured API call can be validated against an active DB record.
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID('LMS') AND name = 'UserSessions'
)
BEGIN
    CREATE TABLE LMS.UserSessions
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,

        SessionId NVARCHAR(50) NOT NULL UNIQUE,

        UserId NVARCHAR(50) NOT NULL,

        Role NVARCHAR(50) NOT NULL,

        DeviceInfo NVARCHAR(500) NULL,

        LoginTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        LogoutTime DATETIME2 NULL,

        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_UserSessions_UserId_IsActive
        ON LMS.UserSessions (UserId, IsActive);
END
GO
