using System;
using System.ComponentModel.DataAnnotations;

namespace ElectricalPOS.Models
{
    public class Backup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        [MaxLength(50)]
        public string BackupType { get; set; } = "Manual"; // Manual, Automatic, Scheduled

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Completed"; // Completed, Failed, InProgress

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? CreatedBy { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        // Additional metadata for better backup management
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(50)]
        public string? CompressionType { get; set; } // None, ZIP, GZIP

        public bool IsEncrypted { get; set; } = false;
    }
}
