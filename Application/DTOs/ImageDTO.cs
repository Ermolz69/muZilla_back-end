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
    }
}
