using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class SalesViewModel : ObservableObject
    {
        private readonly InvoiceService _invoiceService = new();
        private readonly ProductService _productService = new();
        private readonly CashTransactionService _cashTransactionService = new();
        private readonly User _currentUser;

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<CartItem> cartItems = new();

        [ObservableProperty]
        private ObservableCollection<Sale> sales = new();

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private int quantity = 1;

        [ObservableProperty]
        private decimal discount;

        [ObservableProperty]
        private decimal totalAmount;

        [ObservableProperty]
        private decimal finalAmount;

        // Customer information
        [ObservableProperty]
        private string customerName = string.Empty;

        [ObservableProperty]
        private string customerPhone = string.Empty;

        // Current sale for printing
        [ObservableProperty]
        private Sale? lastCompletedSale;

        // Loading state
        [ObservableProperty]
        private bool isLoading;

        public SalesViewModel(User currentUser)
        {
            _currentUser = currentUser;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await LoadProductsAsync();
                await LoadSalesAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            var productList = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in productList.Where(p => p.Quantity > 0))
            {
                Products.Add(product);
            }
        }

        [RelayCommand]
        private async Task LoadSalesAsync()
        {
            var salesList = await _invoiceService.GetAllSalesAsync();
            Sales.Clear();
            foreach (var sale in salesList)
            {
                Sales.Add(sale);
            }
        }

        [RelayCommand]
        private void AddToCart()
        {
            if (SelectedProduct == null || Quantity <= 0)
            {
                MessageBox.Show("الرجاء اختيار منتج وإدخال الكمية", "خطأ في التحقق", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Quantity > SelectedProduct.Quantity)
            {
                MessageBox.Show($"الكمية المتوفرة غير كافية. المتوفر: {SelectedProduct.Quantity}", "خطأ في المخزون", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingItem = CartItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
            if (existingItem != null)
            {
                // Check total quantity doesn't exceed available stock
                int newQuantity = existingItem.Quantity + Quantity;
                if (newQuantity > SelectedProduct.Quantity)
                {
                    MessageBox.Show($"الكمية الإجمالية تتجاوز المخزون المتوفر. المتوفر: {SelectedProduct.Quantity}", "خطأ في المخزون", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                existingItem.Quantity = newQuantity;
            }
            else
            {
                CartItems.Add(new CartItem
                {
                    ProductId = SelectedProduct.Id,
                    ProductName = SelectedProduct.Name,
                    Quantity = Quantity,
                    UnitPrice = SelectedProduct.SalePrice,
                    AvailableStock = SelectedProduct.Quantity
                });
            }

            CalculateTotals();
            Quantity = 1;
        }

        [RelayCommand]
        private void RemoveFromCart(CartItem item)
        {
            CartItems.Remove(item);
            CalculateTotals();
        }

        [RelayCommand]
        private async Task CompleteSaleAsync()
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("السلة فارغة", "خطأ في التحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate cart items
            foreach (var item in CartItems)
            {
                var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    MessageBox.Show($"المنتج {item.ProductName} لم يعد متوفراً", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (item.Quantity > product.Quantity)
                {
                    MessageBox.Show($"كمية {item.ProductName} تتجاوز المخزون المتوفر ({product.Quantity})", "خطأ في المخزون", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (item.UnitPrice <= 0)
                {
                    MessageBox.Show($"سعر {item.ProductName} يجب أن يكون أكبر من صفر", "خطأ في السعر", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            IsLoading = true;
            try
            {
                var sale = new Sale
                {
                    AccountantId = _currentUser.Id,
                    Discount = Discount
                };

                var saleItems = CartItems.Select(ci => new SaleItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.TotalPrice
                }).ToList();

                var result = await _invoiceService.CreateInvoiceAsync(
                    sale, 
                    saleItems,
                    CustomerName,
                    CustomerPhone);

                if (result.Success)
                {
                    LastCompletedSale = result.Sale;
                    var message = $"تم إتمام البيع بنجاح!\n" +
                                  $"رقم الفاتورة: {result.Sale?.InvoiceNumber}\n" +
                                  $"المجموع: {result.Sale?.TotalAmount:N2}";
                    
                    MessageBox.Show(message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    ClearCart();
                    await LoadProductsAsync();
                    await LoadSalesAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ClearCart()
        {
            CartItems.Clear();
            Discount = 0;
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CalculateTotals();
        }

        /// <summary>
        /// Updates a cart item's quantity when user edits it directly
        /// </summary>
        [RelayCommand]
        private void UpdateCartItemQuantity(CartItem item)
        {
            if (item.Quantity <= 0)
            {
                item.Quantity = 1;
            }
            
            // Check against available stock
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null && item.Quantity > product.Quantity)
            {
                item.Quantity = product.Quantity;
                MessageBox.Show($"الكمية تتجاوز المخزون المتوفر ({product.Quantity})", "تحذير", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            CalculateTotals();
        }

        /// <summary>
        /// Updates a cart item's unit price when user edits it directly
        /// </summary>
        [RelayCommand]
        private void UpdateCartItemPrice(CartItem item)
        {
            if (item.UnitPrice < 0)
            {
                item.UnitPrice = 0;
            }
            CalculateTotals();
        }

        partial void OnDiscountChanged(decimal value)
        {
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            TotalAmount = CartItems.Sum(i => i.TotalPrice);
            FinalAmount = TotalAmount - Discount;
        }

        /// <summary>
        /// Gets sales summary for today
        /// </summary>
        public async Task<(decimal TotalSales, decimal TotalProfit, int Count)> GetTodaySummaryAsync()
        {
            var totalSales = await _invoiceService.GetTotalSalesTodayAsync();
            var totalProfit = await _invoiceService.GetTotalProfitTodayAsync();
            var count = await _invoiceService.GetSalesCountTodayAsync();
            return (totalSales, totalProfit, count);
        }
    }
}
