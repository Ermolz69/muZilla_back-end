namespace muZilla.Application.DTOs
{
    public class FilterDTO
    {
        public string? Genres { get; set; }
        public bool? Remixes { get; set; }
        public bool ShowBanned { get; set; } = false;
    }
}
