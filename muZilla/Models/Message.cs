using System;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string SenderLogin { get; set; }

        [Required]
        public string ReceiverLogin { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}