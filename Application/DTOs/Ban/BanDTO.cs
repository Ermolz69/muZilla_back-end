using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Ban
{
    /// <summary>
    /// Data Transfer Object for Ban information.
    /// </summary>
    public class BanDTO
    {
        /// <summary>
        /// Username of the admin who issued the ban.
        /// </summary>
        [Required(ErrorMessage = "AdminId is required.")]
        public int AdminId { get; set; }

        /// <summary>
        /// What type of entity is banned (e.g., user, song, collection).
        /// </summary>
        [Required(ErrorMessage = "WhatIsBanned is required.")]
        public int WhatIsBanned { get; set; }

        /// <summary>
        /// Username of the banned user (if applicable).
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Title of the banned song (if applicable).
        /// </summary>
        public int? SongId { get; internal set; }

        /// <summary>
        /// Name of the banned collection (if applicable).
        /// </summary>
        public int? CollectionId { get; internal set; }

        /// <summary>
        /// Reason for the ban.
        /// </summary>
        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; }

        /// <summary>
        /// Expiration date of the ban in UTC.
        /// </summary>
        [Required(ErrorMessage = "BanUntilUtc is required.")]
        public DateTime BanUntilUtc { get; set; }

        
    }
}
