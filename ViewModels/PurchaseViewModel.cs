using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class PurchaseViewModel : ObservableObject
    {
        private readonly PurchaseService _purchaseService = new();
        private readonly ProductService _productService = new();
        private readonly SupplierService _supplierService = new();
        private readonly CashTransactionService _cashTransactionService = new();

        [ObservableProperty]
        private ObservableCollection<Purchase> purchases = new();

        [ObservableProperty]
        private ObservableCollection<Supplier> suppliers = new();

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<PurchaseItemViewModel> purchaseItems = new();

        [ObservableProperty]
        private Supplier? selectedSupplier;

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private Purchase? selectedPurchase;

        [ObservableProperty]
        private DateTime purchaseDate = DateTime.Now;

        [ObservableProperty]
        private int itemQuantity = 1;

        [ObservableProperty]
        private decimal itemUnitPrice;

        [ObservableProperty]
        private decimal totalAmount;

        [ObservableProperty]
        private decimal paidAmount;

        [ObservableProperty]
        private decimal debt;

        [ObservableProperty]
        private decimal paymentAmount;

        [ObservableProperty]
        private bool isEditMode;

        public PurchaseViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadPurchasesAsync();
            await LoadSuppliersAsync();
            await LoadProductsAsync();
        }

        [RelayCommand]
        private async Task LoadPurchasesAsync()
        {
            var purchaseList = await _purchaseService.GetAllPurchasesAsync();
            Purchases.Clear();
            foreach (var purchase in purchaseList)
            {
                Purchases.Add(purchase);
            }
        }

        [RelayCommand]
        private async Task LoadSuppliersAsync()
        {
            var supplierList = await _supplierService.GetAllSuppliersAsync();
            Suppliers.Clear();
            foreach (var supplier in supplierList)
            {
                Suppliers.Add(supplier);
            }
        }

        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            var productList = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in productList)
            {
                Products.Add(product);
            }
        }

        [RelayCommand]
        private void AddItem()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ItemQuantity <= 0)
            {
                MessageBox.Show("Quantity must be greater than 0", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ItemUnitPrice <= 0)
            {
                MessageBox.Show("Unit price must be greater than 0", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if product already exists in the list
            var existingItem = PurchaseItems.FirstOrDefault(pi => pi.ProductId == SelectedProduct.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += ItemQuantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                PurchaseItems.Add(new PurchaseItemViewModel
                {
                    ProductId = SelectedProduct.Id,
                    ProductName = SelectedProduct.Name,
                    Quantity = ItemQuantity,
                    UnitPrice = ItemUnitPrice,
                    TotalPrice = ItemQuantity * ItemUnitPrice
                });
            }

            CalculateTotal();
            ClearItemForm();
        }

        [RelayCommand]
        private void RemoveItem(PurchaseItemViewModel item)
        {
            if (item != null)
            {
                PurchaseItems.Remove(item);
                CalculateTotal();
            }
        }

        [RelayCommand]
        private async Task CreatePurchaseAsync()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Please select a supplier", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PurchaseItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var purchase = new Purchase
            {
                Date = PurchaseDate,
                SupplierId = SelectedSupplier.Id
            };

            var items = PurchaseItems.Select(pi => new PurchaseItem
            {
                ProductId = pi.ProductId,
                Quantity = pi.Quantity,
                UnitPrice = pi.UnitPrice,
                TotalPrice = pi.TotalPrice
            }).ToList();

            var purchaseId = await _purchaseService.CreatePurchaseAsync(purchase, items);
            if (purchaseId > 0)
            {
                MessageBox.Show($"Purchase invoice created successfully!\nInvoice Number: PO-{purchaseId}\nStock has been updated automatically.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadPurchasesAsync();
            }
            else
            {
                MessageBox.Show("Failed to create purchase", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task PayPurchaseAsync()
        {
            if (SelectedPurchase == null)
            {
                MessageBox.Show("Please select a purchase to pay", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PaymentAmount <= 0)
            {
                MessageBox.Show("Payment amount must be greater than 0", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PaymentAmount > SelectedPurchase.Debt)
            {
                MessageBox.Show("Payment amount exceeds the debt", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await _purchaseService.PayPurchaseAsync(SelectedPurchase.Id, PaymentAmount);
            if (success)
            {
                MessageBox.Show("Payment recorded successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PaymentAmount = 0;
                await LoadPurchasesAsync();
            }
            else
            {
                MessageBox.Show("Failed to record payment", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeletePurchaseAsync()
        {
            if (SelectedPurchase == null)
            {
                MessageBox.Show("Please select a purchase to delete", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete purchase PO-{SelectedPurchase.Id}?\nThis will reverse stock and supplier debt.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _purchaseService.DeletePurchaseAsync(SelectedPurchase.Id);
                if (success)
                {
                    MessageBox.Show("Purchase deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPurchasesAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete purchase", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CalculateTotal()
        {
            TotalAmount = PurchaseItems.Sum(pi => pi.TotalPrice);
            Debt = TotalAmount - PaidAmount;
        }

        private void ClearItemForm()
        {
            SelectedProduct = null;
            ItemQuantity = 1;
            ItemUnitPrice = 0;
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedSupplier = null;
            SelectedProduct = null;
            SelectedPurchase = null;
            PurchaseDate = DateTime.Now;
            PurchaseItems.Clear();
            TotalAmount = 0;
            PaidAmount = 0;
            Debt = 0;
            PaymentAmount = 0;
            IsEditMode = false;
            ClearItemForm();
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            if (value != null)
            {
                ItemUnitPrice = value.PurchasePrice;
            }
        }

        partial void OnPaidAmountChanged(decimal value)
        {
            CalculateTotal();
        }
    }

    public partial class PurchaseItemViewModel : ObservableObject
    {
        public int ProductId { get; set; }
        
        [ObservableProperty]
        private string productName = string.Empty;

        [ObservableProperty]
        private int quantity;

        [ObservableProperty]
        private decimal unitPrice;

        [ObservableProperty]
        private decimal totalPrice;
    }
}
