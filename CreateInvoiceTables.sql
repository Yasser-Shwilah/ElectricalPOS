-- =====================================================
-- Create Invoice and InvoiceItem Tables
-- جدول الفواتير وجدول تفاصيل الفاتورة
-- =====================================================

-- Create Invoices table (جدول الفواتير)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Invoices] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [InvoiceNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [InvoiceDate] DATETIME NOT NULL,
        [CustomerName] NVARCHAR(200) NOT NULL,
        [CustomerPhone] NVARCHAR(50) NULL,
        [Total] DECIMAL(18,2) NOT NULL,
        [PreviousBalance] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [CurrentBalance] DECIMAL(18,2) NOT NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    PRINT 'Created Invoices table successfully.';
END
ELSE
BEGIN
    PRINT 'Invoices table already exists.';
END
GO

-- Create InvoiceItems table (جدول تفاصيل الفاتورة)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InvoiceItems] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [InvoiceId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [ProductName] NVARCHAR(200) NOT NULL,
        [Quantity] INT NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [Subtotal] DECIMAL(18,2) NOT NULL,
        CONSTRAINT [FK_InvoiceItems_Invoices] FOREIGN KEY ([InvoiceId]) 
            REFERENCES [dbo].[Invoices] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InvoiceItems_Products] FOREIGN KEY ([ProductId]) 
            REFERENCES [dbo].[Products] ([Id]) ON DELETE RESTRICT
    );
    
    PRINT 'Created InvoiceItems table successfully.';
END
ELSE
BEGIN
    PRINT 'InvoiceItems table already exists.';
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_InvoiceNumber' AND object_id = OBJECT_ID('dbo.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Invoices_InvoiceNumber] ON [dbo].[Invoices]([InvoiceNumber]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_InvoiceDate' AND object_id = OBJECT_ID('dbo.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Invoices_InvoiceDate] ON [dbo].[Invoices]([InvoiceDate]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_InvoiceItems_InvoiceId' AND object_id = OBJECT_ID('dbo.InvoiceItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_InvoiceItems_InvoiceId] ON [dbo].[InvoiceItems]([InvoiceId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_InvoiceItems_ProductId' AND object_id = OBJECT_ID('dbo.InvoiceItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_InvoiceItems_ProductId] ON [dbo].[InvoiceItems]([ProductId]);
END
GO

PRINT 'Database tables created successfully.';
GO
