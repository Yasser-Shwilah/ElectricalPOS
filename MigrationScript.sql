IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [CashTransactions] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_CashTransactions] PRIMARY KEY ([Id])
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [ContactInfo] nvarchar(500) NOT NULL,
    [TotalDebt] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);

CREATE TABLE [Expenses] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id])
);

CREATE TABLE [Suppliers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [ContactInfo] nvarchar(500) NOT NULL,
    [TotalDebt] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(100) NOT NULL,
    [Password] nvarchar(256) NOT NULL,
    [Role] nvarchar(50) NOT NULL,
    [FullName] nvarchar(200) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [CategoryId] int NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SalePrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Purchases] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseDate] datetime2 NOT NULL,
    [SupplierId] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [Debt] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Purchases] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Purchases_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Sales] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [Profit] decimal(18,2) NOT NULL,
    [AccountantId] int NOT NULL,
    [CustomerId] int NULL,
    CONSTRAINT [PK_Sales] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Sales_Users_AccountantId] FOREIGN KEY ([AccountantId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StockMovements] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [MovementType] nvarchar(50) NOT NULL,
    [Quantity] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [RelatedDocument] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PurchaseItems] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PurchaseItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PurchaseItems_Purchases_PurchaseId] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchases] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SaleItems] (
    [Id] int NOT NULL IDENTITY,
    [SaleId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_SaleItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SaleItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SaleItems_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE INDEX [IX_PurchaseItems_ProductId] ON [PurchaseItems] ([ProductId]);

CREATE INDEX [IX_PurchaseItems_PurchaseId] ON [PurchaseItems] ([PurchaseId]);

CREATE INDEX [IX_Purchases_SupplierId] ON [Purchases] ([SupplierId]);

CREATE INDEX [IX_SaleItems_ProductId] ON [SaleItems] ([ProductId]);

CREATE INDEX [IX_SaleItems_SaleId] ON [SaleItems] ([SaleId]);

CREATE INDEX [IX_Sales_AccountantId] ON [Sales] ([AccountantId]);

CREATE INDEX [IX_Sales_CustomerId] ON [Sales] ([CustomerId]);

CREATE INDEX [IX_StockMovements_ProductId] ON [StockMovements] ([ProductId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260303080106_InitialCreate', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260303080303_AddMissingTables', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260303080911_CreateAllTables', N'10.0.3');

COMMIT;
GO

