-- Fix for SqlNullValueException on IsPaid column
-- This script updates any NULL values in the IsPaid column to 0 (false)

-- Update null values to false
UPDATE Purchases SET IsPaid = 0 WHERE IsPaid IS NULL;
PRINT 'Updated NULL IsPaid values to 0';

-- Ensure all purchases with debt = 0 are marked as paid
UPDATE Purchases SET IsPaid = 1 WHERE Debt = 0 AND IsPaid = 0;
PRINT 'Updated IsPaid for fully paid purchases';
