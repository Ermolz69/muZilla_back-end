using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Ban
{
    /// <summary>
    /// Data Transfer Object for Ban requests.
    /// </summary>
    public class BanRequestDTO
    {
        /// <summary>
        /// The ID of the entity (user, song, collection) to be banned.
        /// </summary>
        [Required(ErrorMessage = "IdToBan is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "IdToBan must be greater than 0.")]
        public int IdToBan { get; set; }

        /// <summary>
        /// The reason for banning.
        /// </summary>
        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot be longer than 500 characters.")]
        [MinLength(5, ErrorMessage = "Reason cannot be shorter than 10 characters.")]
        public string Reason { get; set; }

        /// <summary>
        /// The UTC date and time until which the ban is active. If null, the ban is permanent.
        /// </summary>
        [Required(ErrorMessage = "BanUntilUtc is required.")]
        public DateTime BanUntilUtc { get; set; }
    }

}
