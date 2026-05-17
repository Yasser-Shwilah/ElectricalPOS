using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("PurchaseDate")]
        public DateTime Date { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debt { get; set; }

        // Indicates if purchase is fully paid
        public bool? IsPaid { get; set; }

        // Computed property to check if paid (handles null values)
        [NotMapped]
        public bool IsFullyPaid => IsPaid == true || (Debt <= 0 && TotalAmount > 0);

        // Navigation properties
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
