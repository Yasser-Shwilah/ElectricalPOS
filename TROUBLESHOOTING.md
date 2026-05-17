# Troubleshooting Guide - Electrical POS

## Login Issue: "Invalid username or password"

If you're getting "Invalid username or password" error with admin/admin123, follow these steps:

### Solution 1: Delete and Recreate Database

1. **Open SQL Server Management Studio (SSMS)** or use SQL command:
   ```sql
   USE master;
   GO
   DROP DATABASE IF EXISTS ElectricalPOS_DB;
   GO
   ```

2. **Run the application again** - it will automatically create the database and admin user

### Solution 2: Manually Verify Database

1. **Check if database exists**:
   ```sql
   SELECT name FROM sys.databases WHERE name = 'ElectricalPOS_DB';
   ```

2. **Check if admin user exists**:
   ```sql
   USE ElectricalPOS_DB;
   SELECT * FROM Users WHERE Username = 'admin';
   ```

3. **If no admin user, create one manually**:
   ```sql
   USE ElectricalPOS_DB;
   
   -- The password hash for "admin123" using SHA256
   INSERT INTO Users (Username, Password, Role)
   VALUES ('admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Admin');
   ```

### Solution 3: Check SQL Server Connection

1. **Verify SQL Server is running**:
   - Open Services (services.msc)
   - Look for "SQL Server (MSSQLSERVER)" or your instance name
   - Ensure it's running

2. **Test connection string**:
   - Open [`ElectricalPOSContext.cs`](Models/ElectricalPOSContext.cs)
   - Line 16: Update server name if needed
   ```csharp
   Server=DESKTOP-YASSER9  // Change this to your server name
   ```

3. **Common server names**:
   - Local: `localhost` or `(localdb)\MSSQLLocalDB`
   - Named instance: `.\SQLEXPRESS`
   - Your computer name: Check in SQL Server Configuration Manager

### Solution 4: Check Application Startup

1. **Look for error messages** when the app starts
2. **If you see "Database initialization error"**:
   - SQL Server is not accessible
   - Connection string is incorrect
   - Windows Authentication is not enabled

### Solution 5: Enable Detailed Logging

Add this to [`App.xaml.cs`](App.xaml.cs) in the `InitializeDatabase()` method:

```csharp
try
{
    using var context = new ElectricalPOSContext();
    
    // Test connection
    if (context.Database.CanConnect())
    {
        MessageBox.Show("Database connection successful!");
    }
    else
    {
        MessageBox.Show("Cannot connect to database!");
        return;
    }
    
    // Create database
    context.Database.EnsureCreated();
    MessageBox.Show("Database created/verified!");
    
    // Check admin user
    var adminExists = context.Users.Any(u => u.Username == "admin");
    MessageBox.Show($"Admin user exists: {adminExists}");
    
    if (!adminExists)
    {
        var adminUser = new User
        {
            Username = "admin",
            Password = PasswordHasher.HashPassword("admin123"),
            Role = "Admin"
        };
        context.Users.Add(adminUser);
        context.SaveChanges();
        MessageBox.Show("Admin user created!");
    }
}
catch (Exception ex)
{
    MessageBox.Show($"Error: {ex.Message}\n\nInner: {ex.InnerException?.Message}");
}
```

### Solution 6: Verify Password Hash

The password "admin123" should hash to: `jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=`

You can verify this by adding a test in the login:

```csharp
var testHash = PasswordHasher.HashPassword("admin123");
MessageBox.Show($"Test hash: {testHash}");
```

### Solution 7: Fresh Start

1. **Delete the database**:
   ```sql
   DROP DATABASE ElectricalPOS_DB;
   ```

2. **Delete bin and obj folders**:
   ```bash
   cd ElectricalPOS
   rmdir /s /q bin
   rmdir /s /q obj
   ```

3. **Rebuild**:
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

## Other Common Issues

### Issue: Application won't start

**Solution**: Check if .NET 8 SDK is installed
```bash
dotnet --version
```

### Issue: Build errors

**Solution**: Restore NuGet packages
```bash
dotnet restore
dotnet clean
dotnet build
```

### Issue: MaterialDesign theme not loading

**Solution**: The App.xaml has been updated to use MaterialDesign3.Defaults.xaml

### Issue: Database connection timeout

**Solution**: 
1. Check SQL Server is running
2. Check firewall settings
3. Verify Windows Authentication is enabled

## Quick Test Script

Run this in SQL Server Management Studio to test everything:

```sql
-- Check if database exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ElectricalPOS_DB')
BEGIN
    PRINT 'Database exists'
    
    USE ElectricalPOS_DB;
    
    -- Check tables
    SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
    
    -- Check admin user
    SELECT * FROM Users WHERE Username = 'admin';
    
    -- Check categories
    SELECT * FROM Categories;
END
ELSE
BEGIN
    PRINT 'Database does not exist - run the application to create it'
END
```

## Still Having Issues?

1. **Check the Output window** in Visual Studio for detailed error messages
2. **Enable SQL Server logging** to see connection attempts
3. **Try connecting with SQL Server Management Studio first** to verify connectivity

### Issue: "Invalid object name 'Purchases'"

**Solution**: This error occurs when the Purchases table doesn't exist in the database. The application should automatically create the table on startup. If you still see this error:

1. **Restart the application** - the database initialization should now create the missing tables

2. **Manually run the SQL script**:
   - Open SQL Server Management Studio
   - Connect to your SQL Server instance
   - Run the script at `ElectricalPOS\CreatePurchasesTable.sql`

3. **As a last resort, delete and recreate the database**:
   ```sql
   USE master;
   GO
   DROP DATABASE IF EXISTS ElectricalPOS_DB;
   GO
   ```
   Then restart the application - it will recreate the database with all tables.

## Contact Information

If none of these solutions work:
1. Check the database server name is correct
2. Verify SQL Server service is running
3. Ensure Windows Authentication is enabled
4. Try connecting with SQL Server Management Studio first to verify connectivity
