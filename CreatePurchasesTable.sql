-- Script to create Purchases and PurchaseItems tables
-- Run this script against ElectricalPOS_DB database

-- Create Purchases table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Purchases')
BEGIN
    CREATE TABLE Purchases (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PurchaseDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        SupplierId INT NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Debt DECIMAL(18,2) NOT NULL DEFAULT 0,
        Notes NVARCHAR(500) NULL,
        CONSTRAINT FK_Purchases_Suppliers FOREIGN KEY (SupplierId) 
            REFERENCES Suppliers(Id) ON DELETE NO ACTION
    );

    -- Create index for SupplierId
    CREATE INDEX IX_Purchases_SupplierId ON Purchases(SupplierId);

    PRINT 'Purchases table created successfully.';
END
ELSE
BEGIN
    PRINT 'Purchases table already exists.';
END
GO

-- Create PurchaseItems table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PurchaseItems')
BEGIN
    CREATE TABLE PurchaseItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PurchaseId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT FK_PurchaseItems_Purchases FOREIGN KEY (PurchaseId) 
            REFERENCES Purchases(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_PurchaseItems_Products FOREIGN KEY (ProductId) 
            REFERENCES Products(Id) ON DELETE NO ACTION
    );

    -- Create indexes
    CREATE INDEX IX_PurchaseItems_PurchaseId ON PurchaseItems(PurchaseId);
    CREATE INDEX IX_PurchaseItems_ProductId ON PurchaseItems(ProductId);

    PRINT 'PurchaseItems table created successfully.';
END
ELSE
BEGIN
    PRINT 'PurchaseItems table already exists.';
END
GO

-- Verify tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Purchases', 'PurchaseItems')
ORDER BY TABLE_NAME;
GO

-- Show Purchases table schema
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Purchases'
ORDER BY ORDINAL_POSITION;
GO

-- Show PurchaseItems table schema
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PurchaseItems'
ORDER BY ORDINAL_POSITION;
GO
