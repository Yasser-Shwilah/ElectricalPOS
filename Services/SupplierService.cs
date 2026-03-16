using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class SupplierService
    {
        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Suppliers
                .Include(s => s.Purchases)
                .ToListAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Suppliers
                .Include(s => s.Purchases)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> AddSupplierAsync(Supplier supplier)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Suppliers.Update(supplier);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var supplier = await context.Suppliers.FindAsync(id);
                if (supplier != null)
                {
                    context.Suppliers.Remove(supplier);
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

        public async Task<bool> UpdateSupplierDebtAsync(int supplierId, decimal amount)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var supplier = await context.Suppliers.FindAsync(supplierId);
                if (supplier != null)
                {
                    supplier.TotalDebt += amount;
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

        public async Task<decimal> GetSupplierBalanceAsync(int supplierId)
        {
            using var context = new ElectricalPOSContext();
            var supplier = await context.Suppliers.FindAsync(supplierId);
            return supplier?.TotalDebt ?? 0;
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
    }
}
