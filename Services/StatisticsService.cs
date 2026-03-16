using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class StatisticsService
    {
        private readonly SaleService _saleService = new();
        private readonly PurchaseService _purchaseService = new();
        private readonly ProductService _productService = new();
        private readonly CategoryService _categoryService = new();
        private readonly CustomerService _customerService = new();
        private readonly SupplierService _supplierService = new();
        private readonly CashTransactionService _cashTransactionService = new();

        // Get dashboard summary KPIs
        public async Task<DashboardSummary> GetDashboardSummaryAsync()
        {
            using var context = new ElectricalPOSContext();
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Today's stats
            var todaySales = await context.Sales
                .Where(s => s.Date.Date == today)
                .ToListAsync();

            var todaySalesTotal = todaySales.Sum(s => s.TotalAmount);
            var todayProfit = todaySales.Sum(s => s.Profit);
            var todaySalesCount = todaySales.Count;

            // This week's stats
            var weekSales = await context.Sales
                .Where(s => s.Date.Date >= startOfWeek)
                .ToListAsync();

            var weekSalesTotal = weekSales.Sum(s => s.TotalAmount);
            var weekProfit = weekSales.Sum(s => s.Profit);

            // This month's stats
            var monthSales = await context.Sales
                .Where(s => s.Date.Date >= startOfMonth)
                .ToListAsync();

            var monthSalesTotal = monthSales.Sum(s => s.TotalAmount);
            var monthProfit = monthSales.Sum(s => s.Profit);

            // Total counts
            var totalProducts = await context.Products.SumAsync(p => p.Quantity);
            var totalCustomers = await context.Customers.CountAsync();
            var totalSuppliers = await context.Suppliers.CountAsync();

            // Low stock products (quantity <= 1)
            var lowStockCount = await context.Products
                .Where(p => p.Quantity <= 1)
                .CountAsync();

            // Out of stock products
            var outOfStockCount = await context.Products
                .Where(p => p.Quantity == 0)
                .CountAsync();

            // Total expenses this month
            var monthExpenses = await GetMonthlyExpensesFromCashTransactionsAsync(startOfMonth);

            // Total debt from suppliers
            var totalSupplierDebt = await context.Suppliers
                .SumAsync(s => s.TotalDebt);

            return new DashboardSummary
            {
                TodaySales = todaySalesTotal,
                TodayProfit = todayProfit,
                TodaySalesCount = todaySalesCount,
                WeekSales = weekSalesTotal,
                WeekProfit = weekProfit,
                MonthSales = monthSalesTotal,
                MonthProfit = monthProfit,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalSuppliers = totalSuppliers,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                MonthExpenses = monthExpenses,
                TotalSupplierDebt = totalSupplierDebt
            };
        }

        // Get sales data for the last 7 days
        public async Task<List<DailySalesData>> GetLast7DaysSalesAsync()
        {
            using var context = new ElectricalPOSContext();
            var result = new List<DailySalesData>();
            var today = DateTime.Today;

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var daySales = await context.Sales
                    .Where(s => s.Date.Date == date)
                    .ToListAsync();

                result.Add(new DailySalesData
                {
                    Date = date,
                    DateString = date.ToString("yyyy-MM-dd"),
                    DayName = GetArabicDayName(date.DayOfWeek),
                    Sales = daySales.Sum(s => s.TotalAmount),
                    Profit = daySales.Sum(s => s.Profit),
                    Count = daySales.Count
                });
            }

            return result;
        }

        // Get sales data for the last 30 days
        public async Task<List<DailySalesData>> GetLast30DaysSalesAsync()
        {
            using var context = new ElectricalPOSContext();
            var result = new List<DailySalesData>();
            var today = DateTime.Today;

            for (int i = 29; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var daySales = await context.Sales
                    .Where(s => s.Date.Date == date)
                    .ToListAsync();

                result.Add(new DailySalesData
                {
                    Date = date,
                    DateString = date.ToString("yyyy-MM-dd"),
                    DayName = date.ToString("dd/MM"),
                    Sales = daySales.Sum(s => s.TotalAmount),
                    Profit = daySales.Sum(s => s.Profit),
                    Count = daySales.Count
                });
            }

            return result;
        }

        // Get monthly sales for the last 12 months
        public async Task<List<MonthlySalesData>> GetLast12MonthsSalesAsync()
        {
            using var context = new ElectricalPOSContext();
            var result = new List<MonthlySalesData>();
            var today = DateTime.Today;

            for (int i = 11; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                var startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var monthSales = await context.Sales
                    .Where(s => s.Date >= startOfMonth && s.Date <= endOfMonth)
                    .ToListAsync();

                result.Add(new MonthlySalesData
                {
                    Year = monthDate.Year,
                    Month = monthDate.Month,
                    MonthName = GetArabicMonthName(monthDate.Month),
                    Sales = monthSales.Sum(s => s.TotalAmount),
                    Profit = monthSales.Sum(s => s.Profit),
                    Count = monthSales.Count
                });
            }

            return result;
        }

        // Get sales by category
        public async Task<List<CategorySalesData>> GetSalesByCategoryAsync()
        {
            using var context = new ElectricalPOSContext();

            var salesWithItems = await context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            var categoryData = new Dictionary<string, decimal>();

            foreach (var sale in salesWithItems)
            {
                foreach (var item in sale.SaleItems)
                {
                    var categoryName = item.Product?.Category?.Name ?? "غير مصنف";
                    if (!categoryData.ContainsKey(categoryName))
                    {
                        categoryData[categoryName] = 0;
                    }
                    categoryData[categoryName] += item.TotalPrice;
                }
            }

            return categoryData.Select(kv => new CategorySalesData
            {
                CategoryName = kv.Key,
                Sales = kv.Value
            }).OrderByDescending(x => x.Sales).ToList();
        }

        // Get profit by category
        public async Task<List<CategorySalesData>> GetProfitByCategoryAsync()
        {
            using var context = new ElectricalPOSContext();

            var salesWithItems = await context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            var categoryData = new Dictionary<string, decimal>();

            foreach (var sale in salesWithItems)
            {
                foreach (var item in sale.SaleItems)
                {
                    var categoryName = item.Product?.Category?.Name ?? "غير مصنف";
                    if (!categoryData.ContainsKey(categoryName))
                    {
                        categoryData[categoryName] = 0;
                    }
                    var profit = (item.Product.SalePrice - item.Product.PurchasePrice) * item.Quantity;
                    categoryData[categoryName] += profit;
                }
            }

            return categoryData.Select(kv => new CategorySalesData
            {
                CategoryName = kv.Key,
                Sales = kv.Value
            }).OrderByDescending(x => x.Sales).ToList();
        }

        // Get top selling products
        public async Task<List<TopProductData>> GetTopSellingProductsAsync(int count = 10)
        {
            using var context = new ElectricalPOSContext();

            var salesWithItems = await context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            var productData = new Dictionary<string, (string Name, int Quantity, decimal Amount)>();

            foreach (var sale in salesWithItems)
            {
                foreach (var item in sale.SaleItems)
                {
                    var productName = item.Product?.Name ?? "غير معروف";
                    if (!productData.ContainsKey(productName))
                    {
                        productData[productName] = (productName, 0, 0);
                    }
                    var existing = productData[productName];
                    productData[productName] = (
                        productName,
                        existing.Item2 + item.Quantity,
                        existing.Item3 + item.TotalPrice
                    );
                }
            }

            return productData
                .Select(kv => new TopProductData
                {
                    ProductName = kv.Value.Name,
                    Quantity = kv.Value.Item2,
                    Amount = kv.Value.Item3
                })
                .OrderByDescending(x => x.Quantity)
                .Take(count)
                .ToList();
        }

        // Get top profitable products
        public async Task<List<TopProductData>> GetTopProfitableProductsAsync(int count = 10)
        {
            using var context = new ElectricalPOSContext();

            var salesWithItems = await context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            var productData = new Dictionary<string, (string Name, decimal Profit)>();

            foreach (var sale in salesWithItems)
            {
                foreach (var item in sale.SaleItems)
                {
                    var productName = item.Product?.Name ?? "غير معروف";
                    if (!productData.ContainsKey(productName))
                    {
                        productData[productName] = (productName, 0);
                    }
                    var existing = productData[productName];
                    var profit = (item.Product.SalePrice - item.Product.PurchasePrice) * item.Quantity;
                    productData[productName] = (
                        productName,
                        existing.Item2 + profit
                    );
                }
            }

            return productData
                .Select(kv => new TopProductData
                {
                    ProductName = kv.Value.Name,
                    Quantity = 0,
                    Amount = kv.Value.Item2
                })
                .OrderByDescending(x => x.Amount)
                .Take(count)
                .ToList();
        }

        // Get inventory status
        public async Task<InventoryStatusData> GetInventoryStatusAsync()
        {
            using var context = new ElectricalPOSContext();

            var inStock = await context.Products
                .Where(p => p.Quantity >= 10)
                .CountAsync();

            var lowStock = await context.Products
                .Where(p => p.Quantity > 0 && p.Quantity < 10)
                .CountAsync();

            var outOfStock = await context.Products
                .Where(p => p.Quantity == 0)
                .CountAsync();

            var totalProducts = await context.Products.CountAsync();
            var totalQuantity = await context.Products.SumAsync(p => p.Quantity);
            var totalValue = await context.Products.SumAsync(p => p.Quantity * p.PurchasePrice);

            return new InventoryStatusData
            {
                InStock = inStock,
                LowStock = lowStock,
                OutOfStock = outOfStock,
                TotalProducts = totalProducts,
                TotalQuantity = totalQuantity,
                TotalValue = totalValue
            };
        }

        // Get sales by payment method - simplified as there's no PaymentMethod field
        public async Task<List<PaymentMethodData>> GetSalesByPaymentMethodAsync()
        {
            using var context = new ElectricalPOSContext();

            // Since there's no PaymentMethod field, we'll just return total sales
            var totalSales = await context.Sales.SumAsync(s => s.TotalAmount);

            return new List<PaymentMethodData>
            {
                new PaymentMethodData { Method = "إجمالي المبيعات", Amount = totalSales }
            };
        }

        // Helper methods
        private string GetArabicDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Sunday => "الأحد",
                DayOfWeek.Monday => "الإثنين",
                DayOfWeek.Tuesday => "الثلاثاء",
                DayOfWeek.Wednesday => "الأربعاء",
                DayOfWeek.Thursday => "الخميس",
                DayOfWeek.Friday => "الجمعة",
                DayOfWeek.Saturday => "السبت",
                _ => ""
            };
        }

        private string GetArabicMonthName(int month)
        {
            return month switch
            {
                1 => "يناير",
                2 => "فبراير",
                3 => "مارس",
                4 => "أبريل",
                5 => "مايو",
                6 => "يونيو",
                7 => "يوليو",
                8 => "أغسطس",
                9 => "سبتمبر",
                10 => "أكتوبر",
                11 => "نوفمبر",
                12 => "ديسمبر",
                _ => ""
            };
        }

        // Helper method to get monthly expenses from cash transactions
        private async Task<decimal> GetMonthlyExpensesFromCashTransactionsAsync(DateTime startOfMonth)
        {
            using var context = new ElectricalPOSContext();
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            return await context.CashTransactions
                .Where(ct => ct.Type == "مصروف" && ct.Date >= startOfMonth && ct.Date <= endOfMonth)
                .SumAsync(ct => ct.Amount);
        }
    }

    // Dashboard Summary DTO
    public class DashboardSummary
    {
        public decimal TodaySales { get; set; }
        public decimal TodayProfit { get; set; }
        public int TodaySalesCount { get; set; }
        public decimal WeekSales { get; set; }
        public decimal WeekProfit { get; set; }
        public decimal MonthSales { get; set; }
        public decimal MonthProfit { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal MonthExpenses { get; set; }
        public decimal TotalSupplierDebt { get; set; }
    }

    // Daily Sales Data DTO
    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public string DateString { get; set; } = "";
        public string DayName { get; set; } = "";
        public decimal Sales { get; set; }
        public decimal Profit { get; set; }
        public int Count { get; set; }
    }

    // Monthly Sales Data DTO
    public class MonthlySalesData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public decimal Sales { get; set; }
        public decimal Profit { get; set; }
        public int Count { get; set; }
    }

    // Category Sales Data DTO
    public class CategorySalesData
    {
        public string CategoryName { get; set; } = "";
        public decimal Sales { get; set; }
    }

    // Top Product Data DTO
    public class TopProductData
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    // Inventory Status Data DTO
    public class InventoryStatusData
    {
        public int InStock { get; set; }
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    // Payment Method Data DTO
    public class PaymentMethodData
    {
        public string Method { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
