namespace muZilla.Entities.Models
{
    public class Chat : IModel
    {
        public int Id { get; set; }
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ICollection<User> Members { get; set; }
        public ICollection<Message> Messages { get; set; }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
