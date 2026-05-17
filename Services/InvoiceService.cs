using ElectricalPOS.Models;
using ElectricalPOS.Services;
using ElectricalPOS.Utilities;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPOS.Services
{
    public class InvoiceService
    {
        private readonly ProductService _productService = new();

        /// <summary>
        /// Generates a unique invoice number (simple sequential number)
        /// </summary>
        public async Task<string> GenerateInvoiceNumberAsync()
        {
            using var context = new ElectricalPOSContext();
            
            // Get the last invoice number from Invoices table
            var lastInvoiceNum = 0;
            var lastInvoice = await context.Invoices
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            if (lastInvoice != null && int.TryParse(lastInvoice.InvoiceNumber, out int invNum))
            {
                lastInvoiceNum = invNum;
            }

            // Also check Sales table for the last invoice number
            var lastSaleNum = 0;
            var lastSale = await context.Sales
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();
            
            if (lastSale != null && int.TryParse(lastSale.InvoiceNumber, out int saleNum))
            {
                lastSaleNum = saleNum;
            }

            // Use the maximum sequence from both tables
            int sequence = Math.Max(lastInvoiceNum, lastSaleNum) + 1;

            return sequence.ToString("D4");
        }

        /// <summary>
        /// Creates a new Invoice with proper transactional handling.
        /// 
        /// IMPORTANT: This method handles its own transaction WITHOUT relying on
        /// EnableRetryOnFailure(), which was causing RetryLimitExceededException.
        /// 
        /// The fix:
        /// 1. Removed EnableRetryOnFailure() from DbContext (recommended for local POS)
        /// 2. Uses explicit transaction for data integrity
        /// 3. Properly rolls back on failure
        /// 4. Logs all database operations for debugging
        /// </summary>
        public async Task<(bool Success, string Message, Invoice? Invoice)> SaveInvoiceAsync(
            List<InvoiceItem> invoiceItems,
            string customerName,
            string? customerPhone)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                using var context = new ElectricalPOSContext();

                // Set command timeout
                ElectricalPOSContext.SetCommandTimeout(context, 30);

                // Generate unique invoice number
                var invoiceNumber = await GenerateInvoiceNumberAsync();
                var invoiceDate = DateTime.Now;

                DatabaseLogger.LogInfo($"Starting invoice creation: {invoiceNumber}");

                // Validate stock and calculate totals
                decimal totalAmount = 0;
                var validatedItems = new List<(InvoiceItem Item, Product Product)>();

                // First pass: validate all items
                foreach (var item in invoiceItems)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        DatabaseLogger.LogWarning($"Product not found: ID={item.ProductId}");
                        return (false, $"المنتج غير موجود", null);
                    }

                    // Prevent selling if stock is insufficient
                    if (product.Quantity < item.Quantity)
                    {
                        DatabaseLogger.LogWarning($"Insufficient stock for product: {product.Name} (Requested: {item.Quantity}, Available: {product.Quantity})");
                        return (false, $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {product.Quantity}", null);
                    }

                    // Calculate subtotal on server side only (Quantity × Unit Price)
                    item.Subtotal = item.Quantity * item.UnitPrice;
                    
                    // Save product name snapshot at time of sale
                    item.ProductName = product.Name;
                    
                    totalAmount += item.Subtotal;
                    validatedItems.Add((item, product));
                }

                // Begin explicit transaction for data integrity
                // This ensures all operations succeed or all fail
                using var dbTransaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // Second pass: update stock and create stock movements
                    foreach (var (item, product) in validatedItems)
                    {
                        // Deduct quantity from products table
                        product.Quantity -= item.Quantity;

                        // Add stock movement record
                        var movement = new StockMovement
                        {
                            ProductId = product.Id,
                            MovementType = "بيع",
                            Quantity = -item.Quantity,
                            Date = DateTime.Now,
                            RelatedDocument = invoiceNumber
                        };
                        context.StockMovements.Add(movement);
                    }

                    // Create invoice with required fields
                    var invoice = new Invoice
                    {
                        InvoiceNumber = invoiceNumber,
                        InvoiceDate = invoiceDate,
                        CustomerName = customerName,
                        CustomerPhone = customerPhone,
                        Total = totalAmount,
                        PreviousBalance = 0.00m,
                        CurrentBalance = totalAmount,
                        CreatedAt = DateTime.Now
                    };

                    // Link items to invoice
                    foreach (var (item, _) in validatedItems)
                    {
                        item.Invoice = invoice;
                    }

                    // Save invoice and items
                    context.Invoices.Add(invoice);
                    context.InvoiceItems.AddRange(validatedItems.Select(x => x.Item));

                    // Commit the transaction
                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    DatabaseLogger.LogDatabaseOperation("Invoice creation", stopwatch.Elapsed, true);

                    // Reload the invoice with related data
                    var savedInvoice = await context.Invoices
                        .Include(i => i.InvoiceItems)
                            .ThenInclude(ii => ii.Product)
                        .FirstOrDefaultAsync(i => i.Id == invoice.Id);

                    DatabaseLogger.LogInfo($"Invoice created successfully: {invoiceNumber}");
                    
                    return (true, "تم حفظ الفاتورة بنجاح", savedInvoice);
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    await dbTransaction.RollbackAsync();
                    
                    DatabaseLogger.LogError("Transaction failed, rolled back", ex);
                    throw; // Re-throw to be caught by outer catch
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                DatabaseLogger.LogError("Invoice creation failed", ex);
                
                // Extract underlying exception if it's RetryLimitExceededException
                var underlyingEx = DatabaseLogger.GetUnderlyingException(ex);
                
                // Log the real error for debugging
                DatabaseLogger.LogError("Underlying exception details:", underlyingEx);
                
                System.Diagnostics.Debug.WriteLine($"Invoice creation error: {ex.Message}");
                return (false, $"خطأ في حفظ الفاتورة: {underlyingEx?.Message ?? ex.Message}", null);
            }
        }

        /// <summary>
        /// Creates a new sale with proper transactional handling.
        /// Uses explicit transaction for data integrity.
        /// </summary>
        public async Task<(bool Success, string Message, Sale? Sale)> CreateInvoiceAsync(
            Sale sale, 
            List<SaleItem> saleItems,
            string customerName = null!,
            string customerPhone = null!)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                using var context = new ElectricalPOSContext();

                // Set command timeout
                ElectricalPOSContext.SetCommandTimeout(context, 30);

                // Generate invoice number
                sale.InvoiceNumber = await GenerateInvoiceNumberAsync();
                sale.Date = DateTime.Now;
                sale.CustomerName = customerName;
                sale.CustomerPhone = customerPhone;

                DatabaseLogger.LogInfo($"Starting sale creation: {sale.InvoiceNumber}");

                // Get all product IDs to fetch in one query
                var productIds = saleItems.Select(si => si.ProductId).ToList();
                
                // Fetch all products in one query using AsNoTracking for performance
                var products = await context.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                // Calculate totals and validate stock
                decimal totalAmount = 0;
                decimal totalProfit = 0;
                var processedItems = new List<(SaleItem Item, Product Product)>();

                foreach (var item in saleItems)
                {
                    // Get product from dictionary (already fetched)
                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                        DatabaseLogger.LogWarning($"Product not found: ID={item.ProductId}");
                        return (false, $"المنتج غير موجود", null);
                    }

                    if (product.Quantity < item.Quantity)
                    {
                        DatabaseLogger.LogWarning($"Insufficient stock for product: {product.Name}");
                        return (false, $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {product.Quantity}", null);
                    }

                    // Calculate item total using the provided unit price (allows editing)
                    item.UnitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.SalePrice;
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    
                    totalAmount += item.TotalPrice;
                    totalProfit += (item.UnitPrice - product.PurchasePrice) * item.Quantity;

                    processedItems.Add((item, product));
                }

                // Begin explicit transaction
                using var dbTransaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // Now update stock - use optimistic locking with Entity Framework
                    foreach (var (item, product) in processedItems)
                    {
                        var productToUpdate = await context.Products.FindAsync(item.ProductId);
                        if (productToUpdate == null || productToUpdate.Quantity < item.Quantity)
                        {
                            DatabaseLogger.LogWarning($"Failed to deduct stock for product: {product.Name}");
                            return (false, $"فشل في خصم المخزون للمنتج", null);
                        }

                        productToUpdate.Quantity -= item.Quantity;

                        // Add stock movement record
                        var movement = new StockMovement
                        {
                            ProductId = item.ProductId,
                            MovementType = "بيع",
                            Quantity = -item.Quantity,
                            Date = DateTime.Now,
                            RelatedDocument = sale.InvoiceNumber
                        };
                        context.StockMovements.Add(movement);
                    }

                    // Apply discount
                    sale.TotalAmount = totalAmount - sale.Discount;
                    
                    // Apply discount to profit
                    // The discount reduces the actual revenue received, so it should reduce profit by the same amount
                    // Ensure profit doesn't go negative (in case discount > profit)
                    sale.Profit = Math.Max(0, totalProfit - sale.Discount);
                    
                    // No previous balance - set both to invoice value
                    sale.PreviousBalance = 0;
                    sale.CurrentBalance = sale.TotalAmount;

                    // Link items to sale
                    foreach (var (item, _) in processedItems)
                    {
                        item.Sale = sale;
                    }

                    context.Sales.Add(sale);
                    context.SaleItems.AddRange(processedItems.Select(x => x.Item));

                    // Commit transaction
                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    // Add cash transaction for sale income
                    var cashTransactionService = new CashTransactionService();
                    await cashTransactionService.AddCashTransactionAsync(new CashTransaction
                    {
                        Date = sale.Date,
                        Type = "إيراد",
                        Amount = sale.TotalAmount,
                        Description = $"بيع فاتورة رقم {sale.InvoiceNumber}"
                    });

                    DatabaseLogger.LogDatabaseOperation("Sale creation", stopwatch.Elapsed, true);

                    // Reload the sale with related data
                    var savedSale = await context.Sales
                        .Include(s => s.SaleItems)
                            .ThenInclude(si => si.Product)
                        .Include(s => s.Accountant)
                        .FirstOrDefaultAsync(s => s.Id == sale.Id);

                    DatabaseLogger.LogInfo($"Sale created successfully: {sale.InvoiceNumber}");

                    // Notify any listeners (e.g. statistics viewmodels) to refresh
                    try
                    {
                        WeakReferenceMessenger.Default.Send(new ViewModels.ReloadStatisticsMessage());
                    }
                    catch { }

                    return (true, "تم حفظ الفاتورة بنجاح", savedSale);
                }
                catch (Exception ex)
                {
                    // Rollback on error
                    await dbTransaction.RollbackAsync();
                    DatabaseLogger.LogError("Transaction failed, rolled back", ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                DatabaseLogger.LogError("Sale creation failed", ex);
                
                var underlyingEx = DatabaseLogger.GetUnderlyingException(ex);
                DatabaseLogger.LogError("Underlying exception details:", underlyingEx);
                
                System.Diagnostics.Debug.WriteLine($"Invoice creation error: {ex.Message}");
                return (false, $"خطأ في حفظ الفاتورة: {underlyingEx?.Message ?? ex.Message}", null);
            }
        }

        /// <summary>
        /// Gets all sales with related data
        /// </summary>
        public async Task<List<Sale>> GetAllSalesAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Include(s => s.Accountant)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a sale by ID with all related data
        /// </summary>
        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Include(s => s.Accountant)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Gets a sale by invoice number
        /// </summary>
        public async Task<Sale?> GetSaleByInvoiceNumberAsync(string invoiceNumber)
        {
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Include(s => s.Accountant)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.InvoiceNumber == invoiceNumber);
        }

        /// <summary>
        /// Gets all invoices with related data
        /// </summary>
        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Invoices
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Product)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets an invoice by ID
        /// </summary>
        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Invoices
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>
        /// Gets an invoice by invoice number
        /// </summary>
        public async Task<Invoice?> GetInvoiceByInvoiceNumberAsync(string invoiceNumber)
        {
            using var context = new ElectricalPOSContext();
            return await context.Invoices
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        /// <summary>
<<<<<<< HEAD
        /// Fully reverses a POS sale (<see cref="Sale"/>): restores inventory, records stock movements,
        /// removes the matching cash-register income entry, and deletes the sale record so statistics
        /// (sales, profit, cash balance, inventory value) stay consistent.
        /// </summary>
        public async Task<(bool Success, string Message)> ReverseSaleAsync(int saleId)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var context = new ElectricalPOSContext();
                ElectricalPOSContext.SetCommandTimeout(context, 30);

                using var dbTransaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var sale = await context.Sales
                        .Include(s => s.SaleItems)
                        .FirstOrDefaultAsync(s => s.Id == saleId);

                    if (sale == null)
                    {
                        await dbTransaction.RollbackAsync();
                        return (false, "الفاتورة غير موجودة.");
                    }

                    foreach (var item in sale.SaleItems)
                    {
                        var product = await context.Products.FindAsync(item.ProductId);
                        if (product == null)
                        {
                            await dbTransaction.RollbackAsync();
                            return (false, $"تعذر استرجاع المخزون: المنتج غير موجود (معرف {item.ProductId}).");
                        }

                        product.Quantity += item.Quantity;

                        context.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            MovementType = "استرجاع بيع",
                            Quantity = item.Quantity,
                            Date = DateTime.Now,
                            RelatedDocument = sale.InvoiceNumber
                        });
                    }

                    var descByInvoiceNumber = $"بيع فاتورة رقم {sale.InvoiceNumber}";
                    var descBySaleId = $"بيع فاتورة رقم {sale.Id}";

                    var cashMatch = await context.CashTransactions
                        .Where(ct => ct.Type == "إيراد"
                                     && ct.Amount == sale.TotalAmount
                                     && ct.Description == descByInvoiceNumber)
                        .OrderByDescending(ct => ct.Date)
                        .FirstOrDefaultAsync();

                    if (cashMatch == null)
                    {
                        cashMatch = await context.CashTransactions
                            .Where(ct => ct.Type == "إيراد"
                                         && ct.Amount == sale.TotalAmount
                                         && ct.Description == descBySaleId)
                            .OrderByDescending(ct => ct.Date)
                            .FirstOrDefaultAsync();
                    }

                    if (cashMatch == null && !string.IsNullOrWhiteSpace(sale.InvoiceNumber))
                    {
                        cashMatch = await context.CashTransactions
                            .Where(ct => ct.Type == "إيراد"
                                         && ct.Amount == sale.TotalAmount
                                         && ct.Description.Contains(sale.InvoiceNumber))
                            .OrderByDescending(ct => ct.Date)
                            .FirstOrDefaultAsync();
                    }

                    if (cashMatch == null)
                    {
                        await dbTransaction.RollbackAsync();
                        return (false,
                            "لم يتم العثور على حركة إيراد في الصندوق مطابقة لهذه الفاتورة. لم يُجرَ أي تغيير لتفادي عدم توازن الصندوق.");
                    }

                    context.CashTransactions.Remove(cashMatch);
                    context.Sales.Remove(sale);

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    DatabaseLogger.LogDatabaseOperation("Sale reversal", stopwatch.Elapsed, true);
                    DatabaseLogger.LogInfo($"Sale reversed: invoice {sale.InvoiceNumber}, sale id {saleId}");

                    try
                    {
                        WeakReferenceMessenger.Default.Send(new ViewModels.ReloadStatisticsMessage());
                    }
                    catch { }

                    return (true,
                        "تم استرجاع الفاتورة بالكامل: رُدّ المخزون، وعُدّل الصندوق، وستُحدَّث الإحصائيات (المبيعات والربح ورصيد النقد) تلقائياً.");
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    DatabaseLogger.LogError("Sale reversal transaction failed", ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                DatabaseLogger.LogError("Sale reversal failed", ex);
                var underlying = DatabaseLogger.GetUnderlyingException(ex);
                return (false, $"فشل استرجاع الفاتورة: {underlying?.Message ?? ex.Message}");
            }
        }

        /// <summary>
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        /// Deletes an invoice and restores stock
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteInvoiceAsync(int invoiceId)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                using var context = new ElectricalPOSContext();

                var invoice = await context.Invoices
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    return (false, "الفاتورة غير موجودة");
                }

                // Begin transaction
                using var dbTransaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // Restore stock for each item
                    foreach (var item in invoice.InvoiceItems)
                    {
                        var product = await context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Quantity += item.Quantity;

                            // Add reverse stock movement
                            var movement = new StockMovement
                            {
                                ProductId = product.Id,
                                MovementType = "إلغاء بيع",
                                Quantity = item.Quantity,
                                Date = DateTime.Now,
                                RelatedDocument = invoice.InvoiceNumber
                            };
                            context.StockMovements.Add(movement);
                        }
                    }

                    // Remove invoice items first (due to cascade)
                    context.InvoiceItems.RemoveRange(invoice.InvoiceItems);
                    
                    // Remove invoice
                    context.Invoices.Remove(invoice);

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    DatabaseLogger.LogDatabaseOperation("Invoice deletion", stopwatch.Elapsed, true);
                    DatabaseLogger.LogInfo($"Invoice deleted: {invoice.InvoiceNumber}");

                    return (true, "تم حذف الفاتورة بنجاح");
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    DatabaseLogger.LogError("Invoice deletion transaction failed", ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                DatabaseLogger.LogError("Invoice deletion failed", ex);
                return (false, $"خطأ في حذف الفاتورة: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches invoices by customer name or invoice number
        /// </summary>
        public async Task<List<Invoice>> SearchInvoicesAsync(string searchTerm)
        {
            using var context = new ElectricalPOSContext();
            var term = searchTerm.ToLower();
            
            return await context.Invoices
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Product)
                .Where(i => i.CustomerName.ToLower().Contains(term) || 
                           i.InvoiceNumber.ToLower().Contains(term))
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets invoices by date range
        /// </summary>
        public async Task<List<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = new ElectricalPOSContext();
            return await context.Invoices
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Product)
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets total sales amount for a date range
        /// </summary>
        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var context = new ElectricalPOSContext();
            var query = context.Invoices.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= endDate.Value);

            return await query.SumAsync(i => i.Total);
        }

        /// <summary>
        /// Gets total sales amount for today
        /// </summary>
        public async Task<decimal> GetTotalSalesTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await GetTotalSalesAsync(today, tomorrow.AddSeconds(-1));
        }

        /// <summary>
        /// Gets total profit for today (from Sales table)
        /// </summary>
        public async Task<decimal> GetTotalProfitTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            using var context = new ElectricalPOSContext();
            var result = await context.Sales
                .Where(s => s.Date >= today && s.Date < tomorrow)
                .SumAsync(s => s.Profit);
            return result;
        }

        /// <summary>
        /// Gets sales count for today
        /// </summary>
        public async Task<int> GetSalesCountTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Where(s => s.Date >= today && s.Date < tomorrow)
                .CountAsync();
        }
    }
}
