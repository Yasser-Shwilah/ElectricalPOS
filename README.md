# Electrical POS System

A complete WPF Desktop Point of Sale application for electrical products built with C# and .NET 8.

## Features

### User Management
- **Admin Role**: Full access to all features
- **Accountant Role**: Limited access (Sales and Products only)
- Secure password hashing
- Default admin credentials: `admin` / `admin123`

### Product Management
- Add, edit, and delete products
- Assign products to categories
- Track purchase price, sale price, and quantity
- Real-time stock management

### Sales Management
- Create sales invoices quickly
- Add multiple products to cart
- Apply discounts to invoices
- Automatic profit calculation
- Automatic stock updates after sales
- View sales history with details

### Category Management
- Manage product categories
- Pre-loaded default categories (Cables, Switches, Lights, Tools, Others)

## Technology Stack

- **Framework**: .NET 8 WPF
- **Database**: SQL Server (LocalDB or Full SQL Server)
- **ORM**: Entity Framework Core 10.0
- **UI Framework**: MaterialDesignThemes
- **MVVM**: CommunityToolkit.Mvvm
- **Architecture**: MVVM Pattern

## Database Configuration

The application is configured to connect to:
- **Server**: DESKTOP-YASSER9
- **Database**: ElectricalPOS_DB
- **Authentication**: Windows Authentication (Trusted_Connection)

To change the connection string, edit the `OnConfiguring` method in [`ElectricalPOSContext.cs`](ElectricalPOS/Models/ElectricalPOSContext.cs:16).

## Setup Instructions

### Prerequisites
1. Visual Studio 2022 or later
2. .NET 8 SDK
3. SQL Server (Express, Developer, or Full edition)

### Installation Steps

1. **Clone or extract the project**

2. **Update Database Connection**
   - Open `Models/ElectricalPOSContext.cs`
   - Update the connection string if needed:
   ```csharp
   optionsBuilder.UseSqlServer(@"Server=YOUR_SERVER_NAME;Database=ElectricalPOS_DB;Trusted_Connection=True;TrustServerCertificate=True;");
   ```

3. **Create the Database**
   
   Option A: Using Package Manager Console in Visual Studio
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```

   Option B: The application will automatically create the database on first run

4. **Build and Run**
   ```bash
   dotnet build
   dotnet run
   ```

5. **Login**
   - Username: `admin`
   - Password: `admin123`

## Project Structure

```
ElectricalPOS/
├── Models/              # Database entities
│   ├── User.cs
│   ├── Product.cs
│   ├── Category.cs
│   ├── Sale.cs
│   ├── SaleItem.cs
│   └── ElectricalPOSContext.cs
├── ViewModels/          # MVVM ViewModels
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs
│   ├── ProductViewModel.cs
│   ├── SalesViewModel.cs
│   ├── UserViewModel.cs
│   └── CategoryViewModel.cs
├── Views/               # XAML Views
│   ├── LoginWindow.xaml
│   ├── MainWindow.xaml
│   ├── ProductManagementView.xaml
│   ├── SalesManagementView.xaml
│   ├── UserManagementView.xaml
│   └── CategoryManagementView.xaml
├── Services/            # Business logic services
│   ├── UserService.cs
│   ├── ProductService.cs
│   ├── CategoryService.cs
│   └── SaleService.cs
├── Utilities/           # Helper classes
│   ├── PasswordHasher.cs
│   └── RelayCommand.cs
└── App.xaml            # Application entry point
```

## Usage Guide

### Admin Functions
1. **Manage Users**: Create accountant users, delete users
2. **Manage Categories**: Add, edit, delete product categories
3. **Manage Products**: Full CRUD operations on products
4. **Process Sales**: Create and view sales invoices
5. **View Reports**: Access all sales data and profit information

### Accountant Functions
1. **View Products**: Browse available products and stock levels
2. **Process Sales**: Create sales invoices
3. **View Sales History**: See their own sales records

### Creating a Sale
1. Navigate to Sales menu
2. Select a product from the dropdown
3. Enter quantity
4. Click "Add to Cart"
5. Repeat for multiple products
6. Apply discount if needed
7. Click "Complete Sale"
8. Stock is automatically updated

## Database Schema

### Users Table
- Id (PK)
- Username
- Password (Hashed)
- Role (Admin/Accountant)

### Categories Table
- Id (PK)
- Name

### Products Table
- Id (PK)
- Name
- CategoryId (FK)
- PurchasePrice
- SalePrice
- Quantity

### Sales Table
- Id (PK)
- Date
- TotalAmount
- Discount
- Profit
- AccountantId (FK)

### SaleItems Table
- Id (PK)
- SaleId (FK)
- ProductId (FK)
- Quantity
- UnitPrice
- TotalPrice

## Security Features

- Password hashing using SHA256
- Role-based access control
- UI elements hidden based on user role
- Database-level foreign key constraints

## Future Enhancements

- Barcode scanning support
- Thermal printer integration
- Advanced reporting and analytics
- Backup and restore functionality
- Multi-language support
- Customer management
- Invoice PDF export

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Verify the server name in the connection string
- Check Windows Authentication permissions

### Build Errors
- Restore NuGet packages: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

### Runtime Errors
- Check the database exists and is accessible
- Verify all migrations are applied
- Check the Output window for detailed error messages

## License

This project is provided as-is for educational and commercial use.

## Support

For issues or questions, please check the code comments and documentation within the source files.
