using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.User
{
    /// <summary>
    /// DTO for public user data.
    /// </summary>
    public class UserPublicDataDTO
    {
        /// <summary>
        /// The public username of the user.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        public string Username { get; set; }

        /// <summary>
        /// The unique public ID of the user.
        /// </summary>
        public int PublicId { get; set; }

        /// <summary>
        /// The ID representing the user's access level.
        /// </summary>
        public int AccessLevelId { get; set; }

        /// <summary>
        /// The ID of the user's profile picture (optional).
        /// </summary>
        public int? ProfilePictureId { get; set; }
    }
}
