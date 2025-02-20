using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for a collection of songs.
    /// </summary>
    public class CollectionDTO
    {
        /// <summary>
        /// The title of the collection.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        /// <summary>
        /// A description of the collection.
        /// </summary>
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        /// <summary>
        /// The viewing access level for the collection.
        /// </summary>
        [Required(ErrorMessage = "ViewingAccess is required.")]
        public int ViewingAccess { get; set; }

        /// <summary>
        /// Indicates whether the collection is marked as favorite.
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Indicates whether the collection is banned.
        /// </summary>
        public bool IsBanned { get; set; }

        /// <summary>
        /// The ID of the author who created the collection.
        /// </summary>
        [Required(ErrorMessage = "AuthorId  is required.")]
        public int AuthorId { get; set; }

        /// <summary>
        /// The ID of the cover image for the collection, if available.
        /// </summary>
        public int? CoverId { get; set; }

        /// <summary>
        /// A list of song IDs that are part of the collection.
        /// </summary>
        [Required(ErrorMessage = "SongIds is required.")]
        public List<int> SongIds { get; set; } = new List<int>();
    }
}


