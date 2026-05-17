-- Create StockMovements table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockMovements')
BEGIN
    CREATE TABLE StockMovements (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        MovementType NVARCHAR(50) NOT NULL,
        Quantity INT NOT NULL,
        Date DATETIME NOT NULL,
        RelatedDocument NVARCHAR(200) NULL,
        CONSTRAINT FK_StockMovements_Products FOREIGN KEY (ProductId) 
            REFERENCES Products(Id) ON DELETE NO ACTION
    );
    
    PRINT 'StockMovements table created successfully.';
END
ELSE
BEGIN
    PRINT 'StockMovements table already exists.';
END
GO
