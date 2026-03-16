# Arabic Encoding Fix - Complete Guide

## Problem Summary

Arabic text was displaying as garbled characters like: `Ø§Ù„ØªÙ‚Ø§Ø±ÙŠØ±`

This is caused by UTF-8 bytes being interpreted as Windows-1252/ISO-8859-1 encoding.

---

## Root Causes Identified

### 1. XAML File Encoding (PRIMARY ISSUE)
- Files were saved with incorrect encoding
- Missing UTF-8 BOM (Byte Order Mark)
- Solution: Files must be saved as **UTF-8 with BOM**

### 2. Database Column Types
- VARCHAR columns instead of NVARCHAR
- Solution: Use explicit `[Column(TypeName = "nvarchar(...)")]` in EF Core

### 3. Missing Unicode Configuration
- EF Core models lacked explicit NVARCHAR mapping
- Solution: Add `[Column(TypeName = "nvarchar(X)")]` to all string properties

---

## Fixes Applied

### A. Database Layer (SQL Script)

Run [`FixArabicEncoding.sql`](FixArabicEncoding.sql) to:
1. Check current column types
2. Convert VARCHAR to NVARCHAR
3. Verify database collation

Key SQL statements:
```sql
-- Convert VARCHAR to NVARCHAR
ALTER TABLE Products ALTER COLUMN Name NVARCHAR(200) NOT NULL;
ALTER TABLE Categories ALTER COLUMN Name NVARCHAR(100) NOT NULL;
ALTER TABLE Suppliers ALTER COLUMN Name NVARCHAR(200) NOT NULL;
ALTER TABLE Customers ALTER COLUMN Name NVARCHAR(200) NOT NULL;
ALTER TABLE Users ALTER COLUMN Username NVARCHAR(100) NOT NULL;
```

### B. EF Core Models (Updated)

All models now use explicit NVARCHAR:

```csharp
// Product.cs
[Column(TypeName = "nvarchar(200)")]  // Explicit NVARCHAR
public string Name { get; set; }

// Category.cs  
[Column(TypeName = "nvarchar(100)")]  // Explicit NVARCHAR
public string Name { get; set; }

// Supplier.cs
[Column(TypeName = "nvarchar(200)")]  // Explicit NVARCHAR
public string Name { get; set; }

// Customer.cs
[Column(TypeName = "nvarchar(200)")]  // Explicit NVARCHAR
public string Name { get; set; }

// User.cs
[Column(TypeName = "nvarchar(100)")]  // Explicit NVARCHAR
public string Username { get; set; }
```

### C. XAML Files (Fixed)

- ReportsView.xaml rewritten with proper Arabic text
- UTF-8 BOM preserved (﻿ at start)
- Correct Arabic fonts configured

### D. Font Configuration (Improved)

In App.xaml:
```xml
<FontFamily x:Key="ArabicFont">Segoe UI, Tahoma, Arial, Traditional Arabic</FontFamily>
```

---

## Correct Arabic Insert Statements

When inserting Arabic data directly in SQL, use N'' prefix:

```sql
-- CORRECT - Using N'' prefix for Unicode
INSERT INTO Categories (Name) VALUES (N'الأجهزة الكهربائية');
INSERT INTO Products (Name, CategoryId, PurchasePrice, SalePrice, Quantity) 
VALUES (N'غسالة سامسونج', 1, 500.00, 750.00, 10);
INSERT INTO Suppliers (Name, ContactInfo) VALUES (N'مورد الإلكترونات', N'دمشق - سوريا');

-- WRONG - Missing N'' prefix (causes garbled text)
INSERT INTO Categories (Name) VALUES ('الأجهزة الكهربائية');
```

---

## Best Practices to Avoid This Issue

### 1. File Encoding (VS Code)

**For all .xaml and .cs files containing Arabic:**

1. Open file in VS Code
2. Click encoding in bottom-right corner
3. Select "UTF-8 with BOM"

Or use command palette: `> Change File Encoding` → `UTF-8 with BOM`

**VS Code Settings (settings.json):**
```json
{
    "files.encoding": "utf8bom",
    "files.autoGuessEncoding": true
}
```

### 2. EF Core Configuration

**Always use explicit NVARCHAR:**
```csharp
[Column(TypeName = "nvarchar(200)")]
public string Name { get; set; }
```

**Or configure globally in DbContext:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        var stringProperties = entityType.ClrType.GetProperties()
            .Where(p => p.PropertyType == typeof(string));
        
        foreach (var property in stringProperties)
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(property.Name)
                .HasColumnType("nvarchar(max)");
        }
    }
}
```

### 3. Database Configuration

**Use Arabic-safe collation:**
```sql
-- Database level
ALTER DATABASE YourDatabase COLLATE Arabic_CI_AS;

-- Column level (when creating tables)
CREATE TABLE Products (
    Name NVARCHAR(200) COLLATE Arabic_CI_AS NOT NULL
);
```

### 4. Application Settings

**In App.xaml.cs or Program.cs:**
```csharp
// Ensure SQL connection uses Unicode
var connectionString = "Server=...;Database=...;Trusted_Connection=True;" +
                       "TrustServerCertificate=True;" +
                       "MultipleActiveResultSets=true;";

// The .NET SqlClient handles Unicode automatically with NVARCHAR
```

### 5. Font Selection

For best Arabic rendering in WPF:
```xml
<!-- Priority: Best to Good -->
<FontFamily>Segoe UI, Tahoma, Traditional Arabic, Arial</FontFamily>
```

- **Segoe UI**: Windows 10/11 built-in, excellent Arabic support
- **Tahoma**: Good Arabic support, widely available
- **Traditional Arabic**: Classic Arabic font
- **Arial**: Fallback, basic Arabic support

---

## Testing the Fix

### 1. Database Test
```sql
-- Insert Arabic and read back
INSERT INTO Categories (Name) VALUES (N'اختبار العربية');
SELECT * FROM Categories;
-- Should display: اختبار العربية
```

### 2. Application Test
1. Run the application
2. Navigate to Reports section
3. Verify Arabic text displays correctly
4. Add new product/category with Arabic name
5. Verify it saves and displays correctly

### 3. Check DataGrid
- Open Reports → Sales tab
- Verify Arabic product names display properly
- Check Supplier names in Purchases tab

---

## Summary of Changes

| File | Change |
|------|--------|
| `FixArabicEncoding.sql` | New SQL script for DB fixes |
| `Models/Product.cs` | Added `[Column(TypeName = "nvarchar(200)")]` |
| `Models/Category.cs` | Added `[Column(TypeName = "nvarchar(100)")]` |
| `Models/Supplier.cs` | Added `[Column(TypeName = "nvarchar(200)")]` |
| `Models/Customer.cs` | Added `[Column(TypeName = "nvarchar(200)")]` |
| `Models/User.cs` | Added `[Column(TypeName = "nvarchar(100)")]` |
| `Views/ReportsView.xaml` | Rewritten with correct Arabic encoding |
| `App.xaml` | Improved Arabic font configuration |

---

## Next Steps

1. **Run the SQL script** in SQL Server Management Studio:
   ```
   Open FixArabicEncoding.sql → Execute
   ```

2. **Rebuild the application**:
   ```
   dotnet build
   ```

3. **Test the Reports section** - Arabic should display correctly

4. **For new data entry** - Test adding products/categories with Arabic names

---

*Document generated for ElectricalPOS v1.0*
*Date: 2026-03-02*
