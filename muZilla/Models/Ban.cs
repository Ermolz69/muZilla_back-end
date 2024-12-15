using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Models
{
    public class Ban
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BannedByUserId { get; set; }

        [ForeignKey("BannedByUserId")]
        public virtual User BannedByUser { get; set; }

        [Required]
        public int BannedUserId { get; set; }

        [ForeignKey("BannedUserId")]
        public virtual User BannedUser { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime BanUntilUtc { get; set; }

        [Required]
        public DateTime BannedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
