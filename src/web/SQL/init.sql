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

IF OBJECT_ID('dbo.Food', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Food (
        food_id INT PRIMARY KEY IDENTITY(1,1),
        food_name NVARCHAR(100) NOT NULL,
        food_description NVARCHAR(10000)
    );
END
GO

IF OBJECT_ID('dbo.Nutrients', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Nutrients (
        nutrient_id INT PRIMARY KEY IDENTITY(1,1),
        nutrient_name NVARCHAR(100) NOT NULL,
        nutrient_unit NVARCHAR(50)
    );
END
GO

IF OBJECT_ID('dbo.Ingredients', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ingredients (
        food_id INT NOT NULL,
        nutrient_id INT NOT NULL,
        quantity DECIMAL(10, 2),
        PRIMARY KEY (food_id, nutrient_id),
        FOREIGN KEY (food_id) REFERENCES dbo.Food(food_id),
        FOREIGN KEY (nutrient_id) REFERENCES dbo.Nutrients(nutrient_id)
    );
END
GO

