namespace muZilla.Application.DTOs.Song
{
    public class SongSearchParametersDTO
    {
        public string? Title { get; set; }
        public string? Genres { get; set; }
        public bool? HasExplicit { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
