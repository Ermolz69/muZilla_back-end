namespace muZilla.Models
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public string Genres { get; set; }
        public bool RemixesAllowed { get; set; }
        public DateTime PublishDate { get; set; }
        public int? OriginalId { get; set; }
        public bool HasExplicitLyrics { get; set; }
        public bool IsBanned { get; set; }
        public int Likes { get; set; }
        public int Views { get; set; }

        public virtual ICollection<User> Authors { get; set; }
        public virtual Song Original { get; set; }
        public virtual Image Cover { get; set; }
        public virtual ICollection<Song> Remixes { get; set; }
    }
}
