using ElectricalPOS.Models;
using ElectricalPOS.Services;
using ElectricalPOS.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectricalPOS.Views
{
    public partial class SalesManagementView : UserControl
    {
        private readonly InvoicePrintService _printService = new();

        public SalesManagementView(User currentUser)
        {
            InitializeComponent();
            DataContext = new SalesViewModel(currentUser);
        }

        /// <summary>
        /// Validates that only numeric input is entered
        /// </summary>
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            // Use regex to check if text contains only digits and decimal point
            Regex regex = new Regex(@"^[0-9.]+$");
            return regex.IsMatch(text);
        }

        /// <summary>
        /// Handles the print invoice button click
        /// </summary>
        private async void PrintInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalesViewModel vm)
            {
                if (vm.LastCompletedSale != null)
                {
                    try
                    {
                        await _printService.PrintInvoiceAsync(vm.LastCompletedSale);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // If no completed sale yet, show sales list for selection
                    MessageBox.Show("الرجاء إتمام عملية البيع أولاً لطباعة الفاتورة", 
                        "طباعة الفاتورة", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Prints a specific sale invoice (can be called from sales history)
        /// </summary>
        public async Task PrintSaleInvoiceAsync(Sale sale)
        {
            try
            {
                await _printService.PrintInvoiceAsync(sale);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Shows invoice preview
        /// </summary>
        public async Task ShowInvoicePreviewAsync(Sale sale)
        {
            try
            {
                await _printService.ShowInvoicePreviewAsync(sale);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض الفاتورة: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
