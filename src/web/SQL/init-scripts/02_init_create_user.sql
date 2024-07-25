USE [$(DB_NAME)];
GO

-- Create a new user that will use this database (for Backend work), if does not exist already
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$(DB_USER)')
BEGIN
    CREATE LOGIN [$(DB_USER)] WITH PASSWORD = '$(DB_PASSWORD)';
END
GO

-- Create a user in the database for Backend work, if does not exist already
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(DB_USER)')
BEGIN
    CREATE USER [$(DB_USER)] FOR LOGIN [$(DB_USER)];
END
GO

-- Grant the user necessary permissions... just simply add *all* permissions
ALTER ROLE db_owner ADD MEMBER [$(DB_USER)];
GO
