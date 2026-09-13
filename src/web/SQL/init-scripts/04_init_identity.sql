USE [$(DB_NAME)];
GO

-- Required by SQL Server for filtered indexes used by ASP.NET Identity.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

CREATE TABLE AspNetRoles
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles(NormalizedName)
    WHERE NormalizedName IS NOT NULL;

CREATE TABLE AspNetRoleClaims
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY,
    RoleId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_Roles_RoleId
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims(RoleId);

CREATE TABLE AspNetUserClaims
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY,
    UserId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims(UserId);

CREATE TABLE AspNetUserLogins
(
    LoginProvider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId INT NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins(UserId);

CREATE TABLE AspNetUserRoles
(
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Roles_RoleId
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles(RoleId);

CREATE TABLE AspNetUserTokens
(
    UserId INT NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO
