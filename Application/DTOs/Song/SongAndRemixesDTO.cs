using muZilla.Entities.Models;

namespace muZilla.Application.DTOs.Song
{
    public class SongAndRemixesDTO
    {
        public muZilla.Entities.Models.Song song;

        // сюда будем доавблять ремиксы когда продолжать типо серч типо експандить сонгу
        public virtual ICollection<muZilla.Entities.Models.Song> Remixes { get; set; }
    }
}
