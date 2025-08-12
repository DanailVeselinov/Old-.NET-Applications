
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/10/2015 22:41:25
-- Generated from EDMX file: C:\Users\Danail Veselinov\Documents\Visual Studio 2013\Projects\BetFairBot\BFDL\BFDBLModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BFDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Betline1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlacedBets] DROP CONSTRAINT [FK_Betline1];
GO
IF OBJECT_ID(N'[dbo].[FK_Filter1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Betlines] DROP CONSTRAINT [FK_Filter1];
GO
IF OBJECT_ID(N'[dbo].[FK_OwnerUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Betlines] DROP CONSTRAINT [FK_OwnerUser];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Betlines]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Betlines];
GO
IF OBJECT_ID(N'[dbo].[Filters]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Filters];
GO
IF OBJECT_ID(N'[dbo].[PlacedBets]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PlacedBets];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Betlines'
CREATE TABLE [dbo].[Betlines] (
    [BetlineId] int IDENTITY(1,1) NOT NULL,
    [stakeRangeMin] float  NOT NULL,
    [stakeRangeMax] float  NOT NULL,
    [initialProfitPerBet] float  NOT NULL,
    [profitPerBet] float  NOT NULL,
    [lastBetLost] bit  NOT NULL,
    [isActive] bit  NOT NULL,
    [currencyCode] nvarchar(5)  NULL,
    [Filter_FilterId] int  NULL,
    [lastPlacedBetId] nvarchar(500)  NULL,
    [algorithmName] nvarchar(50)  NULL,
    [betType] nvarchar(20)  NULL,
    [ownersUserName] nvarchar(100)  NULL,
    [betlineName] nvarchar(100)  NULL,
    [profitPerBudjet] float  NOT NULL,
    [ballance] decimal(19,4)  NOT NULL
);
GO

-- Creating table 'Filters'
CREATE TABLE [dbo].[Filters] (
    [FilterId] int IDENTITY(1,1) NOT NULL,
    [marketTimeFrom] datetime  NULL,
    [marketTimeTo] datetime  NULL,
    [isActive] bit  NULL,
    [totalAmountMatchedMin] float  NULL,
    [totalAmountMatchedMax] float  NULL,
    [marketNames] nvarchar(4000)  NULL,
    [sortedOrders] nvarchar(250)  NULL,
    [maxBetsCount] tinyint  NULL,
    [maxAmmountIncremented] float  NULL,
    [maxAmmountIncrementedPerBudjet] float  NOT NULL,
    [goalDifferenceMin] smallint  NULL,
    [goalDifferenceMax] smallint  NULL,
    [textQuery] nvarchar(50)  NULL,
    [exchangeIds] varchar(100)  NULL,
    [eventTypeIds] varchar(100)  NULL,
    [compeitionIds] varchar(100)  NULL,
    [marketIds] varchar(100)  NULL,
    [venues] varchar(50)  NULL,
    [bspOnly] bit  NULL,
    [inPlayOnly] bit  NULL,
    [marketBettingTypes] varchar(100)  NULL,
    [marketCountries] varchar(200)  NULL,
    [withOrders] varchar(50)  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [userName] nvarchar(100)  NOT NULL,
    [Password] nvarchar(32)  NOT NULL,
    [isActive] bit  NOT NULL,
    [appKey] nvarchar(50)  NULL,
    [certFileName] nvarchar(200)  NULL
);
GO

-- Creating table 'PlacedBets'
CREATE TABLE [dbo].[PlacedBets] (
    [betId] nvarchar(50)  NOT NULL,
    [resultCode] nvarchar(50)  NOT NULL,
    [sizeMatched] float  NULL,
    [success] bit  NOT NULL,
    [betlineId] int  NULL,
    [datePlaced] datetime  NOT NULL,
    [eventId] nvarchar(20)  NOT NULL,
    [marketMenuPath] nvarchar(200)  NULL,
    [averagePrice] float  NULL,
    [Selection] nvarchar(50)  NULL,
    [dateSettled] datetime  NULL,
    [score] nvarchar(20)  NULL,
    [sortedOrder] int  NULL,
    [marketName] nvarchar(20)  NULL,
    [eventType] int  NULL,
    [betType] nvarchar(20)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [BetlineId] in table 'Betlines'
ALTER TABLE [dbo].[Betlines]
ADD CONSTRAINT [PK_Betlines]
    PRIMARY KEY CLUSTERED ([BetlineId] ASC);
GO

-- Creating primary key on [FilterId] in table 'Filters'
ALTER TABLE [dbo].[Filters]
ADD CONSTRAINT [PK_Filters]
    PRIMARY KEY CLUSTERED ([FilterId] ASC);
GO

-- Creating primary key on [userName] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([userName] ASC);
GO

-- Creating primary key on [betId] in table 'PlacedBets'
ALTER TABLE [dbo].[PlacedBets]
ADD CONSTRAINT [PK_PlacedBets]
    PRIMARY KEY CLUSTERED ([betId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Filter_FilterId] in table 'Betlines'
ALTER TABLE [dbo].[Betlines]
ADD CONSTRAINT [FK_Filter1]
    FOREIGN KEY ([Filter_FilterId])
    REFERENCES [dbo].[Filters]
        ([FilterId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Filter1'
CREATE INDEX [IX_FK_Filter1]
ON [dbo].[Betlines]
    ([Filter_FilterId]);
GO

-- Creating foreign key on [ownersUserName] in table 'Betlines'
ALTER TABLE [dbo].[Betlines]
ADD CONSTRAINT [FK_OwnerUser]
    FOREIGN KEY ([ownersUserName])
    REFERENCES [dbo].[Users]
        ([userName])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_OwnerUser'
CREATE INDEX [IX_FK_OwnerUser]
ON [dbo].[Betlines]
    ([ownersUserName]);
GO

-- Creating foreign key on [betlineId] in table 'PlacedBets'
ALTER TABLE [dbo].[PlacedBets]
ADD CONSTRAINT [FK_Betline1]
    FOREIGN KEY ([betlineId])
    REFERENCES [dbo].[Betlines]
        ([BetlineId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Betline1'
CREATE INDEX [IX_FK_Betline1]
ON [dbo].[PlacedBets]
    ([betlineId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------