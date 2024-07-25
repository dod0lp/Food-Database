-- Create the main (Food) database, if it does not exist already
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'$(DB_NAME)')
BEGIN
    EXEC sp_executesql N'CREATE DATABASE [$(DB_NAME)]';
END
GO
