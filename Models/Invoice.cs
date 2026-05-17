using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    /// <summary>
    /// جدول الفواتير - Invoice Entity
    /// </summary>
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// رقم_الفاتورة (Unique Invoice Number)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ_الفاتورة (Invoice Date)
        /// </summary>
        [Required]
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// اسم_الزبون (Customer Name)
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// هاتف_الزبون (Customer Phone - Optional)
        /// </summary>
        [MaxLength(50)]
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// الإجمالي (Total Amount)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        /// <summary>
        /// الرصيد_المدور_السابق (Previous Balance - Default 0.00)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousBalance { get; set; } = 0.00m;

        /// <summary>
        /// الرصيد_الحالي (Current Balance)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// تاريخ_الإنشاء (Creation Date)
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
