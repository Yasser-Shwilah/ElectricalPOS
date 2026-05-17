using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ElectricalPOS.Models
{
    public class ElectricalPOSContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<CashTransaction> CashTransactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Backup> Backups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // =====================================================================
                // ROOT CAUSE EXPLANATION:
                // =====================================================================
                // The EnableRetryOnFailure() was causing RetryLimitExceededException
                // because:
                //
                // 1. When EnableRetryOnFailure() is configured, EF Core wraps ALL
                //    database operations with SqlServerRetryingExecutionStrategy
                //
                // 2. This execution strategy intercepts exceptions and retries
                //    up to 5 times for transient failures (deadlocks, timeouts, etc.)
                //
                // 3. However, when EF Core internally manages transactions or when
                //    manual transactions are used with BeginTransactionAsync(), the
                //    retry strategy cannot properly handle the connection state
                //
                // 4. After 5 failed retries, it throws RetryLimitExceededException
                //    wrapping the original SQL exception, making debugging difficult
                //
                // FOR A LOCAL POS SYSTEM:
                // - Local SQL Server is typically stable with low transient failures
                // - Retry logic adds unnecessary overhead and complexity
                // - The retry strategy is designed for cloud/Azure scenarios
                // =====================================================================

                // OPTION 1: Remove retry strategy completely (RECOMMENDED for local POS)
                // IMPORTANT: Update 'YOUR_SERVER' below to match your SQL Server instance
                // Options: localhost, (localdb)\MSSQLLocalDB, .\SQLEXPRESS, or your computer name
                optionsBuilder.UseSqlServer(
                    @"Server=localhost;Database=ElectricalPOS_DB;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;",
                    sqlOptions =>
                    {
                        // No EnableRetryOnFailure() - this is correct for local POS
                        sqlOptions.CommandTimeout(60); // 60 second timeout for long operations
                    });
                
                // Note: For production logging, use DatabaseLogger class in ElectricalPOS/Utilities/

                // =====================================================================
                // OPTION 2: If you really need retry logic, use this instead:
                // =====================================================================
                // optionsBuilder.UseSqlServer(
                //     @"Server=(local);Database=ElectricalPOS_DB;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;",
                //     sqlOptions =>
                //     {
                //         sqlOptions.EnableRetryOnFailure(
                //             maxRetryCount: 3, // Reduced from 5
                //             maxRetryDelay: TimeSpan.FromSeconds(5), // Faster failures
                //             errorNumbersToAdd: new[] { 1205, 1222 }); // Only deadlock errors
                //     });
                //
                // IMPORTANT: If using manual transactions, you must use
                // IExecutionStrategy with proper context configuration.
                // See: ElectricalPOS/Utilities/DbContextFactory.cs for guidance.
            }
        }

        public static void SetCommandTimeout(DbContext context, int timeoutSeconds = 60)
        {
            context.Database.SetCommandTimeout(TimeSpan.FromSeconds(timeoutSeconds));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure existing relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Accountant)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.AccountantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure new relationships
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Invoice relationships
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.InvoiceItems)
                .WithOne(ii => ii.Invoice)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
