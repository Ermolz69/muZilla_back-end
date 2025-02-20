using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a user's access level permissions.
    /// </summary>
    public class AccessLevelDTO
    {
        /// <summary>
        /// Determines if the user can ban other users.
        /// </summary>
        public bool CanBanUser { get; set; }

        /// <summary>
        /// Determines if the user can ban songs.
        /// </summary>
        public bool CanBanSong { get; set; }

        /// <summary>
        /// Determines if the user can download content.
        /// </summary>
        public bool CanDownload { get; set; }

        /// <summary>
        /// Determines if the user can upload content.
        /// </summary>
        public bool CanUpload { get; set; }

        /// <summary>
        /// Determines if the user can report inappropriate content.
        /// </summary>
        public bool CanReport { get; set; }

        /// <summary>
        /// Determines if the user can manage reported content.
        /// </summary>
        public bool CanManageReports { get; set; }

        /// <summary>
        /// Determines if the user can manage support requests.
        /// </summary>
        public bool CanManageSupports { get; set; }

        /// <summary>
        /// Determines if the user can manage access levels.
        /// </summary>
        public bool CanManageAL { get; set; }
    }
}
