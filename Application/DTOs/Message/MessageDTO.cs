using muZilla.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Message
{
    /// <summary>
    /// Data Transfer Object for sending messages.
    /// </summary>
    public class MessageDTO
    {
        /// <summary>
        /// The ID of the message recipient.
        /// </summary>
        [Required(ErrorMessage = "Receiver ID is required.")]
        public int ReceiverId { get; set; }

        /// <summary>
        /// The content of the message.
        /// </summary>
        [Required(ErrorMessage = "Message content is required.")]
        [MaxLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters.")]
        public string Content { get; set; }

        /// <summary>
        /// The type of the message (e.g., text, image, system notification, etc.).
        /// </summary>
        [Required(ErrorMessage = "Message type is required.")]
        public MessageType Type { get; set; }
    }
}
