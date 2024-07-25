-- Choose master database for credentials etc
USE [master];
GO

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
