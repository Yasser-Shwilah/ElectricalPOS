using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private User currentUser;

        [ObservableProperty]
        private object? currentView;

        [ObservableProperty]
        private string welcomeMessage = string.Empty;

        public bool IsAdmin => CurrentUser?.Role == "Admin";
        public bool IsAccountant => CurrentUser?.Role == "Accountant";

        // Visibility properties for different roles
        // Admin: Full access
        // Accountant: Sales only
        public bool CanShowSuppliers => IsAdmin;
        public bool CanShowCustomers => IsAdmin;
        public bool CanShowCategories => IsAdmin;
        public bool CanShowUsers => IsAdmin;
        public bool CanShowProducts => IsAdmin;
        public bool CanShowSales => IsAdmin || IsAccountant;
        public bool CanShowPurchases => IsAdmin;
        public bool CanShowInventory => IsAdmin;
        public bool CanShowCash => IsAdmin;
        public bool CanShowInvoices => IsAdmin;
        public bool CanShowStatistics => IsAdmin;
        public bool CanShowBackup => IsAdmin;
        public bool CanShowStockAudit => IsAdmin;

        public MainViewModel(User user)
        {
            CurrentUser = user;
            var roleText = user.Role == "Admin" ? "مدير النظام" : "محاسب";
            WelcomeMessage = $"مرحباً، {user.Username} ({roleText})";
            
            // Set Sales as the default view when admin logs in
            CurrentView = new Views.SalesManagementView(CurrentUser);
        }

        [RelayCommand]
        private void ShowProducts()
        {
            if (!CanShowProducts) return;
            CurrentView = new Views.ProductManagementView();
        }

        [RelayCommand]
        private void ShowSales()
        {
            if (!CanShowSales) return;
            CurrentView = new Views.SalesManagementView(CurrentUser);
        }

        [RelayCommand]
        private void ShowSuppliers()
        {
            if (!CanShowSuppliers) return;
            CurrentView = new Views.SupplierManagementView();
        }

        [RelayCommand]
        private void ShowCustomers()
        {
            if (!CanShowCustomers) return;
            CurrentView = new Views.CustomerManagementView();
        }

        [RelayCommand]
        private void ShowPurchases()
        {
            if (!CanShowPurchases) return;
            CurrentView = new Views.PurchaseManagementView();
        }

        [RelayCommand]
        private void ShowInventory()
        {
            if (!CanShowInventory) return;
            CurrentView = new Views.InventoryManagementView();
        }

        [RelayCommand]
        private void ShowCash()
        {
            if (!CanShowCash) return;
            CurrentView = new Views.CashManagementView();
        }

        [RelayCommand]
        private void ShowUsers()
        {
            if (IsAdmin)
            {
                CurrentView = new Views.UserManagementView();
            }
        }


        [RelayCommand]
        private void ShowCategories()
        {
            if (IsAdmin)
            {
                CurrentView = new Views.CategoryManagementView();
            }
        }

        [RelayCommand]
        private void ShowInvoices()
        {
            if (!CanShowInvoices) return;
            CurrentView = new Views.InvoicesView();
        }

        [RelayCommand]
        private void ShowStatistics()
        {
            if (!CanShowStatistics) return;
            CurrentView = new Views.StatisticsView();
        }

        [RelayCommand]
        private void ShowBackup()
        {
            if (!CanShowBackup) return;
            CurrentView = new Views.BackupManagementView();
        }

        [RelayCommand]
        private void ShowStockAudit()
        {
            if (!CanShowStockAudit) return;
            CurrentView = new Views.StockAuditView();
        }


        [RelayCommand]
        private void Logout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();
                
                // Close main window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }
    }
}
