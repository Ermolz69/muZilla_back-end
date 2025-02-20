using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Message
{
    /// <summary>
    /// Data Transfer Object for the last message in a conversation.
    /// </summary>
    public class LastMessageDTO
    {
        /// <summary>
        /// The username or identifier of the sender.
        /// </summary>
        [Required(ErrorMessage = "Sender is required.")]
        public string From { get; set; }

        /// <summary>
        /// The content of the message.
        /// </summary>
        [Required(ErrorMessage = "Message content is required.")]
        public string Content { get; set; }

        /// <summary>
        /// The timestamp when the message was sent (in UTC).
        /// </summary>
        [Required(ErrorMessage = "Timestamp is required.")]
        public DateTime Timestamp { get; set; }
    }
}
