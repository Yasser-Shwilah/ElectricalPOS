-- Fix for "Invalid column name 'FullName'" error
-- Run this script in SQL Server Management Studio

USE master;
GO

-- Drop the database if it exists
DROP DATABASE IF EXISTS ElectricalPOS_DB;
GO

-- Create fresh database
CREATE DATABASE ElectricalPOS_DB;
GO

USE ElectricalPOS_DB;
GO

-- Create Users table with FullName column
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Password NVARCHAR(256) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NULL
);
GO

-- Create Categories table
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

-- Create Products table
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Barcode NVARCHAR(50) NULL,
    CategoryId INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CostPrice DECIMAL(18,2) NOT NULL,
    QuantityInStock INT NOT NULL,
    MinimumStock INT NOT NULL,
    Voltage NVARCHAR(50) NULL,
    Wattage NVARCHAR(50) NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
GO

-- Create Customers table
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    ContactInfo NVARCHAR(500) NULL,
    TotalDebt DECIMAL(18,2) DEFAULT 0
);
GO

-- Create Suppliers table
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL
);
GO

-- Create Sales table
CREATE TABLE Sales (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(18,2) DEFAULT 0,
    Profit DECIMAL(18,2) NOT NULL,
    AccountantId INT NOT NULL,
    CustomerId INT NULL,
    PaymentMethod NVARCHAR(50) NULL,
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (AccountantId) REFERENCES Users(Id),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);
GO

-- Create SaleItems table
CREATE TABLE SaleItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- Create Purchases table
CREATE TABLE Purchases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseDate DATETIME2 NOT NULL,
    SupplierId INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
);
GO

-- Create PurchaseItems table
CREATE TABLE PurchaseItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (PurchaseId) REFERENCES Purchases(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- Create StockMovements table
CREATE TABLE StockMovements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    MovementType NVARCHAR(20) NOT NULL,
    Quantity INT NOT NULL,
    MovementDate DATETIME2 NOT NULL,
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- Create CashTransactions table
CREATE TABLE CashTransactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TransactionDate DATETIME2 NOT NULL,
    TransactionType NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL
);
GO

-- Create Expenses table
CREATE TABLE Expenses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Category NVARCHAR(100) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Date DATETIME2 NOT NULL,
    Description NVARCHAR(500) NULL
);
GO

-- Create default admin user (password: admin123)
-- Using SHA256 hash: jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
INSERT INTO Users (Username, Password, Role, FullName)
VALUES ('admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Admin', 'مدير النظام');
GO

-- Create default categories
INSERT INTO Categories (Name) VALUES ('Cables');
INSERT INTO Categories (Name) VALUES ('Switches');
INSERT INTO Categories (Name) VALUES ('Lights');
INSERT INTO Categories (Name) VALUES ('Tools');
INSERT INTO Categories (Name) VALUES ('Others');
GO

PRINT 'Database created successfully with FullName column!';
PRINT 'You can now run the application.';
