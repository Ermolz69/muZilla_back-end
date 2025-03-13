using muZilla.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Message
{
    /// <summary>
    /// Data Transfer Object for sending messages.
    /// </summary>
    public class MessageDTO
    {

        [Required(ErrorMessage = "required.")]
        public int ChatId { get; set; }
        public string? Text { get; set; }
        public string? FileData { get; set; }
        [Required(ErrorMessage = "required.")]
        public MessageType Type { get; set; }
    }
}
