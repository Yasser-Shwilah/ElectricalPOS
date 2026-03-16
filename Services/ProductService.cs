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
                context.Products.Update(product);
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
