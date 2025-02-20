using muZilla.Application.DTOs.Song;

namespace muZilla.ResponseRequestModels
{
    public class PublishSongRequest
    {
        public IFormFile Song { get; set; } = null!;
        public IFormFile? Image { get; set; }
        public IFormFile? Lyrics { get; set; }
        public SongDTO SongDTO { get; set; } = null!;
    }

}
