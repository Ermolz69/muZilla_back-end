namespace muZilla.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ViewingAccess { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsBanned { get; set; }

        public int Likes { get; set; } = 0;
        public int Views { get; set; } = 0;


        public virtual User Author { get; set; }
        public virtual Image Cover { get; set; }
        public virtual ICollection<Song> Songs { get; set; }
    }
}
