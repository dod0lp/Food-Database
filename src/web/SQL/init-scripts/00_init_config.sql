-- Choose master database for credentials etc
USE [master];
GO

-- Admin variables - credentials, based on .env file
DECLARE @DB_NAME NVARCHAR(128) = '$(DB_NAME)';
DECLARE @DB_USER NVARCHAR(128) = '$(DB_USER)';
DECLARE @DB_PASSWORD NVARCHAR(128) = '$(DB_PASSWORD)';


-- Check if the Config table exists and create it if it does not
-- and save credentials and database name into it.
IF OBJECT_ID('dbo.Config', 'U') IS NULL
BEGIN
    CREATE TABLE Config (
        ConfigKey NVARCHAR(128) PRIMARY KEY,
        ConfigValue NVARCHAR(128)
    );
END
GO

-- Use the INSERT statement with a condition to also avoid value duplicates
-- Insert DB_NAME - name of the database
IF NOT EXISTS (SELECT 1 FROM Config WHERE ConfigKey = 'DB_NAME')
BEGIN
    INSERT INTO Config (ConfigKey, ConfigValue) VALUES ('DB_NAME', @DB_NAME);
END

-- Insert DB_USER - name of the user that will be accesing this (Food) database
IF NOT EXISTS (SELECT 1 FROM Config WHERE ConfigKey = 'DB_USER')
BEGIN
    INSERT INTO Config (ConfigKey, ConfigValue) VALUES ('DB_USER', @DB_USER);
END

-- Insert DB_PASSWORD - password of this user
IF NOT EXISTS (SELECT 1 FROM Config WHERE ConfigKey = 'DB_PASSWORD')
BEGIN
    INSERT INTO Config (ConfigKey, ConfigValue) VALUES ('DB_PASSWORD', @DB_PASSWORD);
END
GO
