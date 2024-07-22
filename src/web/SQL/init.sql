USE $(DB_NAME);
GO

-- Create user that will work with database - backend
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$(DB_USER)')
BEGIN
    EXEC sp_executesql N'CREATE LOGIN [$(DB_USER)] WITH PASSWORD = ''$(DB_PASSWORD)''';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(DB_USER)')
BEGIN
    EXEC sp_executesql N'CREATE USER [$(DB_USER)] FOR LOGIN [$(DB_USER)]';
END
GO

ALTER ROLE db_owner ADD MEMBER [$(DB_USER)];
GO

-- Create the table if it does not already exist
IF OBJECT_ID('dbo.test_table', 'U') IS NULL
BEGIN
    CREATE TABLE test_table (
        Id INT PRIMARY KEY IDENTITY,
        Name NVARCHAR(50) NOT NULL
    );
END
GO

INSERT INTO test_table (Name) VALUES ('Sample Data');
GO
