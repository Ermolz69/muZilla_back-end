using muZilla.Entities.Enums;

namespace muZilla.Application.DTOs.Message
{
    public class MessageDTO
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; } 
    }
}
