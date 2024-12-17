namespace muZilla.DTOs.Ban
{
    /// <summary>
    /// Data Transfer Object for Ban requests.
    /// </summary>
    public class BanRequestDTO
    {
        /// <summary>
        /// The ID of the thing to be banned.
        /// </summary>
        public int IdToBan { get; set; }

        /// <summary>
        /// The ID of the admin performing the ban.
        /// </summary>
        public int AdminId { get; set; }

        /// <summary>
        /// The reason for banning the user.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The UTC date and time until which the ban is active.
        /// </summary>
        public DateTime BanUntilUtc { get; set; }
    }

}
