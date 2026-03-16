-- Add CustomerId column to Sales table
-- This fixes the "Invalid column name 'CustomerId'" error

ALTER TABLE Sales ADD CustomerId INT NULL;

-- Note: Foreign key constraint skipped - Customers table does not exist in database
