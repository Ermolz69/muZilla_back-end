using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;
using Microsoft.EntityFrameworkCore;

namespace muZilla.Services
{
    public class ReportService
    {
        private readonly MuzillaDbContext _context;

        public ReportService(MuzillaDbContext context)
        {
            _context = context;
        }

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

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            return await _context.Reports.FindAsync(id);
        }

        /// <summary>
        /// Обновление отчёта с помощью одного и того же DTO.
        /// Здесь можно менять приоритет или статус (IsClosed).
        /// Title и Description тоже можно обновить, если нужно.
        /// </summary>
        public async Task UpdateReportAsync(int id, ReportCreateDTO dto)
        {
            var report = await _context.Reports.FindAsync(id);
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
            await _context.SaveChangesAsync();
        }

        public async Task<List<Report>> GetActiveReportsAsync()
        {
            return await _context.Reports
                .Where(r => r.IsClosed == false)
                .ToListAsync();
        }

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
