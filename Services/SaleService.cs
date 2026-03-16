using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class SaleService
    {
        private readonly ProductService _productService = new();

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

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Include(s => s.Accountant)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> CreateSaleAsync(Sale sale, List<SaleItem> saleItems)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                
                // Set command timeout to 60 seconds for complex operations
                ElectricalPOSContext.SetCommandTimeout(context, 60);

                // Calculate totals
                decimal totalAmount = 0;
                decimal totalProfit = 0;

                foreach (var item in saleItems)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        return false; // Product not found or insufficient stock
                    }

                    item.UnitPrice = product.SalePrice;
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    totalAmount += item.TotalPrice;
                    totalProfit += (product.SalePrice - product.PurchasePrice) * item.Quantity;

                    // Update product stock
                    product.Quantity -= item.Quantity;
                }

                sale.TotalAmount = totalAmount - sale.Discount;
                sale.Profit = totalProfit;
                sale.Date = DateTime.Now;

                // Set the Sale navigation property for each SaleItem so EF Core can handle the FK
                foreach (var item in saleItems)
                {
                    item.Sale = sale;
                }

                context.Sales.Add(sale);
                context.SaleItems.AddRange(saleItems);
                
                await context.SaveChangesAsync();
                
                // Add cash transaction for sale income
                var cashTransactionService = new CashTransactionService();
                await cashTransactionService.AddCashTransactionAsync(new CashTransaction
                {
                    Date = sale.Date,
                    Type = "إيراد",
                    Amount = sale.TotalAmount,
                    Description = $"بيع فاتورة رقم {sale.Id}"
                });
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sale creation error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Sale>> GetSalesByAccountantAsync(int accountantId)
        {
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Where(s => s.AccountantId == accountantId)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSalesTodayAsync()
        {
            using var context = new ElectricalPOSContext();
            var today = DateTime.Today;
            return await context.Sales
                .Where(s => s.Date.Date == today)
                .SumAsync(s => s.TotalAmount);
        }

        public async Task<decimal> GetTotalProfitTodayAsync()
        {
            using var context = new ElectricalPOSContext();
            var today = DateTime.Today;
            var result = await context.Sales
                .Where(s => s.Date.Date == today)
                .SumAsync(s => s.Profit);
            return result;
        }

        public async Task<int> GetSalesCountTodayAsync()
        {
            using var context = new ElectricalPOSContext();
            var today = DateTime.Today;
            return await context.Sales
                .Where(s => s.Date.Date == today)
                .CountAsync();
        }
    }
}
