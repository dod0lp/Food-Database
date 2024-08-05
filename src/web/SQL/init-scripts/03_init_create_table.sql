USE [$(DB_NAME)];
GO

IF OBJECT_ID('dbo.Food', 'U') IS NOT NULL
    DROP TABLE dbo.Food;
GO

IF OBJECT_ID('dbo.Nutrients', 'U') IS NOT NULL
    DROP TABLE dbo.Nutrients;
GO

IF OBJECT_ID('dbo.Ingredients', 'U') IS NOT NULL
    DROP TABLE dbo.Ingredients;
GO

CREATE TABLE dbo.Food (
    ID INT PRIMARY KEY IDENTITY(1, 1),
    Name NVARCHAR(100) NOT NULL,
    Weight FLOAT NOT NULL,
    Description NVARCHAR(4000)
);
GO

CREATE TABLE dbo.Nutrients (
    ID INT PRIMARY KEY IDENTITY(1, 1),
    FoodID INT,
    Energy_Kcal INT NOT NULL,
    Energy_Kj INT NOT NULL,
    Fat_Total FLOAT NOT NULL,
    Fat_Saturated FLOAT NOT NULL,
    Carbs_Total FLOAT NOT NULL,
    Carbs_Saturated FLOAT NOT NULL,
    Protein_Total FLOAT NOT NULL,
    Salt_Total FLOAT NOT NULL,
    CONSTRAINT FK_Food_Nutrients FOREIGN KEY (FoodID) REFERENCES dbo.Food(food_id)
);
GO

CREATE TABLE dbo.Ingredients (
    food_id INT NOT NULL,
    nutrient_id INT NOT NULL,
    quantity DECIMAL(10, 2),
    PRIMARY KEY (food_id, nutrient_id),
    FOREIGN KEY (food_id) REFERENCES dbo.Food(food_id),
    FOREIGN KEY (nutrient_id) REFERENCES dbo.Nutrients(nutrient_id)
);
GO
