using muZilla.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for image information.
    /// </summary>
    public class ImageDTO
    {
        /// <summary>
        /// The file path of the image.
        /// </summary>
        [Required(ErrorMessage = "ImageFilePath is required.")]
        public string ImageFilePath { get; set; }

        /// <summary>
        /// The dominant color of the image, represented as a string.
        /// </summary>
        [Required(ErrorMessage = "DomainColor is required.")]
        public string DomainColor { get; set; }
        
        /// <summary>
        /// Type of song related file.
        /// </summary>
        [Required(ErrorMessage = "FileType is required.")]
        public SongFile FileType { get; set; }
    }
}
