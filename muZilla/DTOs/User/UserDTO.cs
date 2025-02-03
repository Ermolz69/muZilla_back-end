namespace muZilla.DTOs.User
{
    public class UserDTO
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool ReceiveNotifications { get; set; }
        public UserPublicDataDTO userPublicDataDTO { get; set; }
    }
}
