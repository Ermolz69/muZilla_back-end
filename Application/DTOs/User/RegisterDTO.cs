using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.User
{
    /// <summary>
    /// Data Transfer Object for user registration.
    /// </summary>
    public class RegisterDTO
    {
        /// <summary>
        /// User account credentials (login and password).
        /// </summary>
        [Required(ErrorMessage = "Login information is required.")]
        public LoginDTO LoginDTO { get; set; }

        /// <summary>
        /// Additional user details.
        /// </summary>
        [Required(ErrorMessage = "User details are required.")]
        public UserDTO UserDTO { get; set; }
    }

}
