using System.ComponentModel.DataAnnotations;

namespace muZilla.Models
{
    public class BlockedUser
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BlockedId { get; set; }

        public virtual User User { get; set; }
        public virtual User Blocked { get; set; }
    }
}
