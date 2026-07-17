 USE [master];
 GO

CREATE TABLE Users
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Users PRIMARY KEY
);

CREATE TABLE Food
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Food PRIMARY KEY,

    Name NVARCHAR(200) NOT NULL,

    -- Nutritional values per 100 g
    EnergyKcal DECIMAL(8,2) NULL,
    FatG DECIMAL(8,2) NULL,
    SaturatedFatG DECIMAL(8,2) NULL,
    CarbohydrateG DECIMAL(8,2) NULL,
    SugarsG DECIMAL(8,2) NULL,
    FibreG DECIMAL(8,2) NULL,
    ProteinG DECIMAL(8,2) NULL,
    SaltG DECIMAL(8,2) NULL,

    -- NULL = base food / manually defined food
    -- Non-NULL = user-created food
    CreatedByUserId INT NULL
        CONSTRAINT FK_Food_CreatedByUser
        REFERENCES Users(Id),

    CONSTRAINT CK_Food_Nutrients
        CHECK (
            (EnergyKcal IS NULL OR EnergyKcal >= 0) AND
            (FatG IS NULL OR FatG >= 0) AND
            (SaturatedFatG IS NULL OR SaturatedFatG >= 0) AND
            (CarbohydrateG IS NULL OR CarbohydrateG >= 0) AND
            (SugarsG IS NULL OR SugarsG >= 0) AND
            (FibreG IS NULL OR FibreG >= 0) AND
            (ProteinG IS NULL OR ProteinG >= 0) AND
            (SaltG IS NULL OR SaltG >= 0)
        )
);

CREATE TABLE UserFood
(
    UserId INT NOT NULL
        CONSTRAINT FK_UserFood_User
        REFERENCES Users(Id),

    FoodId INT NOT NULL
        CONSTRAINT FK_UserFood_Food
        REFERENCES Food(Id),

    -- User-specific price, per 100 g
    PriceEur DECIMAL(10,4) NULL,

    CONSTRAINT PK_UserFood
        PRIMARY KEY (UserId, FoodId),

    CONSTRAINT CK_UserFood_Price
        CHECK (PriceEur IS NULL OR PriceEur >= 0)
);

CREATE TABLE FoodIngredient
(
    FoodId INT NOT NULL
        CONSTRAINT FK_FoodIngredient_Food
        REFERENCES Food(Id),

    IngredientFoodId INT NOT NULL
        CONSTRAINT FK_FoodIngredient_IngredientFood
        REFERENCES Food(Id),

    -- Amount of the ingredient used in the composed food
    AmountG DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_FoodIngredient
        PRIMARY KEY (FoodId, IngredientFoodId),

    CONSTRAINT CK_FoodIngredient_Amount
        CHECK (AmountG > 0),

    CONSTRAINT CK_FoodIngredient_NotSelf
        CHECK (FoodId <> IngredientFoodId)
);