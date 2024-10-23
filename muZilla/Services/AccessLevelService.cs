using Microsoft.AspNetCore.Mvc;
using muZilla.Data;
using muZilla.Models;
using System.ComponentModel.DataAnnotations;


namespace muZilla.Services
{
    public class AccessLevelDTO
    {
        [Required]
        public bool CanBanUser { get; set; }

        [Required]
        public bool CanBanSong { get; set; }

        [Required]
        public bool CanDownload { get; set; }

        [Required]
        public bool CanUpload { get; set; }

        [Required]
        public bool CanReport { get; set; }

        [Required]
        public bool CanManageAL { get; set; }
    }

    public class AccessLevelService
    {
        private readonly MuzillaDbContext _context;

        public AccessLevelService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task CreateAccessLevelAsync(AccessLevelDTO accessLevelDTO)
        {
            var accessLevel = new AccessLevel()
            {
                CanBanUser = accessLevelDTO.CanBanUser,
                CanBanSong = accessLevelDTO.CanBanSong,
                CanDownload = accessLevelDTO.CanDownload,
                CanManageAL = accessLevelDTO.CanManageAL,
                CanReport = accessLevelDTO.CanReport,
                CanUpload = accessLevelDTO.CanUpload
            };

            _context.AccessLevels.Add(accessLevel);
            await _context.SaveChangesAsync();
        }

        public async Task<AccessLevel?> GetAccessLevelById(int id)
        {
            var accessLevel = await _context.AccessLevels.FindAsync(id);

            return accessLevel;
        }

        public async Task UpdateAccessLevelByIdAsync(int id, AccessLevelDTO accessLevelDTO)
        {
            var accessLevel = await _context.AccessLevels.FindAsync(id);
            if (accessLevel != null)
            {
                accessLevel.CanBanUser = accessLevelDTO.CanBanUser;
                accessLevel.CanBanSong = accessLevelDTO.CanBanSong;
                accessLevel.CanDownload = accessLevelDTO.CanDownload;
                accessLevel.CanUpload = accessLevelDTO.CanUpload;
                accessLevel.CanReport = accessLevelDTO.CanReport;
                accessLevel.CanManageAL = accessLevelDTO.CanManageAL;

                _context.AccessLevels.Update(accessLevel);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAccessLevelByIdAsync(int id)
        {
            var accessLevel = await _context.AccessLevels.FindAsync(id);
            if (accessLevel != null)
            {
                _context.AccessLevels.Remove(accessLevel);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CreateDefaultAccessLevelAsync()
        {
            await CreateAccessLevelAsync(new AccessLevelDTO
            {
                CanBanUser = false,
                CanBanSong = false,
                CanDownload = false,
                CanUpload = true,
                CanReport = true,
                CanManageAL = false
            });

            return _context.AccessLevels.OrderByDescending(i => i.Id)
                .FirstOrDefault().Id;
        }
    }
}