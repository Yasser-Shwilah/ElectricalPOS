using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly StockMovementService _stockMovementService = new();
        private readonly ProductService _productService = new();

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<StockMovement> stockMovements = new();

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private StockMovement? selectedMovement;

        [ObservableProperty]
        private int adjustmentQuantity;

        [ObservableProperty]
        private string adjustmentReason = string.Empty;

        [ObservableProperty]
        private string selectedMovementType = "Purchase";

        [ObservableProperty]
        private int lowStockThreshold = 10;

        [ObservableProperty]
        private ObservableCollection<Product> lowStockProducts = new();

        [ObservableProperty]
        private decimal totalStockValue;

        [ObservableProperty]
        private int totalProductsQuantity;

        [ObservableProperty]
        private bool showLowStockAlert;

        [ObservableProperty]
        private bool isAdjustmentMode;

        public List<string> MovementTypes { get; } = new() { "شراء", "بيع", "تسوية" };

        public InventoryViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadProductsAsync();
            await LoadStockMovementsAsync();
            await CheckLowStockAsync();
            await CalculateStockValueAsync();
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
            TotalProductsQuantity = Products.Sum(p => p.Quantity);
        }

        [RelayCommand]
        private async Task LoadStockMovementsAsync()
        {
            var movements = await _stockMovementService.GetAllStockMovementsAsync();
            StockMovements.Clear();
            foreach (var movement in movements)
            {
                StockMovements.Add(movement);
            }
        }

        [RelayCommand]
        private async Task FilterMovementsByTypeAsync()
        {
            if (string.IsNullOrEmpty(SelectedMovementType))
            {
                await LoadStockMovementsAsync();
            }
            else
            {
                var movements = await _stockMovementService.GetMovementsByTypeAsync(SelectedMovementType);
                StockMovements.Clear();
                foreach (var movement in movements)
                {
                    StockMovements.Add(movement);
                }
            }
        }

        [RelayCommand]
        private async Task CheckLowStockAsync()
        {
            var lowStock = await _stockMovementService.GetLowStockProductsAsync(LowStockThreshold);
            LowStockProducts.Clear();
            foreach (var kvp in lowStock)
            {
                var product = Products.FirstOrDefault(p => p.Name == kvp.Key);
                if (product != null)
                {
                    LowStockProducts.Add(product);
                }
            }
            ShowLowStockAlert = LowStockProducts.Count > 0;
        }

        [RelayCommand]
        private async Task CalculateStockValueAsync()
        {
            TotalStockValue = await _stockMovementService.GetTotalStockValueAsync();
        }

        [RelayCommand]
        private void StartAdjustment()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsAdjustmentMode = true;
        }

        [RelayCommand]
        private async Task PerformAdjustmentAsync()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AdjustmentReason))
            {
                MessageBox.Show("Please enter a reason for the adjustment", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await _stockMovementService.PerformStockAdjustmentAsync(
                SelectedProduct.Id, 
                AdjustmentQuantity, 
                AdjustmentReason);

            if (success)
            {
                MessageBox.Show($"Stock adjusted successfully!\nProduct: {SelectedProduct.Name}\nAdjustment: {AdjustmentQuantity}\nNew Quantity: {SelectedProduct.Quantity + AdjustmentQuantity}", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearAdjustmentForm();
                await LoadProductsAsync();
                await LoadStockMovementsAsync();
                await CalculateStockValueAsync();
            }
            else
            {
                MessageBox.Show("Failed to adjust stock", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task PerformStockTakeAsync()
        {
            var result = MessageBox.Show("This will open a stock take dialog where you can enter actual quantities.\nContinue?", 
                "Stock Take", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Stock take feature - Please update quantities directly in the products list", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ViewProductMovements()
        {
            if (SelectedProduct != null)
            {
                LoadProductMovementsAsync(SelectedProduct.Id);
            }
        }

        private async void LoadProductMovementsAsync(int productId)
        {
            var movements = await _stockMovementService.GetProductStockMovementsAsync(productId);
            StockMovements.Clear();
            foreach (var movement in movements)
            {
                StockMovements.Add(movement);
            }
        }

        private void ClearAdjustmentForm()
        {
            AdjustmentQuantity = 0;
            AdjustmentReason = string.Empty;
            IsAdjustmentMode = false;
        }

        [RelayCommand]
        private void CancelAdjustment()
        {
            ClearAdjustmentForm();
        }
    }
}
