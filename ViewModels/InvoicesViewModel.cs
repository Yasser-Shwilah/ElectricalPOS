using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class InvoicesViewModel : ObservableObject
    {
        private readonly InvoiceService _invoiceService = new();
        
        // Pagination
        private const int PageSize = 10;
        private List<Sale> _allSales = new();

        [ObservableProperty]
        private ObservableCollection<Sale> paginatedSales = new();

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        private int totalInvoices = 0;

        // Search
        [ObservableProperty]
        private string searchInvoiceNumber = string.Empty;

        [ObservableProperty]
        private Sale? selectedInvoice;

<<<<<<< HEAD
        partial void OnSelectedInvoiceChanged(Sale? value)
        {
            ReverseSaleCommand.NotifyCanExecuteChanged();
        }

=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isInvoiceDetailsVisible;

        [ObservableProperty]
        private Sale? invoiceDetails;

        public InvoicesViewModel()
        {
<<<<<<< HEAD
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
=======
            LoadDataAsync();
        }

        private async void LoadDataAsync()
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        {
            IsLoading = true;
            try
            {
                var salesList = await _invoiceService.GetAllSalesAsync();
                _allSales = salesList;
                TotalInvoices = _allSales.Count;
                TotalPages = (int)Math.Ceiling((double)TotalInvoices / PageSize);
                if (TotalPages == 0) TotalPages = 1;
                CurrentPage = 1;
                UpdatePaginatedSales();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdatePaginatedSales()
        {
            var skip = (CurrentPage - 1) * PageSize;
            var paged = _allSales
                .Skip(skip)
                .Take(PageSize)
                .ToList();

            PaginatedSales.Clear();
            foreach (var sale in paged)
            {
                PaginatedSales.Add(sale);
            }
        }

        [RelayCommand]
        private async Task SearchInvoiceAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchInvoiceNumber))
            {
<<<<<<< HEAD
                await LoadDataAsync();
=======
                LoadDataAsync();
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
                return;
            }

            IsLoading = true;
            try
            {
                var sale = await _invoiceService.GetSaleByInvoiceNumberAsync(SearchInvoiceNumber.Trim());
                if (sale != null)
                {
                    _allSales = new List<Sale> { sale };
                    TotalInvoices = 1;
                    TotalPages = 1;
                    CurrentPage = 1;
                    UpdatePaginatedSales();
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على فاتورة بهذا الرقم", "بحث الفواتير", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _allSales = new List<Sale>();
                    PaginatedSales.Clear();
                    TotalInvoices = 0;
                    TotalPages = 1;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
<<<<<<< HEAD
        private async Task ClearSearchAsync()
        {
            SearchInvoiceNumber = string.Empty;
            await LoadDataAsync();
=======
        private void ClearSearch()
        {
            SearchInvoiceNumber = string.Empty;
            LoadDataAsync();
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdatePaginatedSales();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePaginatedSales();
            }
        }

        [RelayCommand]
        private void FirstPage()
        {
            if (CurrentPage != 1)
            {
                CurrentPage = 1;
                UpdatePaginatedSales();
            }
        }

        [RelayCommand]
        private void LastPage()
        {
            if (CurrentPage != TotalPages)
            {
                CurrentPage = TotalPages;
                UpdatePaginatedSales();
            }
        }

        [RelayCommand]
        private async Task ViewInvoiceDetailsAsync(Sale? sale)
        {
            if (sale == null) return;

            IsLoading = true;
            try
            {
                var fullSale = await _invoiceService.GetSaleByIdAsync(sale.Id);
                if (fullSale != null)
                {
                    InvoiceDetails = fullSale;
                    IsInvoiceDetailsVisible = true;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CloseInvoiceDetails()
        {
            IsInvoiceDetailsVisible = false;
            InvoiceDetails = null;
        }

        [RelayCommand]
        private async Task PrintInvoiceAsync(Sale? sale)
        {
            if (sale == null) return;

            IsLoading = true;
            try
            {
                var fullSale = await _invoiceService.GetSaleByIdAsync(sale.Id);
<<<<<<< HEAD
                if (fullSale == null)
                {
                    MessageBox.Show("تعذّر تحميل بيانات الفاتورة.", "طباعة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var printService = new InvoicePrintService();
                await printService.PrintInvoiceAsync(fullSale);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "طباعة الفاتورة",
                    MessageBoxButton.OK, MessageBoxImage.Error);
=======
                if (fullSale != null)
                {
                    // Print functionality - will use the print service
                    var printService = new InvoicePrintService();
                    await printService.PrintInvoiceAsync(fullSale);
                }
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportToPdfAsync(Sale? sale)
        {
            if (sale == null) return;

            IsLoading = true;
            try
            {
                var fullSale = await _invoiceService.GetSaleByIdAsync(sale.Id);
                if (fullSale != null)
                {
                    // Export to PDF functionality - show in browser for now
                    var printService = new InvoicePrintService();
                    await printService.ShowInvoicePreviewAsync(fullSale);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
<<<<<<< HEAD

        private bool CanReverseSale(Sale? sale) => (sale ?? SelectedInvoice) != null;

        [RelayCommand(CanExecute = nameof(CanReverseSale))]
        private async Task ReverseSaleAsync(Sale? sale)
        {
            var target = sale ?? SelectedInvoice;
            if (target == null)
            {
                MessageBox.Show("يرجى اختيار فاتورة من القائمة أولاً.", "استرجاع فاتورة",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"سيتم التراجع الكامل عن عملية البيع للفاتورة رقم {target.InvoiceNumber} بمبلغ {target.TotalAmount:N2}.\n\n" +
                "• إرجاع كميات المنتجات إلى المخزون وتسجيل حركة مخزون\n" +
                "• حذف قيد الإيراد المرتبط من الصندوق (نفس مبلغ الفاتورة)\n" +
                "• حذف سجل الفاتورة من المبيعات بحيث تُحدَّث الإحصائيات (المبيعات والربح ورصيد النقد وقيمة المخزون)\n\n" +
                "لا يمكن التراجع عن هذا الإجراء. هل تريد المتابعة؟",
                "تأكيد استرجاع الفاتورة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (confirm != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            try
            {
                var (success, message) = await _invoiceService.ReverseSaleAsync(target.Id);
                MessageBox.Show(message, success ? "تم الاسترجاع" : "تعذر الاسترجاع",
                    MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (success)
                {
                    SelectedInvoice = null;
                    await LoadDataAsync();
                }
            }
            finally
            {
                IsLoading = false;
                ReverseSaleCommand.NotifyCanExecuteChanged();
            }
        }
=======
>>>>>>> 45a01fe8df36f882b267fd93476aba2fae06053e
    }
}
