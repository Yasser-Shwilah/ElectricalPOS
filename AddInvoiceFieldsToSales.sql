-- ============================================
-- Migration Script: Add Invoice Fields to Sales Table
-- Description: Adds invoice number, customer info, and balance tracking to Sales table
-- ============================================

-- 1. Add InvoiceNumber column (unique invoice identifier)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'InvoiceNumber')
BEGIN
    ALTER TABLE Sales ADD InvoiceNumber NVARCHAR(50) NULL;
    PRINT 'Column InvoiceNumber added successfully';
END
ELSE
BEGIN
    PRINT 'Column InvoiceNumber already exists';
END

-- 2. Add CustomerName column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'CustomerName')
BEGIN
    ALTER TABLE Sales ADD CustomerName NVARCHAR(200) NULL;
    PRINT 'Column CustomerName added successfully';
END
ELSE
BEGIN
    PRINT 'Column CustomerName already exists';
END

-- 3. Add CustomerPhone column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'CustomerPhone')
BEGIN
    ALTER TABLE Sales ADD CustomerPhone NVARCHAR(50) NULL;
    PRINT 'Column CustomerPhone added successfully';
END
ELSE
BEGIN
    PRINT 'Column CustomerPhone already exists';
END

-- 4. Add PreviousBalance column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'PreviousBalance')
BEGIN
    ALTER TABLE Sales ADD PreviousBalance DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column PreviousBalance added successfully';
END
ELSE
BEGIN
    PRINT 'Column PreviousBalance already exists';
END

-- 5. Add CurrentBalance column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'CurrentBalance')
BEGIN
    ALTER TABLE Sales ADD CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column CurrentBalance added successfully';
END
ELSE
BEGIN
    PRINT 'Column CurrentBalance already exists';
END

-- 6. Add Notes column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'Notes')
BEGIN
    ALTER TABLE Sales ADD Notes NVARCHAR(500) NULL;
    PRINT 'Column Notes added successfully';
END
ELSE
BEGIN
    PRINT 'Column Notes already exists';
END

-- 7. Create unique index on InvoiceNumber
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sales_InvoiceNumber' AND object_id = OBJECT_ID('Sales'))
BEGIN
    CREATE UNIQUE INDEX IX_Sales_InvoiceNumber ON Sales(InvoiceNumber) WHERE InvoiceNumber IS NOT NULL;
    PRINT 'Unique index IX_Sales_InvoiceNumber created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_Sales_InvoiceNumber already exists';
END

-- 8. Generate invoice numbers for existing sales (if InvoiceNumber is null)
UPDATE Sales 
SET InvoiceNumber = 'INV-' + CONVERT(VARCHAR, Date, 112) + '-' + RIGHT('0000' + CAST(Id AS VARCHAR(4)), 4)
WHERE InvoiceNumber IS NULL;

PRINT '';
PRINT 'Migration completed successfully!';
PRINT 'The Sales table has been updated with invoice fields.';
