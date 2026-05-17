using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class StockMovementService
    {
        public async Task<List<StockMovement>> GetAllStockMovementsAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();
        }

        public async Task<List<StockMovement>> GetProductStockMovementsAsync(int productId)
        {
            using var context = new ElectricalPOSContext();
            return await context.StockMovements
                .Where(sm => sm.ProductId == productId)
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();
        }

        public async Task<bool> AddStockMovementAsync(StockMovement movement)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                
                var product = await context.Products.FindAsync(movement.ProductId);
                if (product == null)
                {
                    return false;
                }

                // Update product quantity based on movement type
                if (movement.MovementType == "شراء" || movement.MovementType == "تسوية")
                {
                    product.Quantity += movement.Quantity;
                }
                else if (movement.MovementType == "بيع")
                {
                    product.Quantity -= movement.Quantity;
                }

                context.StockMovements.Add(movement);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<StockMovement>> GetMovementsByTypeAsync(string movementType)
        {
            using var context = new ElectricalPOSContext();
            return await context.StockMovements
                .Where(sm => sm.MovementType == movementType)
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();
        }

        public async Task<List<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = new ElectricalPOSContext();
            return await context.StockMovements
                .Where(sm => sm.Date >= startDate && sm.Date <= endDate)
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            using var context = new ElectricalPOSContext();
            var products = await context.Products.ToListAsync();
            return products.Sum(p => p.Quantity * p.PurchasePrice);
        }

        public async Task<Dictionary<string, int>> GetLowStockProductsAsync(int threshold = 10)
        {
            using var context = new ElectricalPOSContext();
            var products = await context.Products
                .Where(p => p.Quantity <= threshold)
                .ToListAsync();
            // Group by product name to avoid duplicate dictionary keys when multiple
            // product records share the same name. Sum quantities for the grouped
            // items so the returned dictionary contains the total quantity per name.
            return products
                .GroupBy(p => p.Name)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
        }

        public async Task<bool> PerformStockAdjustmentAsync(int productId, int adjustment, string reason)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                
                var product = await context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // Add stock movement record
                var movement = new StockMovement
                {
                    ProductId = productId,
                    MovementType = "تسوية",
                    Quantity = adjustment,
                    Date = DateTime.Now,
                    RelatedDocument = $"Adjustment: {reason}"
                };
                context.StockMovements.Add(movement);

                // Update product quantity
                product.Quantity += adjustment;
                
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task PerformStockTakeAsync(Dictionary<int, int> actualQuantities)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                
                foreach (var kvp in actualQuantities)
                {
                    var product = await context.Products.FindAsync(kvp.Key);
                    if (product != null)
                    {
                        int difference = kvp.Value - product.Quantity;
                        if (difference != 0)
                        {
                            // Add stock movement for adjustment
                            var movement = new StockMovement
                            {
                                ProductId = kvp.Key,
                                MovementType = "تسوية",
                                Quantity = difference,
                                Date = DateTime.Now,
                                RelatedDocument = "Stock Take"
                            };
                            context.StockMovements.Add(movement);
                            
                            // Update quantity
                            product.Quantity = kvp.Value;
                        }
                    }
                }
                
                await context.SaveChangesAsync();
            }
            catch
            {
                // Handle error
            }
        }
    }
}
