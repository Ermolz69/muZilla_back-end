using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;
using muZilla.Entities.Enums;

using muZilla.Application.DTOs;
using muZilla.Application.Interfaces;

namespace muZilla.Application.Services
{
    public class ReportService
    {
        private readonly IGenericRepository _repository;

        public ReportService(IGenericRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Creates a new report with the specified details.
        /// </summary>
        /// <param name="creatorLogin">The login of the user creating the report.</param>
        /// <param name="dto">The data transfer object containing report details.</param>
        /// <returns>The newly created report object.</returns>
        public async Task<Report> CreateReportAsync(string creatorLogin, ReportCreateDTO dto)
        {
            var priority = ParsePriority(dto.Priority);

            var report = new Report
            {
                CreatorLogin = creatorLogin,
                Title = dto.Title,
                Description = dto.Description,
                Priority = priority
            };

            await _repository.AddAsync<Report>(report);
            await _repository.SaveChangesAsync();
            return report;
        }

        /// <summary>
        /// Retrieves a report by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the report.</param>
        /// <returns>The report object if found, otherwise null.</returns>
        public async Task<Report?> GetReportByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<Report>(id);
        }

        /// <summary>
        /// Updates an existing report with new details.
        /// </summary>
        /// <param name="id">The unique identifier of the report to update.</param>
        /// <param name="dto">The data transfer object containing updated report details.</param>
        /// <returns>An asynchronous task representing the update operation.</returns>
        public async Task UpdateReportAsync(int id, ReportCreateDTO dto)
        {
            var report = await _repository.GetByIdAsync<Report>(id);
            if (report == null) return;

            if (!string.IsNullOrEmpty(dto.Title))
                report.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description))
                report.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Priority))
            {
                var priority = ParsePriority(dto.Priority);
                report.Priority = priority;
            }

            if (dto.IsClosed.HasValue)
            {
                report.IsClosed = dto.IsClosed.Value;
            }

            report.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all active reports that are not closed.
        /// </summary>
        /// <returns>A list of active reports.</returns>
        public async Task<List<Report>> GetActiveReportsAsync()
        {
            return await _repository.GetAllAsync<Report>().Result
                .Where(r => r.IsClosed == false)
                .ToListAsync();
        }

        /// <summary>
        /// Converts a priority string into a <see cref="PriorityLevel"/> enumeration value.
        /// </summary>
        /// <param name="priorityStr">The string representation of the priority level.</param>
        /// <returns>The corresponding <see cref="PriorityLevel"/> enumeration value.</returns>
        private PriorityLevel ParsePriority(string priorityStr)
        {
            return priorityStr.ToLower() switch
            {
                "low" => PriorityLevel.Low,
                "high" => PriorityLevel.High,
                _ => PriorityLevel.Medium
            };
        }
    }
}
