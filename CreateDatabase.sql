-- Create the ElectricalPOS Database
-- Run this script in SQL Server Management Studio (SSMS) or via sqlcmd

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ElectricalPOS_DB')
BEGIN
    CREATE DATABASE ElectricalPOS_DB;
    PRINT 'Database ElectricalPOS_DB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database ElectricalPOS_DB already exists.';
END
GO

-- Switch to the database
USE ElectricalPOS_DB;
GO

-- Verify connection
SELECT DB_NAME() AS CurrentDatabase, SUSER_SNAME() AS CurrentUser;
GO
