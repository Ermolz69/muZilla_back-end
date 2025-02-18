namespace muZilla.Application.DTOs
{
    public class CollectionDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ViewingAccess { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsBanned { get; set; }
        public int AuthorId { get; set; }
        public int? CoverId { get; set; }
        public List<int> SongIds { get; set; } = new List<int>();
    }
}
