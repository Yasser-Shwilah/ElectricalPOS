-- =====================================================
-- Fix: Add FullName column to Users table
-- Error: Invalid column name 'FullName'
-- =====================================================

-- Step 1: Check if column exists before adding
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'FullName'
)
BEGIN
    ALTER TABLE Users
    ADD FullName NVARCHAR(200) NULL;
    
    PRINT 'FullName column added successfully to Users table.';
END
ELSE
BEGIN
    PRINT 'FullName column already exists in Users table.';
END

-- Step 2: Verify the column was added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;
