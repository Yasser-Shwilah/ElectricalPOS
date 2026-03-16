using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace ElectricalPOS.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly SaleService _saleService = new();
        private readonly PurchaseService _purchaseService = new();
        private readonly ProductService _productService = new();
        private readonly CashTransactionService _cashService = new();

        [ObservableProperty]
        private DateTime startDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime endDate = DateTime.Today;

        [ObservableProperty]
        private string selectedReportType = "Ø§Ù„Ù…Ø¨ÙŠØ¹Ø§Øª";

        [ObservableProperty]
        private decimal totalSales;

        [ObservableProperty]
        private decimal totalPurchases;

        [ObservableProperty]
        private decimal totalProfit;

        [ObservableProperty]
        private decimal totalExpenses;

        [ObservableProperty]
        private decimal netProfit;

        [ObservableProperty]
        private decimal cashBalance;

        [ObservableProperty]
        private int totalProducts;

        [ObservableProperty]
        private int lowStockCount;

        [ObservableProperty]
        private decimal todaySales;

        [ObservableProperty]
        private decimal todayProfit;

        [ObservableProperty]
        private decimal monthSales;

        [ObservableProperty]
        private decimal monthProfit;

        public ObservableCollection<Sale> SalesList { get; } = new();
        public ObservableCollection<Purchase> PurchasesList { get; } = new();
        public ObservableCollection<Product> LowStockProducts { get; } = new();

        public List<string> ReportTypes { get; } = new()
        {
            "Ø§Ù„Ù…Ø¨ÙŠØ¹Ø§Øª",
            "Ø§Ù„Ù…Ø´ØªØ±ÙŠØ§Øª",
            "Ø§Ù„Ø±Ø¨Ø­ ÙˆØ§Ù„Ø®Ø³Ø§Ø±Ø©",
            "Ø§Ù„Ù…ØµØ±ÙˆÙØ§Øª",
            "Ø§Ù„ØµÙ†Ø¯ÙˆÙ‚",
            "Ø§Ù„Ù…Ø®Ø²ÙˆÙ†"
        };

        public ReportsViewModel()
        {
            LoadReportData();
        }

        [RelayCommand]
        private async Task LoadReportData()
        {
            await LoadSummaryData();
            await LoadDetailedData();
        }

        private async Task LoadSummaryData()
        {
            try
            {
                using var context = new ElectricalPOSContext();

                var salesInRange = await context.Sales
                    .Where(s => s.Date >= StartDate && s.Date <= EndDate.AddDays(1))
                    .ToListAsync();
                TotalSales = salesInRange.Sum(s => s.TotalAmount);
                TotalProfit = salesInRange.Sum(s => s.Profit);

                var purchasesInRange = await context.Purchases
                    .Where(p => p.Date >= StartDate && p.Date <= EndDate.AddDays(1))
                    .ToListAsync();
                TotalPurchases = purchasesInRange.Sum(p => p.TotalAmount);

                // Get expenses from CashTransactions
                var expensesInRange = await context.CashTransactions
                    .Where(ct => ct.Type == "مصروف" && ct.Date >= StartDate && ct.Date <= EndDate.AddDays(1))
                    .ToListAsync();
                TotalExpenses = expensesInRange.Sum(e => e.Amount);

                NetProfit = TotalProfit - TotalExpenses;
                CashBalance = await _cashService.GetCurrentCashBalanceAsync();

                var products = await context.Products.ToListAsync();
                TotalProducts = products.Count;
                LowStockCount = products.Count(p => p.Quantity < 10);

                var today = DateTime.Today;
                TodaySales = await _saleService.GetTotalSalesTodayAsync();
                TodayProfit = await _saleService.GetTotalProfitTodayAsync();

                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthSalesList = await context.Sales
                    .Where(s => s.Date >= monthStart)
                    .ToListAsync();
                MonthSales = monthSalesList.Sum(s => s.TotalAmount);
                MonthProfit = monthSalesList.Sum(s => s.Profit);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading summary data: {ex.Message}");
            }
        }

        private async Task LoadDetailedData()
        {
            try
            {
                SalesList.Clear();
                PurchasesList.Clear();
                LowStockProducts.Clear();

                var sales = await _saleService.GetAllSalesAsync();
                var salesInRange = sales.Where(s => s.Date >= StartDate && s.Date <= EndDate.AddDays(1)).ToList();
                foreach (var sale in salesInRange.Take(100))
                {
                    SalesList.Add(sale);
                }

                var purchases = await _purchaseService.GetAllPurchasesAsync();
                var purchasesInRange = purchases.Where(p => p.Date >= StartDate && p.Date <= EndDate.AddDays(1)).ToList();
                foreach (var purchase in purchasesInRange.Take(100))
                {
                    PurchasesList.Add(purchase);
                }

                var products = await _productService.GetAllProductsAsync();
                foreach (var product in products.Where(p => p.Quantity < 10))
                {
                    LowStockProducts.Add(product);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading detailed data: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task FilterByDateRange()
        {
            await LoadReportData();
        }

        partial void OnSelectedReportTypeChanged(string value)
        {
            _ = LoadReportData();
        }
    }
}
