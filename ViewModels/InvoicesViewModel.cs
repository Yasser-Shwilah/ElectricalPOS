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

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isInvoiceDetailsVisible;

        [ObservableProperty]
        private Sale? invoiceDetails;

        public InvoicesViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
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
                LoadDataAsync();
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
        private void ClearSearch()
        {
            SearchInvoiceNumber = string.Empty;
            LoadDataAsync();
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
                if (fullSale != null)
                {
                    // Print functionality - will use the print service
                    var printService = new InvoicePrintService();
                    await printService.PrintInvoiceAsync(fullSale);
                }
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
    }
}
