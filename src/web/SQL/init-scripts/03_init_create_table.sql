USE [$(DB_NAME)];
GO

IF OBJECT_ID('dbo.Food', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Food (
        ID INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Weight FLOAT NOT NULL,
        Description NVARCHAR(4000)
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
