-- Adds LMS.UserSessions.LastActivityTime to support idle-timeout detection
-- (heartbeat pings + per-request touch), independent of the JWT's own
-- absolute expiry. Needed so a crashed/closed browser that never calls
-- /terminate gets cleaned up well before its 60-minute JWT would expire.
-- Safe to run once per database (guarded with IF NOT EXISTS check).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.UserSessions') AND name = 'LastActivityTime'
)
BEGIN
    ALTER TABLE LMS.UserSessions
    ADD LastActivityTime DATETIME2 NOT NULL
        CONSTRAINT DF_UserSessions_LastActivityTime DEFAULT SYSUTCDATETIME();
END
GO
