using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Models;

namespace muZilla.Services
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool ReceiveNotifications { get; set; }
        public int AccessLevelId { get; set; }
        public int ProfilePictureId { get; set; }
    }
    public class UserService
    {
        private readonly MuzillaDbContext _context;

        public UserService(MuzillaDbContext context)
        {
            _context = context;
        }

        public bool IsUserValid(UserDTO userDTO)
        {
            if (_context.Users.Select(u => u).Where(u => u.Login == userDTO.Login).Any())
                return false;

            return true;
        }

        public async Task CreateUserAsync(UserDTO userDTO)
        {
            if (IsUserValid(userDTO))
            {
                User user = new User()
                {
                    Username = userDTO.Username,
                    Email = userDTO.Email,
                    Login = userDTO.Login,
                    PhoneNumber = userDTO.PhoneNumber,
                    Password = userDTO.Password,
                    DateOfBirth = userDTO.DateOfBirth,
                    ReceiveNotifications = userDTO.ReceiveNotifications,
                    IsBanned = false,
                    TwoFactoredAuthentification = false,
                    AccessLevel = await _context.AccessLevels.FindAsync(userDTO.AccessLevelId),
                    ProfilePicture = await _context.Images.FindAsync(userDTO.ProfilePictureId)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.AccessLevel)
        .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateUserByIdAsync(int id, UserDTO userDTO)
        {
            if (IsUserValid(userDTO))
            {
                User user = await _context.Users.FindAsync(id);
                user.Username = userDTO.Username;
                user.Email = userDTO.Email;
                user.Login = userDTO.Login;
                user.PhoneNumber = userDTO.PhoneNumber;
                user.Password = userDTO.Password;
                user.DateOfBirth = userDTO.DateOfBirth;
                user.ReceiveNotifications = userDTO.ReceiveNotifications;
                user.AccessLevel = await _context.AccessLevels.FindAsync(userDTO.AccessLevelId);
                user.ProfilePicture = await _context.Images.FindAsync(userDTO.ProfilePictureId);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUserByIdAsync(int id)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user != null) _context.Users.Remove(user);
        }

        public int CanLogin(string login, string password)
        {
            User? user = _context.Users.Select(a => a).Where(a => a.Login == login).FirstOrDefault();

            if (user == null) return 0;
            if (user.Password != password) return -1;
            if (user.IsBanned) return -2;

            return 1;
        }

        public async Task<bool> BanUserByIdAsync(int idToBlock, int idOfAdmin)
        {
            User? user = await GetUserByIdAsync(idToBlock);
            User? admin = await GetUserByIdAsync(idOfAdmin);

            if (user == null || admin == null)
            {
                return false;
            }

            if ((admin.AccessLevel.CanBanUser 
                && !admin.IsBanned 
                && !user.AccessLevel.CanBanUser)
                || admin.AccessLevel.CanManageAL) 
                    user.IsBanned = true;

            else return false;

            await _context.SaveChangesAsync();
            return true;
        }

        public int GetIdByLogin(string login)
        {
            try
            {
                return _context.Users
                    .Select(a => a)
                    .Where(a => a.Login == login)
                    .FirstOrDefault().Id;
            }
            catch
            {
                return -1;
            }
        }
    }
}
