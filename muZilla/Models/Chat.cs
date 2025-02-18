namespace muZilla.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public ICollection<User> Members { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}
