namespace muZilla.DTOs
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool ReceiveNotifications { get; set; }
        public int AccessLevelId { get; set; }
        public int ProfilePictureId { get; set; }
    }
}
