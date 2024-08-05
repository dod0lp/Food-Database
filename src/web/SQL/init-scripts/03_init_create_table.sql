USE [$(DB_NAME)];
GO

/*IF OBJECT_ID('dbo.Food', 'U') IS NOT NULL
    DROP TABLE dbo.Food;
GO

IF OBJECT_ID('dbo.Nutrients', 'U') IS NOT NULL
    DROP TABLE dbo.Nutrients;
GO

IF OBJECT_ID('dbo.Ingredients', 'U') IS NOT NULL
    DROP TABLE dbo.Ingredients;
GO*/

CREATE TABLE dbo.Food (
    ID INT PRIMARY KEY IDENTITY(1, 1),
    Name NVARCHAR(100) NOT NULL,
    Weight FLOAT NOT NULL,
    Description NVARCHAR(4000)
);
GO

CREATE TABLE dbo.Nutrients (
    Food_ID INT PRIMARY KEY,
    Energy_Kcal INT NOT NULL,
    Energy_Kj INT NOT NULL,
    Fat_Total FLOAT NOT NULL,
    Fat_Saturated FLOAT NOT NULL,
    Carbs_Total FLOAT NOT NULL,
    Carbs_Saturated FLOAT NOT NULL,
    Protein_Total FLOAT NOT NULL,
    Salt_Total FLOAT NOT NULL,

    CONSTRAINT FK_Food_Nutrients FOREIGN KEY (Food_ID) REFERENCES dbo.Food(ID)
);
GO

CREATE TABLE dbo.Ingredients (
    Food_ID_Complete INT NOT NULL,
    Food_ID_Part     INT NOT NULL,
    
    CONSTRAINT PK_Ingredients PRIMARY KEY (Food_ID_Complete, Food_ID_Part),
    
    CONSTRAINT FK_Ingredients_Food_Complete FOREIGN KEY (Food_ID_Complete) REFERENCES dbo.Food(ID),
    CONSTRAINT FK_Ingredients_Food_Part FOREIGN KEY (Food_ID_Part) REFERENCES dbo.Food(ID)
);
GO
