using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs
{
    public class AccessLevelDTO
    {
        [Required]
        public bool CanBanUser { get; set; }

        [Required]
        public bool CanBanSong { get; set; }

        [Required]
        public bool CanDownload { get; set; }

        [Required]
        public bool CanUpload { get; set; }

        [Required]
        public bool CanReport { get; set; }

        [Required]
        public bool CanManageReports { get; set; }
        [Required]
        public bool CanManageSupports { get; set; }

        [Required]
        public bool CanManageAL { get; set; }
    }
}
