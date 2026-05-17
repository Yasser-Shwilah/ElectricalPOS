-- Script to fix Purchases table schema
-- Run this script against ElectricalPOS_DB database

-- Add missing PaidAmount column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Purchases' AND COLUMN_NAME = 'PaidAmount')
BEGIN
    ALTER TABLE Purchases ADD PaidAmount decimal(18,2) NOT NULL DEFAULT 0;
END
GO

-- Add missing Debt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Purchases' AND COLUMN_NAME = 'Debt')
BEGIN
    ALTER TABLE Purchases ADD Debt decimal(18,2) NOT NULL DEFAULT 0;
END
GO

-- Verify the table schema
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Purchases'
ORDER BY ORDINAL_POSITION;
GO
