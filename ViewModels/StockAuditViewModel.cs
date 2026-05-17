using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.ViewModels
{
    public partial class StockAuditViewModel : ObservableObject
    {
        private readonly ProductService _productService = new();
        private readonly CashTransactionService _cashTransactionService = new();
        private readonly StockMovementService _stockMovementService = new();

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<CategoryAuditSummary> categorySummaries = new();

        [ObservableProperty]
        private int totalProductsCount;

        [ObservableProperty]
        private int totalProductsQuantity;

        [ObservableProperty]
        private decimal totalInventoryValueAtCost;

        [ObservableProperty]
        private decimal totalInventoryValueAtSelling;

        [ObservableProperty]
        private decimal totalExpectedProfit;

        [ObservableProperty]
        private decimal currentCashBalance;

        [ObservableProperty]
        private DateTime auditDate = DateTime.Now;

        [ObservableProperty]
        private string auditorName = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasPerformedAudit;

        [ObservableProperty]
        private DateTime? performedAuditDate;

        private decimal _totalAssets;
        public decimal TotalAssets
        {
            get => _totalAssets;
            set => SetProperty(ref _totalAssets, value);
        }

        public StockAuditViewModel()
        {
            LoadAuditDataAsync();
        }

        [RelayCommand]
        private async Task PerformAuditAsync()
        {
            IsLoading = true;
            try
            {
                // Load all products
                var productList = await _productService.GetAllProductsAsync();
                Products.Clear();
                foreach (var product in productList)
                {
                    Products.Add(product);
                }

                // Calculate totals
                TotalProductsCount = Products.Count;
                TotalProductsQuantity = Products.Sum(p => p.Quantity);
                TotalInventoryValueAtCost = Products.Sum(p => p.Quantity * p.PurchasePrice);
                TotalInventoryValueAtSelling = Products.Sum(p => p.Quantity * p.SalePrice);
                TotalExpectedProfit = TotalInventoryValueAtSelling - TotalInventoryValueAtCost;

                // Get cash balance
                CurrentCashBalance = await _cashTransactionService.GetCurrentCashBalanceAsync();

                // Calculate total assets
                TotalAssets = TotalInventoryValueAtSelling + CurrentCashBalance;

                // Calculate category summaries
                await CalculateCategorySummariesAsync();

                // Set audit date only when performing audit
                PerformedAuditDate = DateTime.Now;
                HasPerformedAudit = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadAuditDataAsync()
        {
            await PerformAuditAsync();
        }

        private async Task CalculateCategorySummariesAsync()
        {
            CategorySummaries.Clear();

            using var context = new ElectricalPOSContext();
            var categories = await context.Categories.Include(c => c.Products).ToListAsync();

            foreach (var category in categories)
            {
                var categoryProducts = Products.Where(p => p.CategoryId == category.Id).ToList();
                if (categoryProducts.Any())
                {
                    CategorySummaries.Add(new CategoryAuditSummary
                    {
                        CategoryName = category.Name,
                        ProductsCount = categoryProducts.Count,
                        TotalQuantity = categoryProducts.Sum(p => p.Quantity),
                        TotalValueAtCost = categoryProducts.Sum(p => p.Quantity * p.PurchasePrice),
                        TotalValueAtSelling = categoryProducts.Sum(p => p.Quantity * p.SalePrice)
                    });
                }
            }
        }

        [RelayCommand]
        private async Task ExportToTextAsync()
        {
            try
            {
                if (!HasPerformedAudit)
                {
                    System.Windows.MessageBox.Show("يرجى تنفيذ الجرد أولاً", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"جرد المخزون_{PerformedAuditDate:yyyyMMdd_HHmmss}.txt",
                    Title = "تصدير تقرير الجرد"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var report = GenerateAuditReport();
                    await File.WriteAllTextAsync(saveDialog.FileName, report);
                    System.Windows.MessageBox.Show("تم تصدير تقرير الجرد بنجاح!", "نجاح", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في تصدير التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task PrintAuditReportAsync()
        {
            try
            {
                if (!HasPerformedAudit)
                {
                    System.Windows.MessageBox.Show("يرجى تنفيذ الجرد أولاً", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    FileName = $"جرد المخزون_{PerformedAuditDate:yyyyMMdd_HHmmss}.txt",
                    Title = "تصدير تقرير الجرد"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var report = GenerateAuditReport();
                    await File.WriteAllTextAsync(saveDialog.FileName, report, System.Text.Encoding.UTF8);
                    
                    // Show preview
                    var previewWindow = new Window
                    {
                        Title = "معاينة تقرير الجرد",
                        Width = 800,
                        Height = 600,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        FlowDirection = FlowDirection.RightToLeft,
                        Content = new System.Windows.Controls.ScrollViewer
                        {
                            Content = new System.Windows.Controls.TextBox
                            {
                                Text = report,
                                IsReadOnly = true,
                                TextWrapping = TextWrapping.Wrap,
                                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                                FontSize = 11,
                                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                                Margin = new Thickness(10)
                            }
                        }
                    };
                    
                    previewWindow.ShowDialog();
                    
                    System.Windows.MessageBox.Show("تم تصدير تقرير الجرد بنجاح!", "نجاح", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في تصدير التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateAuditReport()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine("                        تقرير الجرد الشامل                        ");
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"تاريخ الجرد: {PerformedAuditDate:yyyy/MM/dd HH:mm:ss}");
            if (!string.IsNullOrEmpty(AuditorName))
                sb.AppendLine($"الجرد أجراه: {AuditorName}");
            sb.AppendLine();
            
            // Summary Section
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("                         ملخص الجرد                            ");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine($"إجمالي عدد الأصناف: {TotalProductsCount:N0}");
            sb.AppendLine($"إجمالي كمية المنتجات: {TotalProductsQuantity:N0}");
            sb.AppendLine();
            sb.AppendLine("────────────────────── قيمه المخزون ────────────────────────");
            sb.AppendLine();
            sb.AppendLine($"قيمة المخزون بسعر التكلفة: {TotalInventoryValueAtCost:N2} $");
            sb.AppendLine($"قيمة المخزون بسعر البيع:   {TotalInventoryValueAtSelling:N2} $");
            sb.AppendLine($"الربح المتوقع:             {TotalExpectedProfit:N2} $");
            sb.AppendLine();
            
            sb.AppendLine("────────────────────────── الصندوق ─────────────────────────");
            sb.AppendLine();
            sb.AppendLine($"الرصيد الحالي في الصندوق: {CurrentCashBalance:N2} $");
            sb.AppendLine();
            
            sb.AppendLine("────────────────────── الإجمالي الكلي ──────────────────────");
            sb.AppendLine();
            sb.AppendLine($"إجمالي قيمة المخزون + الصندوق: {(TotalInventoryValueAtSelling + CurrentCashBalance):N2} $");
            sb.AppendLine();
            
            // Category Details
            if (CategorySummaries.Any())
            {
                sb.AppendLine("───────────────────────────────────────────────────────────────");
                sb.AppendLine("                      تفاصيل الأصناف                          ");
                sb.AppendLine("───────────────────────────────────────────────────────────────");
                sb.AppendLine();
                
                foreach (var cat in CategorySummaries)
                {
                    sb.AppendLine($"التصنيف: {cat.CategoryName}");
                    sb.AppendLine($"  - عدد الأصناف: {cat.ProductsCount}");
                    sb.AppendLine($"  - إجمالي الكمية: {cat.TotalQuantity}");
                    sb.AppendLine($"  - القيمة بسعر التكلفة: {cat.TotalValueAtCost:N2} $");
                    sb.AppendLine($"  - القيمة بسعر البيع: {cat.TotalValueAtSelling:N2} $");
                    sb.AppendLine();
                }
            }
            
            // Products Details
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("                     تفاصيل المنتجات                          ");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine($"{"الرقم".PadRight(5)} {"اسم المنتج".PadRight(25)} {"التصنيف".PadRight(15)} {"الكمية".PadRight(8)} {"سعر البيع".PadRight(12)} {"القيمة الإجمالية".PadRight(15)}");
            sb.AppendLine(new string('-', 90));
            
            foreach (var product in Products.OrderByDescending(p => p.Quantity * p.SalePrice))
            {
                var categoryName = product.Category?.Name ?? "-";
                var totalValue = product.Quantity * product.SalePrice;
                sb.AppendLine($"{product.Id.ToString().PadRight(5)} {product.Name.PadRight(25).Substring(0, Math.Min(25, product.Name.Length))} {categoryName.PadRight(15).Substring(0, Math.Min(15, categoryName.Length))} {product.Quantity.ToString().PadRight(8)} {product.SalePrice.ToString("N2").PadRight(12)} {totalValue.ToString("N2").PadRight(15)}");
            }
            
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine($"تم إنشاء هذا التقرير في: {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
            sb.AppendLine("                     نهاية التقرير                            ");
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            
            return sb.ToString();
        }

        [RelayCommand]
        private void RefreshAudit()
        {
            PerformAuditAsync();
        }
    }

    public class CategoryAuditSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValueAtCost { get; set; }
        public decimal TotalValueAtSelling { get; set; }
    }
}
