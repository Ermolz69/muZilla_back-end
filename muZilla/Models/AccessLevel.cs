﻿using Microsoft.Identity.Client;

namespace muZilla.Models
{
    public class AccessLevel
    {
        public int Id { get; set; }
        public bool CanBanUser { get; set; }
        public bool CanBanSong { get; set; }
        public bool CanDownload { get; set; }
        public bool CanUpload { get; set; }
        public bool CanReport { get; set; }
        public bool CanManageReports { get; set; }
        public bool CanManageSupports { get; set; }
        public bool CanManageAL { get; set; }
    }
}
