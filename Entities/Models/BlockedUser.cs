using System.ComponentModel.DataAnnotations;

namespace muZilla.Entities.Models 
{
    public class BlockedUser : IModel
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BlockedId { get; set; }

        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public virtual User User { get; set; }
        public virtual User Blocked { get; set; }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
