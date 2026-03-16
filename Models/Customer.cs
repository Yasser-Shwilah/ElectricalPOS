using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]  // Explicit NVARCHAR for Arabic support
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]  // Explicit NVARCHAR for Arabic support
        public string ContactInfo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebt { get; set; }

        // Navigation property - customers can have sales
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
