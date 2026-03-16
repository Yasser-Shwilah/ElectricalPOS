-- Fix Admin Password Script
-- This script updates the admin user password to the correct hash for "admin123"

USE ElectricalPOS_DB;
GO

-- Check current admin user
SELECT * FROM Users WHERE Username = 'admin';
GO

-- Update admin password to correct hash for "admin123"
-- The hash is: jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
UPDATE Users 
SET Password = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg='
WHERE Username = 'admin';
GO

-- Verify the update
SELECT * FROM Users WHERE Username = 'admin';
GO

PRINT 'Admin password has been updated successfully!';
PRINT 'You can now login with:';
PRINT 'Username: admin';
PRINT 'Password: admin123';
