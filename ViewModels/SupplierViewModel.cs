using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class SupplierViewModel : ObservableObject
    {
        private readonly SupplierService _supplierService = new();

        [ObservableProperty]
        private ObservableCollection<Supplier> suppliers = new();

        [ObservableProperty]
        private ObservableCollection<Purchase> supplierPurchases = new();

        [ObservableProperty]
        private Supplier? selectedSupplier;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string contactInfo = string.Empty;

        [ObservableProperty]
        private decimal totalDebt;

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private bool isViewMode;

        public SupplierViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadSuppliersAsync();
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
        private async Task AddSupplierAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter supplier name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var supplier = new Supplier
            {
                Name = Name,
                ContactInfo = ContactInfo,
                TotalDebt = 0
            };

            var success = await _supplierService.AddSupplierAsync(supplier);
            if (success)
            {
                MessageBox.Show("Supplier added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadSuppliersAsync();
            }
            else
            {
                MessageBox.Show("Failed to add supplier", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateSupplierAsync()
        {
            if (SelectedSupplier == null || string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedSupplier.Name = Name;
            SelectedSupplier.ContactInfo = ContactInfo;

            var success = await _supplierService.UpdateSupplierAsync(SelectedSupplier);
            if (success)
            {
                MessageBox.Show("Supplier updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadSuppliersAsync();
            }
            else
            {
                MessageBox.Show("Failed to update supplier", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteSupplierAsync()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Please select a supplier to delete", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedSupplier.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _supplierService.DeleteSupplierAsync(SelectedSupplier.Id);
                if (success)
                {
                    MessageBox.Show("Supplier deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadSuppliersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete supplier", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void EditSupplier()
        {
            if (SelectedSupplier != null)
            {
                Name = SelectedSupplier.Name;
                ContactInfo = SelectedSupplier.ContactInfo;
                TotalDebt = SelectedSupplier.TotalDebt;
                IsEditMode = true;
                IsViewMode = false;
            }
        }

        [RelayCommand]
        private async Task ViewSupplierAccount()
        {
            if (SelectedSupplier != null)
            {
                var purchases = await _supplierService.GetSupplierPurchasesAsync(SelectedSupplier.Id);
                SupplierPurchases.Clear();
                foreach (var purchase in purchases)
                {
                    SupplierPurchases.Add(purchase);
                }
                TotalDebt = SelectedSupplier.TotalDebt;
                IsViewMode = true;
                IsEditMode = false;
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            Name = string.Empty;
            ContactInfo = string.Empty;
            TotalDebt = 0;
            SelectedSupplier = null;
            IsEditMode = false;
            IsViewMode = false;
            SupplierPurchases.Clear();
        }
    }
}
