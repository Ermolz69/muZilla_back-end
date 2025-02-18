using muZilla.Utils.Chat;

namespace muZilla.DTOs.Message
{
    public class MessageDTO
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; } 
    }
}
