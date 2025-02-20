using muZilla.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Song
{
    /// <summary>
    /// Data Transfer Object for a song and its remixes.
    /// </summary>
    public class SongAndRemixesDTO
    {
        /// <summary>
        /// The original song.
        /// </summary>
        [Required(ErrorMessage = "Song is required.")]
        public Entities.Models.Song Song { get; set; }

        /// <summary>
        /// A collection of remixes for the song.
        /// </summary>
        public ICollection<Entities.Models.Song> Remixes { get; set; } = new List<Entities.Models.Song>();
    }
}
