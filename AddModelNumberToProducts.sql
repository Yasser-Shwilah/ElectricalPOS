-- Add ModelNumber column to Products table (Optional field)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ModelNumber')
BEGIN
    ALTER TABLE Products ADD ModelNumber NVARCHAR(100) NULL;
    PRINT 'ModelNumber column added successfully.';
END
ELSE
BEGIN
    PRINT 'ModelNumber column already exists.';
END
GO
