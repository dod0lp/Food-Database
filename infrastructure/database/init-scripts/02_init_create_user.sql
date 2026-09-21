USE [$(DB_NAME)];
GO

-- Create a new user that will use this database (for Backend work) (if doesn't exist already)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$(DB_USER)')
BEGIN
    CREATE LOGIN [$(DB_USER)] WITH PASSWORD = '$(DB_PASSWORD)';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(DB_USER)')
BEGIN
    CREATE USER [$(DB_USER)] FOR LOGIN [$(DB_USER)];
END
GO

-- Grant the user necessary permissions... just simply all permissions
-- (I have config so I know this user is meant to be admin)
ALTER ROLE db_owner ADD MEMBER [$(DB_USER)];
GO
