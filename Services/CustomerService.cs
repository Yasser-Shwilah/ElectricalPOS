using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class CustomerService
    {
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Customers
                .Include(c => c.Sales)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Customers
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Customers.Update(customer);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var customer = await context.Customers.FindAsync(id);
                if (customer != null)
                {
                    context.Customers.Remove(customer);
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

        public async Task<bool> UpdateCustomerDebtAsync(int customerId, decimal amount)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var customer = await context.Customers.FindAsync(customerId);
                if (customer != null)
                {
                    customer.TotalDebt += amount;
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

        public async Task<decimal> GetCustomerBalanceAsync(int customerId)
        {
            using var context = new ElectricalPOSContext();
            var customer = await context.Customers.FindAsync(customerId);
            return customer?.TotalDebt ?? 0;
        }

        public async Task<List<Sale>> GetCustomerSalesAsync(int customerId)
        {
            using var context = new ElectricalPOSContext();
            return await context.Sales
                .Where(s => s.CustomerId == customerId)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }
    }
}
