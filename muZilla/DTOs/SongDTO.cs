namespace muZilla.DTOs
{
    public class SongDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Length { get; set; }
        public string Genres { get; set; }
        public bool RemixesAllowed { get; set; }
        public DateTime PublishDate { get; set; }
        public int? OriginalId { get; set; }
        public bool HasExplicitLyrics { get; set; }
        public int? ImageId { get; set; }
        public List<int> AuthorIds { get; set; }
    }
}
