namespace muZilla.Application.DTOs.Ban
{
    /// <summary>
    /// Data Transfer Object for Ban information.
    /// </summary>
    public class BanDTO
    {
        public string BannedByUsername { get; set; }
        public int WhatIsBanned { get; set; }
        public string? BannedUsername { get; set; }
        public string Reason { get; set; }
        public DateTime BanUntilUtc { get; set; }
        public string? BannedSongTitle { get; internal set; }
        public string? BannedCollectionName { get; internal set; }
    }
}
