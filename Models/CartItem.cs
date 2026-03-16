using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    /// <summary>
    /// Represents an item in the shopping cart with editable quantity and unit price
    /// </summary>
    public partial class CartItem : ObservableObject
    {
        [ObservableProperty]
        private int productId;

        [ObservableProperty]
        private string productName = string.Empty;

        [ObservableProperty]
        private int availableStock;

        [ObservableProperty]
        private int quantity = 1;

        [ObservableProperty]
        private decimal unitPrice;

        [ObservableProperty]
        private decimal totalPrice;

        /// <summary>
        /// Recalculates the total price based on quantity and unit price
        /// </summary>
        public void RecalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
        }

        partial void OnQuantityChanged(int value)
        {
            RecalculateTotal();
        }

        partial void OnUnitPriceChanged(decimal value)
        {
            RecalculateTotal();
        }
    }
}
