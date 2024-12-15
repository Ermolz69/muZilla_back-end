namespace muZilla.DTOs.Ban
{
    /// <summary>
    /// Data Transfer Object for Ban information.
    /// </summary>
    public class BanDTO
    {
        public string BannedByUsername { get; set; }
        public string BannedUsername { get; set; }
        public string Reason { get; set; }
        public DateTime BanUntilUtc { get; set; }
    }
}
