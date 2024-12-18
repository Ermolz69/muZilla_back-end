using muZilla.Models;

namespace muZilla.DTOs
{
    public class SongAndRemixesDTO
    {
        public Song song;

        // сюда будем доавблять ремиксы когда продолжать типо серч типо експандить сонгу
        public virtual ICollection<Song> Remixes { get; set; }
    }
}
