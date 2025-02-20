using System.ComponentModel.DataAnnotations;

namespace muZilla.Entities.Models
{
    public class SupportMessage : IModel
    {
        public int Id { get; set; }

        [Required]
        public string SenderLogin { get; set; }

        [Required]
        public string ReceiverLogin { get; set; }

        [Required]
        public int SupporterId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}