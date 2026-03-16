using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectricalPOS.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]  // Explicit NVARCHAR for Arabic support
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]  // Explicit NVARCHAR for Arabic support
        public string Role { get; set; } = string.Empty; // Admin or Accountant

        // Optional: Full name in Arabic
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]  // Explicit NVARCHAR for Arabic support
        public string? FullName { get; set; }

        // Navigation property
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
