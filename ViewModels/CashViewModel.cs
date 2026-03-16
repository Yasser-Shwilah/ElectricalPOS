using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class CashViewModel : ObservableObject
    {
        private readonly CashTransactionService _cashTransactionService = new();

        [ObservableProperty]
        private ObservableCollection<CashTransaction> cashTransactions = new();

        [ObservableProperty]
        private CashTransaction? selectedTransaction;

        [ObservableProperty]
        private string transactionType = "إيراد";

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private DateTime transactionDate = DateTime.Now;

        [ObservableProperty]
        private decimal totalIncome;

        [ObservableProperty]
        private decimal totalExpense;

        [ObservableProperty]
        private decimal currentBalance;

        [ObservableProperty]
        private decimal todayIncome;

        [ObservableProperty]
        private decimal todayExpense;

        [ObservableProperty]
        private decimal monthExpense;

        [ObservableProperty]
        private DateTime filterStartDate = DateTime.Today;

        [ObservableProperty]
        private DateTime filterEndDate = DateTime.Today;

        [ObservableProperty]
        private bool isEditMode;

        public List<string> TransactionTypes { get; } = new() { "إيراد", "مصروف" };

        public CashViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadTransactionsAsync();
            await LoadSummaryAsync();
        }

        [RelayCommand]
        private async Task LoadTransactionsAsync()
        {
            var transactions = await _cashTransactionService.GetCashTransactionsByDateRangeAsync(FilterStartDate, FilterEndDate);
            CashTransactions.Clear();
            foreach (var transaction in transactions)
            {
                CashTransactions.Add(transaction);
            }
        }

        [RelayCommand]
        private async Task LoadSummaryAsync()
        {
            var summary = await _cashTransactionService.GetCashSummaryAsync();
            TotalIncome = summary["TotalIncome"];
            TotalExpense = summary["TotalExpense"];
            CurrentBalance = summary["CurrentBalance"];
            TodayIncome = summary["TodayIncome"];
            TodayExpense = summary["TodayExpense"];
            MonthExpense = summary["MonthExpense"];
        }

        [RelayCommand]
        private async Task AddTransactionAsync()
        {
            if (Amount <= 0)
            {
                MessageBox.Show("Amount must be greater than 0", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                MessageBox.Show("Please enter a description", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var transaction = new CashTransaction
            {
                Date = TransactionDate,
                Type = TransactionType,
                Amount = Amount,
                Description = Description
            };

            var success = await _cashTransactionService.AddCashTransactionAsync(transaction);
            if (success)
            {
                MessageBox.Show("Transaction added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadTransactionsAsync();
                await LoadSummaryAsync();
            }
            else
            {
                MessageBox.Show("Failed to add transaction", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteTransactionAsync()
        {
            if (SelectedTransaction == null)
            {
                MessageBox.Show("Please select a transaction to delete", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete this transaction?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _cashTransactionService.DeleteCashTransactionAsync(SelectedTransaction.Id);
                if (success)
                {
                    MessageBox.Show("Transaction deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadTransactionsAsync();
                    await LoadSummaryAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete transaction", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task FilterByDateRangeAsync()
        {
            await LoadTransactionsAsync();
        }

        [RelayCommand]
        private async Task ShowTodayTransactionsAsync()
        {
            FilterStartDate = DateTime.Today;
            FilterEndDate = DateTime.Today;
            await LoadTransactionsAsync();
        }

        [RelayCommand]
        private async Task ShowAllTransactionsAsync()
        {
            FilterStartDate = DateTime.MinValue;
            FilterEndDate = DateTime.MaxValue;
            await LoadTransactionsAsync();
        }

        private void ClearForm()
        {
            TransactionType = "إيراد";
            Amount = 0;
            Description = string.Empty;
            TransactionDate = DateTime.Now;
            SelectedTransaction = null;
            IsEditMode = false;
        }

        [RelayCommand]
        private void ClearFormCommand()
        {
            ClearForm();
        }
    }
}
