using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MovementType { get; set; } = string.Empty; // Purchase, Sale, Adjustment

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(200)]
        public string RelatedDocument { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
