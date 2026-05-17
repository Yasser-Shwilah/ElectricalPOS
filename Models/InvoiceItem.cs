using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    /// <summary>
    /// جدول تفاصيل الفاتورة - InvoiceItem Entity
    /// </summary>
    public class InvoiceItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// InvoiceId (مفتاح أجنبي - Foreign Key)
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// ProductId (مفتاح أجنبي - Foreign Key)
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// اسم_المادة (Product Name - Saved copy at time of sale)
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// الكمية (Quantity)
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// سعر_الوحدة (Unit Price)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// الإجمالي_الفرعي (Subtotal = Quantity × Unit Price)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // Navigation properties
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
