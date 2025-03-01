﻿using muZilla.Entities.Enums;

namespace muZilla.Entities.Models
{
    public class Report : IModel
    {
        public int Id { get; set; }

        public string CreatorLogin { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// true — report closew, false — open.
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// Priority of task: Low, Medium, High
        /// </summary>
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
