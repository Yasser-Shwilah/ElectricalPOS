using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class PurchaseService
    {
        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> CreatePurchaseAsync(Purchase purchase, List<PurchaseItem> items)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                
                // Calculate totals
                purchase.TotalAmount = items.Sum(i => i.TotalPrice);
                purchase.PaidAmount = 0;
                purchase.Debt = purchase.TotalAmount;
                purchase.IsPaid = false;

                // Add purchase
                context.Purchases.Add(purchase);
                await context.SaveChangesAsync();

                // Add purchase items
                foreach (var item in items)
                {
                    item.PurchaseId = purchase.Id;
                    context.PurchaseItems.Add(item);
                }
                await context.SaveChangesAsync();

                // Update supplier debt
                var supplier = await context.Suppliers.FindAsync(purchase.SupplierId);
                if (supplier != null)
                {
                    supplier.TotalDebt += purchase.Debt;
                    await context.SaveChangesAsync();
                }

                // Update stock for each product
                foreach (var item in items)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                        product.PurchasePrice = item.UnitPrice; // Update purchase price
                    }
                }
                await context.SaveChangesAsync();

                // Add stock movement records
                foreach (var item in items)
                {
                    var stockMovement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        MovementType = "شراء",
                        Quantity = item.Quantity,
                        Date = purchase.Date,
                        RelatedDocument = $"PO-{purchase.Id}"
                    };
                    context.StockMovements.Add(stockMovement);
                }
                
                // Add cash transaction for purchase expense
                var cashTransactionService = new CashTransactionService();
                await cashTransactionService.AddCashTransactionAsync(new CashTransaction
                {
                    Date = purchase.Date,
                    Type = "مصروف",
                    Amount = purchase.TotalAmount,
                    Description = $"شراء فاتورة رقم {purchase.Id}"
                });
                
                await context.SaveChangesAsync();

                return purchase.Id;
            }
            catch
            {
                return -1;
            }
        }

        public async Task<bool> UpdatePurchaseAsync(Purchase purchase)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Purchases.Update(purchase);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePurchaseAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var purchase = await context.Purchases
                    .Include(p => p.PurchaseItems)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchase != null)
                {
                    // Reverse supplier debt
                    var supplier = await context.Suppliers.FindAsync(purchase.SupplierId);
                    if (supplier != null)
                    {
                        supplier.TotalDebt -= purchase.Debt;
                    }

                    // Reverse stock
                    foreach (var item in purchase.PurchaseItems)
                    {
                        var product = await context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Quantity -= item.Quantity;
                        }
                    }

                    context.Purchases.Remove(purchase);
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

        public async Task<bool> PayPurchaseAsync(int purchaseId, decimal amount)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var purchase = await context.Purchases.FindAsync(purchaseId);
                
                if (purchase != null)
                {
                    purchase.PaidAmount += amount;
                    purchase.Debt = purchase.TotalAmount - purchase.PaidAmount;
                    purchase.IsPaid = purchase.Debt <= 0;

                    // Update supplier debt
                    var supplier = await context.Suppliers.FindAsync(purchase.SupplierId);
                    if (supplier != null)
                    {
                        supplier.TotalDebt -= amount;
                    }

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

        public async Task<List<Purchase>> GetSupplierPurchasesAsync(int supplierId)
        {
            using var context = new ElectricalPOSContext();
            return await context.Purchases
                .Where(p => p.SupplierId == supplierId)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPurchasesAmountAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Purchases.SumAsync(p => p.TotalAmount);
        }

        public async Task<decimal> GetTotalPurchasesDebtAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Purchases.SumAsync(p => p.Debt);
        }
    }
}
