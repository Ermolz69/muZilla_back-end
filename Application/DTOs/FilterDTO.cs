namespace muZilla.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for filtering song collections.
    /// </summary>
    public class FilterDTO
    {
        /// <summary>
        /// The genres to filter the songs by.
        /// </summary>
        public string? Genres { get; set; }

        /// <summary>
        /// Indicates whether to include remixes in the results.
        /// </summary>
        public bool? Remixes { get; set; }

        /// <summary>
        /// Indicates whether to show banned items in the results.
        /// Default is false.
        /// </summary>
        public bool ShowBanned { get; set; } = false;
    }
}

