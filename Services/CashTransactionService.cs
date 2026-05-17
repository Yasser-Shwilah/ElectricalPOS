using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class CashTransactionService
    {
        public async Task<List<CashTransaction>> GetAllCashTransactionsAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.CashTransactions
                .OrderByDescending(ct => ct.Date)
                .ToListAsync();
        }

        public async Task<List<CashTransaction>> GetCashTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = new ElectricalPOSContext();
            // Use .Date to compare only the date part, ignoring time
            return await context.CashTransactions
                .Where(ct => ct.Date.Date >= startDate.Date && ct.Date.Date <= endDate.Date)
                .OrderByDescending(ct => ct.Date)
                .ToListAsync();
        }

        public async Task<bool> AddCashTransactionAsync(CashTransaction transaction)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.CashTransactions.Add(transaction);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCashTransactionAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var transaction = await context.CashTransactions.FindAsync(id);
                if (transaction != null)
                {
                    context.CashTransactions.Remove(transaction);
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

        public async Task<decimal> GetTodayIncomeAsync(DateTime date)
        {
            using var context = new ElectricalPOSContext();
            var transactions = await context.CashTransactions
                .Where(ct => ct.Date.Date == date.Date && ct.Type == "إيراد")
                .ToListAsync();

            return transactions.Sum(t => t.Amount);
        }

        public async Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var context = new ElectricalPOSContext();
            var query = context.CashTransactions.Where(ct => ct.Type == "إيراد");

            if (startDate.HasValue)
                query = query.Where(ct => ct.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ct => ct.Date <= endDate.Value);

            return await query.SumAsync(ct => ct.Amount);
        }

        public async Task<decimal> GetTotalExpenseAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var context = new ElectricalPOSContext();
            var query = context.CashTransactions.Where(ct => ct.Type == "مصروف");

            if (startDate.HasValue)
                query = query.Where(ct => ct.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ct => ct.Date <= endDate.Value);

            return await query.SumAsync(ct => ct.Amount);
        }

        public async Task<decimal> GetCurrentCashBalanceAsync()
        {
            using var context = new ElectricalPOSContext();
            var allTransactions = await context.CashTransactions.ToListAsync();
            
            decimal income = allTransactions.Where(t => t.Type == "إيراد").Sum(t => t.Amount);
            decimal expense = allTransactions.Where(t => t.Type == "مصروف").Sum(t => t.Amount);

            return income - expense;
        }

        public async Task<decimal> GetTodayExpenseAsync(DateTime date)
        {
            using var context = new ElectricalPOSContext();
            var transactions = await context.CashTransactions
                .Where(ct => ct.Date.Date == date.Date && ct.Type == "مصروف")
                .ToListAsync();

            return transactions.Sum(t => t.Amount);
        }

        public async Task<decimal> GetMonthlyExpenseAsync(DateTime date)
        {
            using var context = new ElectricalPOSContext();
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            // Use half-open interval [startOfMonth, startOfNextMonth) to include all times
            var transactions = await context.CashTransactions
                .Where(ct => ct.Date >= startOfMonth && ct.Date < startOfNextMonth && ct.Type == "مصروف")
                .ToListAsync();

            return transactions.Sum(t => t.Amount);
        }

        public async Task<decimal> GetYearlyIncomeAsync(DateTime date)
        {
            using var context = new ElectricalPOSContext();
            var startOfYear = new DateTime(date.Year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);
            return await context.CashTransactions
                .Where(ct => ct.Date >= startOfYear && ct.Date < startOfNextYear && ct.Type == "إيراد")
                .SumAsync(ct => ct.Amount);
        }

        public async Task<decimal> GetYearlyExpenseAsync(DateTime date)
        {
            using var context = new ElectricalPOSContext();
            var startOfYear = new DateTime(date.Year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);
            return await context.CashTransactions
                .Where(ct => ct.Date >= startOfYear && ct.Date < startOfNextYear && ct.Type == "مصروف")
                .SumAsync(ct => ct.Amount);
        }

        public async Task<Dictionary<string, decimal>> GetCashSummaryAsync()
        {
            var summary = new Dictionary<string, decimal>();
            var today = DateTime.Today;

            summary["TotalIncome"] = await GetYearlyIncomeAsync(today);
            summary["TotalExpense"] = await GetYearlyExpenseAsync(today);
            summary["CurrentBalance"] = await GetCurrentCashBalanceAsync();
            summary["TodayIncome"] = await GetTodayIncomeAsync(today);
            summary["TodayExpense"] = await GetTodayExpenseAsync(today);
            summary["MonthExpense"] = await GetMonthlyExpenseAsync(today);
            
            return summary;
        }
    }
}
