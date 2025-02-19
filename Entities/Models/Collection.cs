namespace muZilla.Entities.Models
{
    public class Collection : IModel
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
