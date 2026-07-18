USE [$(DB_NAME)];
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE Users
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Users PRIMARY KEY
    );
END;


IF OBJECT_ID(N'dbo.Food', N'U') IS NULL
BEGIN
    CREATE TABLE Food
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Food PRIMARY KEY,

        Name NVARCHAR(200) NOT NULL,

        Weight_Total DECIMAL(16,2) NULL,
        Energy_Kcal INT NULL,
        Fat_Total DECIMAL(16,2) NULL,
        Fat_Saturated DECIMAL(16,2) NULL,
        Carbs_Total DECIMAL(16,2) NULL,
        Carbs_Sugar DECIMAL(16,2) NULL,
        Protein_Total DECIMAL(16,2) NULL,
        Salt_Total DECIMAL(16,2) NULL,

        CONSTRAINT CK_Food_Nutrients
            CHECK (
                (Weight_Total IS NULL OR Weight_Total >= 0) AND
                (Energy_Kcal IS NULL OR Energy_Kcal >= 0) AND
                (Fat_Total IS NULL OR Fat_Total >= 0) AND
                (Fat_Saturated IS NULL OR Fat_Saturated >= 0) AND
                (Carbs_Total IS NULL OR Carbs_Total >= 0) AND
                (Carbs_Sugar IS NULL OR Carbs_Sugar >= 0) AND
                (Protein_Total IS NULL OR Protein_Total >= 0) AND
                (Salt_Total IS NULL OR Salt_Total >= 0)
            )
    );
END;

-- Every user can set his own price for food
IF OBJECT_ID(N'dbo.UserFood', N'U') IS NULL
BEGIN
    CREATE TABLE UserFood
    (
        User_Id INT NOT NULL
            CONSTRAINT FK_UserFood_User
            REFERENCES Users(Id),

        Food_Id INT NOT NULL
            CONSTRAINT FK_UserFood_Food
            REFERENCES Food(Id),

        Price_Eur DECIMAL(16,4) NULL,

        CONSTRAINT PK_UserFood
            PRIMARY KEY (User_Id, Food_Id),

        CONSTRAINT CK_UserFood_Price
            CHECK (Price_Eur IS NULL OR Price_Eur >= 0)
    );
END;

IF OBJECT_ID(N'dbo.FoodIngredient', N'U') IS NULL
BEGIN
    CREATE TABLE FoodIngredient
    (
        -- Main food that is complete
        Food_Id INT NOT NULL
            CONSTRAINT FK_FoodIngredient_Food
            REFERENCES Food(Id),

        -- One of the ingredients in the main food
        Ingredient_Food_Id INT NOT NULL
            CONSTRAINT FK_FoodIngredient_IngredientFood
            REFERENCES Food(Id),

        -- Amount of the ingredient used in the composed food
        Weight_Ingredient DECIMAL(16,2) NOT NULL,

        CONSTRAINT PK_FoodIngredient
            PRIMARY KEY (Food_Id, Ingredient_Food_Id),

        CONSTRAINT CK_FoodIngredient_Amount
            CHECK (Weight_Ingredient > 0),

        CONSTRAINT CK_FoodIngredient_NotSelf
            CHECK (Food_Id <> Ingredient_Food_Id)
    );
END;

IF OBJECT_ID(N'dbo.FoodDescription', N'U') IS NULL
BEGIN
    CREATE TABLE FoodDescription
    (
        Food_Id INT NOT NULL
            CONSTRAINT FK_FoodDescription_Food
            REFERENCES Food(Id),

        Food_Description NVARCHAR(4000) NULL
    );
END;