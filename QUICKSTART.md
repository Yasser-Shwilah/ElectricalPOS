# Quick Start Guide - Electrical POS System

## 🚀 Getting Started in 5 Minutes

### Step 1: Prerequisites Check
- ✅ .NET 8 SDK installed
- ✅ SQL Server running (Express, Developer, or Full)
- ✅ Visual Studio 2022 or VS Code

### Step 2: Configure Database Connection

Open [`Models/ElectricalPOSContext.cs`](Models/ElectricalPOSContext.cs) and update line 16:

```csharp
optionsBuilder.UseSqlServer(@"Server=YOUR_SERVER_NAME;Database=ElectricalPOS_DB;Trusted_Connection=True;TrustServerCertificate=True;");
```

**Common Server Names:**
- Local SQL Server: `localhost` or `(localdb)\MSSQLLocalDB`
- Named Instance: `DESKTOP-YASSER9` (as configured)
- SQL Express: `.\SQLEXPRESS`

### Step 3: Build and Run

```bash
cd ElectricalPOS
dotnet build
dotnet run
```

Or press **F5** in Visual Studio.

### Step 4: First Login

The application will automatically:
1. Create the database
2. Create tables with proper relationships
3. Seed default admin user and categories

**Login Credentials:**
- Username: `admin`
- Password: `admin123`

### Step 5: Start Using

#### As Admin:
1. **Add Categories** (optional - defaults already loaded)
   - Navigate to Categories menu
   - Add custom categories for your products

2. **Add Products**
   - Navigate to Products menu
   - Fill in product details
   - Set purchase and sale prices
   - Set initial quantity

3. **Create Users**
   - Navigate to Users menu
   - Add accountant users
   - They can only access Sales and Products

4. **Process Sales**
   - Navigate to Sales menu
   - Select products
   - Add to cart
   - Apply discount if needed
   - Complete sale

#### As Accountant:
1. View available products
2. Create sales invoices
3. View sales history

## 📊 Database Schema

The application creates these tables automatically:

```
Users (Id, Username, Password, Role)
Categories (Id, Name)
Products (Id, Name, CategoryId, PurchasePrice, SalePrice, Quantity)
Sales (Id, Date, TotalAmount, Discount, Profit, AccountantId)
SaleItems (Id, SaleId, ProductId, Quantity, UnitPrice, TotalPrice)
```

## 🎨 Features Overview

### Product Management
- ✅ Add/Edit/Delete products
- ✅ Categorize products
- ✅ Track purchase and sale prices
- ✅ Real-time stock management

### Sales Management
- ✅ Quick invoice creation
- ✅ Multi-product cart
- ✅ Discount application
- ✅ Automatic profit calculation
- ✅ Automatic stock updates
- ✅ Sales history with full details

### User Management (Admin Only)
- ✅ Create accountant users
- ✅ Role-based access control
- ✅ Secure password hashing

### Category Management (Admin Only)
- ✅ Create/Edit/Delete categories
- ✅ Pre-loaded defaults

## 🔧 Troubleshooting

### Database Connection Failed
```
Error: Cannot connect to database
```
**Solution:** 
1. Verify SQL Server is running
2. Check server name in connection string
3. Ensure Windows Authentication is enabled

### Build Errors
```
Error: Package not found
```
**Solution:**
```bash
dotnet restore
dotnet clean
dotnet build
```

### Login Not Working
```
Error: Invalid credentials
```
**Solution:**
1. Ensure database was created successfully
2. Check if admin user exists in Users table
3. Try resetting: Delete database and restart app

## 📁 Project Structure

```
ElectricalPOS/
├── Models/              # Database entities & DbContext
├── ViewModels/          # MVVM ViewModels with business logic
├── Views/               # XAML UI files
├── Services/            # Data access services
├── Utilities/           # Helper classes (PasswordHasher, Converters)
├── App.xaml            # Application resources & startup
└── README.md           # Full documentation
```

## 🎯 Next Steps

1. **Customize Categories**: Add your specific product categories
2. **Add Products**: Populate your inventory
3. **Create Users**: Add accountant users for your team
4. **Start Selling**: Process your first sale!

## 💡 Tips

- **Keyboard Shortcuts**: Tab through form fields for quick data entry
- **Stock Alerts**: Products with 0 quantity won't appear in sales dropdown
- **Profit Tracking**: Each sale automatically calculates profit
- **Role Security**: Accountants can't access Users or Categories

## 📞 Support

For issues or questions:
1. Check the full [README.md](README.md)
2. Review code comments in source files
3. Check database connection and permissions

## 🔐 Security Notes

- Passwords are hashed using SHA256
- Role-based UI restrictions
- Database foreign key constraints
- Input validation on all forms

---

**Ready to go!** 🎉 Start the application and begin managing your electrical products business.
