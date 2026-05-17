-- ============================================================================
-- Arabic Encoding Fix Script for ElectricalPOS Database
-- ============================================================================
-- This script checks and fixes encoding issues for Arabic text support
-- Run this in SQL Server Management Studio or Azure Data Studio
-- ============================================================================

USE ElectricalPOS_DB;
GO

-- ============================================================================
-- STEP 1: Check current column types and collation
-- ============================================================================

PRINT '=== Current String Column Configuration ===';
GO

-- Check Products table
SELECT 
    'Products' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
    AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext');

-- Check Categories table
SELECT 
    'Categories' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Categories' 
    AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext');

-- Check Suppliers table
SELECT 
    'Suppliers' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Suppliers' 
    AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext');

-- Check Customers table
SELECT 
    'Customers' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
    AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext');

-- Check Users table
SELECT 
    'Users' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
    AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext');

GO

-- ============================================================================
-- STEP 2: Fix VARCHAR columns to NVARCHAR (if needed)
-- ============================================================================

PRINT '=== Converting VARCHAR to NVARCHAR ===';
GO

-- Fix Products table - Name column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Name' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Products ALTER COLUMN Name NVARCHAR(200) NOT NULL;
    PRINT 'Products.Name converted to NVARCHAR(200)';
END
GO

-- Fix Categories table - Name column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'Name' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Categories ALTER COLUMN Name NVARCHAR(100) NOT NULL;
    PRINT 'Categories.Name converted to NVARCHAR(100)';
END
GO

-- Fix Suppliers table - Name column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'Name' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Suppliers ALTER COLUMN Name NVARCHAR(200) NOT NULL;
    PRINT 'Suppliers.Name converted to NVARCHAR(200)';
END
GO

-- Fix Suppliers table - Address column (if exists)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'Address' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Suppliers ALTER COLUMN Address NVARCHAR(MAX);
    PRINT 'Suppliers.Address converted to NVARCHAR(MAX)';
END
GO

-- Fix Customers table - Name column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Customers' AND COLUMN_NAME = 'Name' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Customers ALTER COLUMN Name NVARCHAR(200) NOT NULL;
    PRINT 'Customers.Name converted to NVARCHAR(200)';
END
GO

-- Fix Customers table - Phone column (if exists)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Customers' AND COLUMN_NAME = 'Phone' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Customers ALTER COLUMN Phone NVARCHAR(50);
    PRINT 'Customers.Phone converted to NVARCHAR(50)';
END
GO

-- Fix Customers table - Address column (if exists)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Customers' AND COLUMN_NAME = 'Address' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Customers ALTER COLUMN Address NVARCHAR(MAX);
    PRINT 'Customers.Address converted to NVARCHAR(MAX)';
END
GO

-- Fix Users table - Username column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Username' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Users ALTER COLUMN Username NVARCHAR(100) NOT NULL;
    PRINT 'Users.Username converted to NVARCHAR(100)';
END
GO

-- Fix Users table - FullName column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'FullName' 
           AND DATA_TYPE = 'varchar')
BEGIN
    ALTER TABLE Users ALTER COLUMN FullName NVARCHAR(200);
    PRINT 'Users.FullName converted to NVARCHAR(200)';
END
GO

-- ============================================================================
-- STEP 3: Change database collation to Arabic_CI_AS (if needed)
-- ============================================================================

PRINT '=== Checking Database Collation ===';
GO

DECLARE @DBCollation NVARCHAR(128);
SELECT @DBCollation = collation_name FROM sys.databases WHERE name = 'ElectricalPOS_DB';

PRINT 'Current Database Collation: ' + @DBCollation;

-- Uncomment the following line if you want to change collation:
-- ALTER DATABASE ElectricalPOS_DB COLLATE Arabic_CI_AS;
-- PRINT 'Database collation changed to Arabic_CI_AS';

GO

-- ============================================================================
-- STEP 4: Verify the fixes
-- ============================================================================

PRINT '=== Verification After Fixes ===';
GO

SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE DATA_TYPE IN ('varchar', 'nvarchar')
ORDER BY TABLE_NAME, COLUMN_NAME;

GO

-- ============================================================================
-- STEP 5: Test Arabic Insert (example)
-- ============================================================================

PRINT '=== Testing Arabic Insert ===';
GO

-- Example correct insert with N'' prefix for Unicode
-- INSERT INTO Categories (Name) VALUES (N'الأجهزة الكهربائية');
-- INSERT INTO Products (Name, CategoryId, PurchasePrice, SalePrice, Quantity) 
-- VALUES (N'غسالة سامسونج、自动洗衣机', 1, 500.00, 750.00, 10);

PRINT 'Script completed successfully!';
GO
