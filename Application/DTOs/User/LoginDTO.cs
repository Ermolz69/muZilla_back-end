using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.User
{
    /// <summary>
    /// Data Transfer Object for user login.
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// The user's login (username or email).
        /// </summary>
        [Required(ErrorMessage = "Login is required.")]
        public string Login { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }

}
