-- Choose master database for credentials etc
USE [master];
GO;

-- Admin variables - credentials, based on .env file
DECLARE @DB_NAME NVARCHAR(128) = '$(DB_NAME)';
DECLARE @DB_USER NVARCHAR(128) = '$(DB_USER)';
DECLARE @DB_PASSWORD NVARCHAR(128) = '$(DB_PASSWORD)';

-- Save credentials into newly created table Config
CREATE TABLE Config (
    ConfigKey NVARCHAR(128) PRIMARY KEY,
    ConfigValue NVARCHAR(128)
);

INSERT INTO Config (ConfigKey, ConfigValue) VALUES ('DB_NAME', @DB_NAME);
INSERT INTO Config (ConfigKey, ConfigValue) VALUES ('DB_USER', @DB_USER);
INSERT INTO Config (ConfigKey, ConfigValue) VALUES ('DB_PASSWORD', @DB_PASSWORD);
GO


-- Create the main (Food) database, if it does not exist already
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'$(DB_NAME)')
BEGIN
    EXEC sp_executesql N'CREATE DATABASE [$(DB_NAME)]';
END
GO

-- Use the database for initial creation of tables
-- and for setting up user that will work with this database
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

-- Create test-table, if does not exist already
IF OBJECT_ID('dbo.TestTable', 'U') IS NULL
BEGIN
    CREATE TABLE TestTable (
        Id INT PRIMARY KEY IDENTITY,
        Name NVARCHAR(50) NOT NULL
    );
END
GO

-- Insert sample data, if does not exist already
IF NOT EXISTS (SELECT * FROM TestTable WHERE Name = 'Sample Data')
BEGIN
    INSERT INTO TestTable (Name) VALUES ('Sample Data');
END
GO