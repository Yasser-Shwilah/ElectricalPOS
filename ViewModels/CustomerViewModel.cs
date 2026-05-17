using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class CustomerViewModel : ObservableObject
    {
        private readonly CustomerService _customerService = new();

        [ObservableProperty]
        private ObservableCollection<Customer> customers = new();

        [ObservableProperty]
        private ObservableCollection<Sale> customerSales = new();

        [ObservableProperty]
        private Customer? selectedCustomer;

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

        public CustomerViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadCustomersAsync();
        }

        [RelayCommand]
        private async Task LoadCustomersAsync()
        {
            var customerList = await _customerService.GetAllCustomersAsync();
            Customers.Clear();
            foreach (var customer in customerList)
            {
                Customers.Add(customer);
            }
        }

        [RelayCommand]
        private async Task AddCustomerAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter customer name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var customer = new Customer
            {
                Name = Name,
                ContactInfo = ContactInfo,
                TotalDebt = 0
            };

            var success = await _customerService.AddCustomerAsync(customer);
            if (success)
            {
                MessageBox.Show("Customer added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadCustomersAsync();
            }
            else
            {
                MessageBox.Show("Failed to add customer", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateCustomerAsync()
        {
            if (SelectedCustomer == null || string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedCustomer.Name = Name;
            SelectedCustomer.ContactInfo = ContactInfo;

            var success = await _customerService.UpdateCustomerAsync(SelectedCustomer);
            if (success)
            {
                MessageBox.Show("Customer updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadCustomersAsync();
            }
            else
            {
                MessageBox.Show("Failed to update customer", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to delete", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedCustomer.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _customerService.DeleteCustomerAsync(SelectedCustomer.Id);
                if (success)
                {
                    MessageBox.Show("Customer deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadCustomersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete customer", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void EditCustomer()
        {
            if (SelectedCustomer != null)
            {
                Name = SelectedCustomer.Name;
                ContactInfo = SelectedCustomer.ContactInfo;
                TotalDebt = SelectedCustomer.TotalDebt;
                IsEditMode = true;
                IsViewMode = false;
            }
        }

        [RelayCommand]
        private async Task ViewCustomerAccount()
        {
            if (SelectedCustomer != null)
            {
                var sales = await _customerService.GetCustomerSalesAsync(SelectedCustomer.Id);
                CustomerSales.Clear();
                foreach (var sale in sales)
                {
                    CustomerSales.Add(sale);
                }
                TotalDebt = SelectedCustomer.TotalDebt;
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
            SelectedCustomer = null;
            IsEditMode = false;
            IsViewMode = false;
            CustomerSales.Clear();
        }
    }
}
