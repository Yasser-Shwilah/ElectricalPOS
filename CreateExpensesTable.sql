-- Create Expenses table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Expenses')
BEGIN
    CREATE TABLE Expenses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Date DATETIME NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL
    );
    
    PRINT 'Expenses table created successfully.';
END
ELSE
BEGIN
    PRINT 'Expenses table already exists.';
END
GO
