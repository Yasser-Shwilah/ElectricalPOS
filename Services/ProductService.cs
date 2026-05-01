using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class ProductService
    {
        public async Task<List<Product>> GetAllProductsAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Products.Add(product);
                await context.SaveChangesAsync();
                
                // Add cash transaction for product purchase expense
                var cashTransactionService = new CashTransactionService();
                await cashTransactionService.AddCashTransactionAsync(new CashTransaction
                {
                    Date = DateTime.Now,
                    Type = "مصروف",
                    Amount = product.PurchasePrice * product.Quantity,
                    Description = $"إضافة منتج جديد: {product.Name}"
                });
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var existing = await context.Products.FindAsync(product.Id);
                if (existing == null)
                    return false;

                // Calculate quantity difference
                int oldQuantity = existing.Quantity;
                int newQuantity = product.Quantity;
                int difference = newQuantity - oldQuantity;

                // Update fields
                existing.Name = product.Name;
                existing.CategoryId = product.CategoryId;
                existing.PurchasePrice = product.PurchasePrice;
                existing.SalePrice = product.SalePrice;
                existing.ModelNumber = product.ModelNumber;
                existing.Quantity = newQuantity;

                // If quantity increased as part of manual edit, create stock movement and record cash expense
                if (difference > 0)
                {
                    // Add stock movement for the adjustment
                    var movement = new StockMovement
                    {
                        ProductId = existing.Id,
                        MovementType = "تسوية",
                        Quantity = difference,
                        Date = DateTime.Now,
                        RelatedDocument = "تعديل كمية منتج"
                    };
                    context.StockMovements.Add(movement);

                    // Add cash transaction as expense (purchase cost of added items)
                    var cashTransaction = new CashTransaction
                    {
                        Date = DateTime.Now,
                        Type = "مصروف",
                        Amount = existing.PurchasePrice * difference,
                        Description = $"تعديل كمية منتج: {existing.Name} - إضافة {difference}"
                    };
                    context.CashTransactions.Add(cashTransaction);
                }
                else if (difference < 0)
                {
                    // Create stock movement for decrease (store positive quantity for consistency)
                    var movement = new StockMovement
                    {
                        ProductId = existing.Id,
                        MovementType = "تسوية",
                        Quantity = Math.Abs(difference),
                        Date = DateTime.Now,
                        RelatedDocument = "تعديل كمية منتج"
                    };
                    context.StockMovements.Add(movement);

                    // Add cash transaction as income for removed items (reverse of expense)
                    var cashTransactionIncome = new CashTransaction
                    {
                        Date = DateTime.Now,
                        Type = "إيراد",
                        Amount = existing.PurchasePrice * Math.Abs(difference),
                        Description = $"تعديل كمية منتج: {existing.Name} - إزالة {Math.Abs(difference)}"
                    };
                    context.CashTransactions.Add(cashTransactionIncome);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var product = await context.Products.FindAsync(id);
                if (product != null)
                {
                    context.Products.Remove(product);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var product = await context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.Quantity -= quantity;
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
