using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class ProductViewModel : ObservableObject
    {
        private readonly ProductService _productService = new();
        private readonly CategoryService _categoryService = new();

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private decimal purchasePrice;

        [ObservableProperty]
        private decimal salePrice;

        [ObservableProperty]
        private int quantity;

        [ObservableProperty]
        private string? modelNumber;

        [ObservableProperty]
        private bool isEditMode;

        public ProductViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadProductsAsync();
            await LoadCategoriesAsync();
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
        private async Task LoadCategoriesAsync()
        {
            var categoryList = await _categoryService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categoryList)
            {
                Categories.Add(category);
            }
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null)
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = new Product
            {
                Name = Name,
                CategoryId = SelectedCategory.Id,
                PurchasePrice = PurchasePrice,
                SalePrice = SalePrice,
                Quantity = Quantity,
                ModelNumber = ModelNumber
            };

            var success = await _productService.AddProductAsync(product);
            if (success)
            {
                MessageBox.Show("Product added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show("Failed to add product", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateProductAsync()
        {
            if (SelectedProduct == null || string.IsNullOrWhiteSpace(Name) || SelectedCategory == null)
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedProduct.Name = Name;
            SelectedProduct.CategoryId = SelectedCategory.Id;
            SelectedProduct.PurchasePrice = PurchasePrice;
            SelectedProduct.SalePrice = SalePrice;
            SelectedProduct.Quantity = Quantity;
            SelectedProduct.ModelNumber = ModelNumber;

            var success = await _productService.UpdateProductAsync(SelectedProduct);
            if (success)
            {
                MessageBox.Show("Product updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show("Failed to update product", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product to delete", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedProduct.Name}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _productService.DeleteProductAsync(SelectedProduct.Id);
                if (success)
                {
                    MessageBox.Show("Product deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadProductsAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete product", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void EditProduct()
        {
            if (SelectedProduct != null)
            {
                Name = SelectedProduct.Name;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedProduct.CategoryId);
                PurchasePrice = SelectedProduct.PurchasePrice;
                SalePrice = SelectedProduct.SalePrice;
                Quantity = SelectedProduct.Quantity;
                ModelNumber = SelectedProduct.ModelNumber;
                IsEditMode = true;
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            Name = string.Empty;
            SelectedCategory = null;
            PurchasePrice = 0;
            SalePrice = 0;
            Quantity = 0;
            ModelNumber = null;
            SelectedProduct = null;
            IsEditMode = false;
        }
    }
}
