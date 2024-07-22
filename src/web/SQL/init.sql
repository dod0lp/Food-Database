CREATE DATABASE db_food;
GO

USE db_food;

GO

CREATE TABLE TestTable (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(50) NOT NULL
);
GO

INSERT INTO TestTable (Name) VALUES ('Sample Data');
GO
