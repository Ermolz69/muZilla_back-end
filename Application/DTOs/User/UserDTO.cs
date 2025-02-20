using System;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.User
{
    /// <summary>
    /// Data Transfer Object for user details.
    /// </summary>
    public class UserDTO
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// User's phone number.
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// User's date of birth.
        /// </summary>
        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Indicates whether the user wants to receive notifications.
        /// </summary>
        public bool ReceiveNotifications { get; set; }

        /// <summary>
        /// Public user data.
        /// </summary>
        [Required(ErrorMessage = "Public user data is required.")]
        public UserPublicDataDTO UserPublicData { get; set; }
    }
}

