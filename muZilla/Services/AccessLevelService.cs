using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;


namespace muZilla.Services
{
    public class AccessLevelService
    {
        private readonly MuzillaDbContext _context;
        private readonly UserService _userService;

        public AccessLevelService(MuzillaDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
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
                CanManageReports = accessLevelDTO.CanManageReports,
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
                accessLevel.CanManageReports = accessLevelDTO.CanManageReports;
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
                CanManageReports = false,
                CanManageAL = false
            });

            return _context.AccessLevels.OrderByDescending(i => i.Id)
                .FirstOrDefault().Id;
        }

        public void EnsureThisUserIsNotNull(User? user)
        {
            if (user == null) throw new Exception($"User is null.");
        }

        public void EnsureThisUsersAccessLevelIsNotNull(User? user)
        {
            if (user.AccessLevel == null) throw new Exception($"{user.Login}'s access level is null.");
        }

        public void EnsureThisUserCanDoAnything(User? user)
        {
            EnsureThisUserIsNotNull(user);
            EnsureThisUsersAccessLevelIsNotNull(user);

            if (user.IsBanned) throw new Exception($"{user.Login} is banned and cannot do anything.");
        }

        public void EnsureThisUserCannotBanUser(User? user)
        {
            EnsureThisUserCanDoAnything(user);

            if (user.AccessLevel.CanManageAL) throw new Exception($"{user.Login} can ban users.");
            if (user.AccessLevel.CanBanUser) throw new Exception($"{user.Login} can ban users.");
        }

        public void EnsureThisUserCanBanThatUser(User? user, User userToBan)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;
            EnsureThisUserCannotBanUser(userToBan);

            if (userToBan.IsBanned) throw new Exception($"{userToBan.Login} is already banned.");
            if (user == userToBan) throw new Exception("User and user to ban are the same. Weirdo.");
            if (!user.AccessLevel.CanBanUser) throw new Exception($"{user.Login} cannot ban users.");
        }

        public void EnsureThisUserCanUnBanThatUser(User? user, User userToBan)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!userToBan.IsBanned) throw new Exception($"{userToBan.Login} is not banned.");
            if (user == userToBan) throw new Exception("User and user to ban are the same. Weirdo.");
            if (!user.AccessLevel.CanBanUser) throw new Exception($"{user.Login} cannot unban users.");
        }

        public void EnsureThisUserCanBanSong(User? user, Song song)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!song.IsBanned) throw new Exception($"{song.Title} is banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot ban songs.");
        }

        public void EnsureThisUserCanUnBanSong(User? user, Song song)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!song.IsBanned) throw new Exception($"{song.Title} is not banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot unban songs.");
        }

        public void EnsureThisUserCanBanCollection(User? user, Collection collection)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!collection.IsBanned) throw new Exception($"{collection.Title} is banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot ban collections.");
        }

        public void EnsureThisUserCanUnBanCollection(User? user, Collection collection)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!collection.IsBanned) throw new Exception($"{collection.Title} is not banned.");
            if (!user.AccessLevel.CanBanSong) throw new Exception($"{user.Login} cannot unban collections.");
        }

        public void EnsureThisUserCanManageReports(User? user)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!user.AccessLevel.CanManageReports) throw new Exception($"{user.Login} cannot manage reports.");
        }

        public void EnsureThisUserCanDownloadSongs(User? user)
        {
            EnsureThisUserCanDoAnything(user);
            if (!user.AccessLevel.CanManageAL) return;

            if (!user.AccessLevel.CanDownload) throw new Exception($"{user.Login} cannot download songs.");
        }
    }
}