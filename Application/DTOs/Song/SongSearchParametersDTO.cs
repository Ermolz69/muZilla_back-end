namespace muZilla.Application.DTOs.Song
{
    /// <summary>
    /// Data Transfer Object for searching songs.
    /// </summary>
    public class SongSearchParametersDTO
    {
        /// <summary>
        /// The title of the song to search for.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The genres associated with the song to search for.
        /// </summary>
        public string? Genres { get; set; }

        /// <summary>
        /// Indicates if only explicit songs should be included in the search results.
        /// </summary>
        public bool? HasExplicit { get; set; }

        /// <summary>
        /// The start date for the song's publish date range.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// The end date for the song's publish date range.
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
