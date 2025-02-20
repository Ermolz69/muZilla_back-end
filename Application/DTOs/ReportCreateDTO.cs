using System.ComponentModel.DataAnnotations;

namespace muZilla.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a report.
    /// </summary>
    public class ReportCreateDTO
    {
        /// <summary>
        /// The title of the report.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        /// <summary>
        /// The description of the report.
        /// </summary>
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        /// <summary>
        /// The priority of the report, default is "Medium".
        /// </summary>
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Indicates whether the report is closed.
        /// </summary>
        public bool? IsClosed { get; set; }
    }
}

