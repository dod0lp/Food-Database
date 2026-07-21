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
        -- Default food description
        Food_Description NVARCHAR(4000) NULL,

        -- Following is normalised for 100g
        Energy_Kcal INT NULL,
        Fat_Total DECIMAL(16,2) NULL,
        Fat_Saturated DECIMAL(16,2) NULL,
        Carbs_Total DECIMAL(16,2) NULL,
        Carbs_Sugar DECIMAL(16,2) NULL,
        Protein_Total DECIMAL(16,2) NULL,
        Salt_Total DECIMAL(16,2) NULL,

        CONSTRAINT CK_Food_Nutrients
            CHECK (
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

-- User's favorite food
IF OBJECT_ID(N'dbo.UserFoodFavorite', N'U') IS NULL
BEGIN
    CREATE TABLE UserFoodFavorite
    (
        User_Id INT NOT NULL
            CONSTRAINT FK_UserFoodFavorite_User
            REFERENCES Users(Id),

        Food_Id INT NOT NULL
            CONSTRAINT FK_UserFoodFavorite_Food
            REFERENCES Food(Id),

        CONSTRAINT PK_UserFoodFavorite
            PRIMARY KEY (User_Id, Food_Id)
    );
END;

IF OBJECT_ID(N'dbo.UserFoodRemark', N'U') IS NULL
BEGIN
    CREATE TABLE UserFoodRemark
    (
        User_Id INT NOT NULL
            CONSTRAINT FK_UserFoodRemark_User
            REFERENCES Users(Id),

        Food_Id INT NOT NULL
            CONSTRAINT FK_UserFoodRemark_Food
            REFERENCES Food(Id),

        -- Each user has his own (optional) note for food
        Food_Remark NVARCHAR(4000) NULL,

        CONSTRAINT PK_UserFoodRemark
            PRIMARY KEY (User_Id, Food_Id)
    );
END;

IF OBJECT_ID(N'dbo.UserFoodOptions', N'U') IS NULL
BEGIN
    CREATE TABLE UserFoodOptions
    (
        User_Id INT NOT NULL
            CONSTRAINT FK_UserFoodOptions_User
            REFERENCES Users(Id),

        Food_Id INT NOT NULL
            CONSTRAINT FK_UserFoodOptions_Food
            REFERENCES Food(Id),

        -- Based on weight easily decompose food ingredients
        -- Since not null, maybe by default work with 100, but it is up to app/backend
        Weight_Total DECIMAL(16,4) NOT NULL,
        Price_Eur DECIMAL(16,4) NULL,

        CONSTRAINT CK_UserFood_Price
            CHECK (Price_Eur IS NULL OR Price_Eur >= 0),

        CONSTRAINT PK_UserFoodOptions
            PRIMARY KEY (User_Id, Food_Id)
    );
END;

IF OBJECT_ID(N'dbo.FoodIngredient', N'U') IS NULL
BEGIN
    CREATE TABLE FoodIngredient
    (
        -- Main food that is complete, same ID be multiple times
        Food_Id INT NOT NULL
            CONSTRAINT FK_FoodIngredient_Food
            REFERENCES Food(Id),

        -- One of the ingredients in the main food
        Ingredient_Food_Id INT NOT NULL
            CONSTRAINT FK_FoodIngredient_IngredientFood
            REFERENCES Food(Id),

        -- Amount of the ingredient used in the composed food
        -- Normalised for 100g
        Weight_Ingredient_Normalised DECIMAL(16,2) NOT NULL,

        CONSTRAINT PK_FoodIngredient
            PRIMARY KEY (Food_Id, Ingredient_Food_Id),

        CONSTRAINT CK_FoodIngredient_Amount
            CHECK (Weight_Ingredient_Normalised > 0),

        CONSTRAINT CK_FoodIngredient_NotSelf
            CHECK (Food_Id <> Ingredient_Food_Id)
    );
END;