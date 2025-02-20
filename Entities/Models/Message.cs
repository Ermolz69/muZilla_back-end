using muZilla.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Entities.Models
{
    public class Message : IModel
    {
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ChatId { get; set; }

        public string? Text { get; set; }
        public byte[]? FileData { get; set; }
        public MessageType Type { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual Chat Chat { get; set; }

    }
}