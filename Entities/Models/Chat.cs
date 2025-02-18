namespace muZilla.Entities.Models
{
    public class Chat : IModel
    {
        public int Id { get; set; }

        public ICollection<User> Members { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}
