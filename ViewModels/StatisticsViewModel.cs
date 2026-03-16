using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Services;
using Newtonsoft.Json;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly StatisticsService _statisticsService = new();

        [ObservableProperty]
        private DashboardSummary? summary;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private string selectedPeriod = "week";

        // Chart data as JSON strings for WebView2
        [ObservableProperty]
        private string last7DaysSalesJson = "[]";

        [ObservableProperty]
        private string last30DaysSalesJson = "[]";

        [ObservableProperty]
        private string last12MonthsSalesJson = "[]";

        [ObservableProperty]
        private string categorySalesJson = "[]";

        [ObservableProperty]
        private string profitByCategoryJson = "[]";

        [ObservableProperty]
        private string topSellingProductsJson = "[]";

        [ObservableProperty]
        private string topProfitableProductsJson = "[]";

        [ObservableProperty]
        private string inventoryStatusJson = "[]";

        [ObservableProperty]
        private string paymentMethodJson = "[]";

        public StatisticsViewModel()
        {
            LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load all data in parallel
                var summaryTask = _statisticsService.GetDashboardSummaryAsync();
                var last7DaysTask = _statisticsService.GetLast7DaysSalesAsync();
                var last30DaysTask = _statisticsService.GetLast30DaysSalesAsync();
                var last12MonthsTask = _statisticsService.GetLast12MonthsSalesAsync();
                var categorySalesTask = _statisticsService.GetSalesByCategoryAsync();
                var profitByCategoryTask = _statisticsService.GetProfitByCategoryAsync();
                var topSellingTask = _statisticsService.GetTopSellingProductsAsync();
                var topProfitableTask = _statisticsService.GetTopProfitableProductsAsync();
                var inventoryStatusTask = _statisticsService.GetInventoryStatusAsync();
                var paymentMethodTask = _statisticsService.GetSalesByPaymentMethodAsync();

                await Task.WhenAll(
                    summaryTask,
                    last7DaysTask,
                    last30DaysTask,
                    last12MonthsTask,
                    categorySalesTask,
                    profitByCategoryTask,
                    topSellingTask,
                    topProfitableTask,
                    inventoryStatusTask,
                    paymentMethodTask
                );

                Summary = summaryTask.Result;
                Last7DaysSalesJson = ConvertDailySalesToJson(last7DaysTask.Result);
                Last30DaysSalesJson = ConvertDailySalesToJson(last30DaysTask.Result);
                Last12MonthsSalesJson = ConvertMonthlySalesToJson(last12MonthsTask.Result);
                CategorySalesJson = ConvertCategorySalesToJson(categorySalesTask.Result);
                ProfitByCategoryJson = ConvertCategorySalesToJson(profitByCategoryTask.Result);
                TopSellingProductsJson = ConvertTopProductsToJson(topSellingTask.Result);
                TopProfitableProductsJson = ConvertTopProductsToJson(topProfitableTask.Result);
                InventoryStatusJson = ConvertInventoryStatusToJson(inventoryStatusTask.Result);
                PaymentMethodJson = ConvertPaymentMethodToJson(paymentMethodTask.Result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
                MessageBox.Show($"حدث خطأ في تحميل الإحصائيات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ChangePeriod(string period)
        {
            SelectedPeriod = period;
            LoadDataAsync();
        }

        // JSON Conversion helpers
        private string ConvertDailySalesToJson(List<DailySalesData> data)
        {
            var result = data.Select(d => new
            {
                date = d.DateString,
                dayName = d.DayName,
                sales = d.Sales,
                profit = d.Profit,
                count = d.Count
            }).ToList();
            return JsonConvert.SerializeObject(result);
        }

        private string ConvertMonthlySalesToJson(List<MonthlySalesData> data)
        {
            var result = data.Select(d => new
            {
                year = d.Year,
                month = d.Month,
                monthName = d.MonthName,
                sales = d.Sales,
                profit = d.Profit,
                count = d.Count
            }).ToList();
            return JsonConvert.SerializeObject(result);
        }

        private string ConvertCategorySalesToJson(List<CategorySalesData> data)
        {
            var result = data.Select(d => new
            {
                name = d.CategoryName,
                value = d.Sales
            }).ToList();
            return JsonConvert.SerializeObject(result);
        }

        private string ConvertTopProductsToJson(List<TopProductData> data)
        {
            var result = data.Select(d => new
            {
                name = d.ProductName,
                quantity = d.Quantity,
                amount = d.Amount
            }).ToList();
            return JsonConvert.SerializeObject(result);
        }

        private string ConvertInventoryStatusToJson(InventoryStatusData data)
        {
            var result = new[]
            {
                new { name = "متوفر", value = data.InStock },
                new { name = "مخزون منخفض", value = data.LowStock },
                new { name = "نفد المخزون", value = data.OutOfStock }
            };
            return JsonConvert.SerializeObject(result);
        }

        private string ConvertPaymentMethodToJson(List<PaymentMethodData> data)
        {
            var result = data.Select(d => new
            {
                name = d.Method,
                value = d.Amount
            }).ToList();
            return JsonConvert.SerializeObject(result);
        }
    }
}
