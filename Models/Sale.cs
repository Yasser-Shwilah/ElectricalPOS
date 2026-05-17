using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique invoice number in format INV-YYYYMMDD-XXXX
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }

        [Required]
        public int AccountantId { get; set; }

        public int? CustomerId { get; set; }

        /// <summary>
        /// Customer name (optional, can be entered at sale time)
        /// </summary>
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Customer phone (optional)
        /// </summary>
        [MaxLength(50)]
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// Previous balance from customer account
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousBalance { get; set; }

        /// <summary>
        /// Current balance after this sale
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Notes for the invoice
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("AccountantId")]
        public virtual User Accountant { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
