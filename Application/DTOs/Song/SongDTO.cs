using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs.Song
{
    /// <summary>
    /// Data Transfer Object for song information.
    /// </summary>
    public class SongDTO
    {
        /// <summary>
        /// The title of the song.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; }

        /// <summary>
        /// The description of the song.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        /// <summary>
        /// The length of the song in seconds.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Length must be greater than zero.")]
        public int Length { get; set; }

        /// <summary>
        /// The genres associated with the song.
        /// </summary>
        [Required(ErrorMessage = "Genres are required.")]
        public string Genres { get; set; }

        /// <summary>
        /// Indicates if remixes are allowed for the song.
        /// </summary>
        [Required(ErrorMessage = "RemixesAllowed are required.")]
        public bool RemixesAllowed { get; set; }

        /// <summary>
        /// The publish date of the song.
        /// </summary>
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The ID of the original song if this is a remix.
        /// </summary>
        public int? OriginalId { get; set; }

        /// <summary>
        /// Indicates whether the song contains explicit lyrics.
        /// </summary>
        [Required(ErrorMessage = "HasExplicitLyrics are required.")]
        public bool HasExplicitLyrics { get; set; }

        /// <summary>
        /// The ID of the cover image for the song.
        /// </summary>
        public int? ImageId { get; set; }

        /// <summary>
        /// A list of author IDs associated with the song.
        /// </summary>
        [Required(ErrorMessage = "At least one author is required.")]
        public List<int> AuthorIds { get; set; } = new();
    }
}
