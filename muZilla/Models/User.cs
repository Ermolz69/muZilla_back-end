using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace muZilla.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool ReceiveNotifications { get; set; }
        public bool IsBanned { get; set; }
        public bool TwoFactoredAuthentification { get; set; }
        
        public virtual AccessLevel AccessLevel { get; set; }
        public virtual Image ProfilePicture { get; set; }
        public virtual ICollection<FriendsCouple> Friends { get; set; }
        public virtual ICollection<BlockedUser> Blocked { get; set; }
    }
}
