using ElectricalPOS.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ElectricalPOS.Views
{
    public partial class StatisticsView : UserControl
    {
        private StatisticsViewModel? _viewModel;
        private bool _isWebViewInitialized = false;
        private static CoreWebView2Environment? _sharedEnvironment = null;
        private static readonly object _lock = new object();

        public StatisticsView()
        {
            InitializeComponent();
            
            _viewModel = new StatisticsViewModel();
            DataContext = _viewModel;
            
            // Subscribe to data changes
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Initialize WebView2
            InitializeWebViewAsync();
        }

        private async void InitializeWebViewAsync()
        {
            try
            {
                // Check if WebView2 is already initialized
                if (StatisticsWebView.CoreWebView2 != null)
                {
                    // Already initialized, just navigate
                    var dashboardPath = GetHtmlFilePath();
                    if (File.Exists(dashboardPath))
                    {
                        StatisticsWebView.CoreWebView2.Navigate(new Uri(dashboardPath).AbsoluteUri);
                    }
                    else
                    {
                        LoadEmbeddedHtml();
                    }
                    _isWebViewInitialized = true;
                    return;
                }

                // Use a shared environment to avoid conflicts
                lock (_lock)
                {
                    if (_sharedEnvironment == null)
                    {
                        // Create environment synchronously and store it
                        _sharedEnvironment = CoreWebView2Environment.CreateAsync().GetAwaiter().GetResult();
                    }
                }

                await StatisticsWebView.EnsureCoreWebView2Async(_sharedEnvironment);

                // Configure WebView2
                StatisticsWebView.CoreWebView2.Settings.IsScriptEnabled = true;
                StatisticsWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                StatisticsWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                StatisticsWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                // Handle messages from JavaScript
                StatisticsWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // Get the HTML file path
                var htmlFilePath = GetHtmlFilePath();

                if (File.Exists(htmlFilePath))
                {
                    StatisticsWebView.CoreWebView2.Navigate(new Uri(htmlFilePath).AbsoluteUri);
                }
                else
                {
                    // Load embedded HTML if file doesn't exist
                    LoadEmbeddedHtml();
                }

                _isWebViewInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 initialization error: {ex.Message}");
                // Show error in the WebView itself
                ShowErrorMessage($"خطأ في تهيئة WebView2: {ex.Message}");
            }
        }

        private void ShowErrorMessage(string message)
        {
            try
            {
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, sans-serif; padding: 40px; background: #f5f7fa; text-align: center; }}
        .error {{ background: #fee; border: 1px solid #fcc; padding: 20px; border-radius: 8px; color: #c00; }}
    </style>
</head>
<body>
    <div class='error'>
        <h2>خطأ</h2>
        <p>{message}</p>
    </div>
</body>
</html>";
                StatisticsWebView.NavigateToString(html);
            }
            catch { }
        }

        private string GetHtmlFilePath()
        {
            // Try different paths
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Check in Web subfolder (for published app)
            var webPath = Path.Combine(baseDir, "Web", "StatisticsDashboard.html");
            if (File.Exists(webPath))
                return webPath;

            // Check in output directory
            var outputPath = Path.Combine(baseDir, "StatisticsDashboard.html");
            if (File.Exists(outputPath))
                return outputPath;

            // Return default path
            return webPath;
        }

        private void LoadEmbeddedHtml()
        {
            // Load a basic HTML if file not found
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Statistics Dashboard</title>
    <script src='https://cdn.jsdelivr.net/npm/echarts@5.4.3/dist/echarts.min.js'></script>
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; padding: 20px; background: #f5f7fa; }
        h1 { color: #1e293b; text-align: center; }
    </style>
</head>
<body>
    <h1>جاري تحميل لوحة الإحصائيات...</h1>
</body>
</html>";
            StatisticsWebView.NavigateToString(html);
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.TryGetWebMessageAsString();
                System.Diagnostics.Debug.WriteLine($"WebView message: {message}");

                if (message == "DashboardReady" && _viewModel != null && _isWebViewInitialized)
                {
                    // Send data to the dashboard
                    SendDataToDashboard();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing web message: {ex.Message}");
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Send data to WebView when ViewModel properties change
            if (_isWebViewInitialized && e.PropertyName?.EndsWith("Json") == true)
            {
                SendDataToDashboard();
            }
        }

        private async void SendDataToDashboard()
        {
            try
            {
                if (_viewModel == null || !_isWebViewInitialized) return;
                if (StatisticsWebView.CoreWebView2 == null) return;

                // Prepare dashboard data
                var dashboardData = new
                {
                    summary = new
                    {
                        TodaySales = _viewModel.Summary?.TodaySales ?? 0,
                        TodayProfit = _viewModel.Summary?.TodayProfit ?? 0,
                        TodaySalesCount = _viewModel.Summary?.TodaySalesCount ?? 0,
                        WeekSales = _viewModel.Summary?.WeekSales ?? 0,
                        WeekProfit = _viewModel.Summary?.WeekProfit ?? 0,
                        MonthSales = _viewModel.Summary?.MonthSales ?? 0,
                        MonthProfit = _viewModel.Summary?.MonthProfit ?? 0,
                        TotalProducts = _viewModel.Summary?.TotalProducts ?? 0,
                        TotalCustomers = _viewModel.Summary?.TotalCustomers ?? 0,
                        LowStockCount = _viewModel.Summary?.LowStockCount ?? 0,
                        OutOfStockCount = _viewModel.Summary?.OutOfStockCount ?? 0,
                        MonthExpenses = _viewModel.Summary?.MonthExpenses ?? 0
                    },
                    last7DaysSales = string.IsNullOrEmpty(_viewModel.Last7DaysSalesJson) 
                        ? Array.Empty<object>() 
                        : JsonConvert.DeserializeObject(_viewModel.Last7DaysSalesJson),
                    last12MonthsSales = string.IsNullOrEmpty(_viewModel.Last12MonthsSalesJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.Last12MonthsSalesJson),
                    categorySales = string.IsNullOrEmpty(_viewModel.CategorySalesJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.CategorySalesJson),
                    profitByCategory = string.IsNullOrEmpty(_viewModel.ProfitByCategoryJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.ProfitByCategoryJson),
                    topSellingProducts = string.IsNullOrEmpty(_viewModel.TopSellingProductsJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.TopSellingProductsJson),
                    topProfitableProducts = string.IsNullOrEmpty(_viewModel.TopProfitableProductsJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.TopProfitableProductsJson),
                    inventoryStatus = string.IsNullOrEmpty(_viewModel.InventoryStatusJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.InventoryStatusJson),
                    paymentMethod = string.IsNullOrEmpty(_viewModel.PaymentMethodJson)
                        ? Array.Empty<object>()
                        : JsonConvert.DeserializeObject(_viewModel.PaymentMethodJson)
                };

                var json = JsonConvert.SerializeObject(dashboardData);
                var script = $"setDashboardData({json});";
                
                await StatisticsWebView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending data to dashboard: {ex.Message}");
            }
        }
    }
}
