using System.ComponentModel.DataAnnotations;

namespace muZilla.Entities.Models
{
    public class FriendsCouple : IModel
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FriendId { get; set; }

        public virtual User User { get; set; }
        public virtual User Friend { get; set; }
    }
}
