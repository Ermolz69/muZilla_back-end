using Microsoft.EntityFrameworkCore;

using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;

namespace muZilla.Services
{
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

        /// <summary>
        /// Creates a new user based on the provided data.
        /// </summary>
        /// <param name="userDTO">The DTO object containing user data for creation.</param>
        /// <returns>An asynchronous task representing the user creation process.</returns>
        /// <remarks>
        /// 1. Validates the user data using the <see cref="IsUserValid(UserDTO)"/> method.
        /// 2. Creates a new user and adds them to the database.
        /// 3. Automatically creates a "Favorites" collection for the new user.
        /// 4. Links the created collection to the user.
        /// </remarks>
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
                    PublicId = GetPublicIdUnique(),
                    TwoFactoredAuthentification = false,
                    AccessLevel = await _context.AccessLevels.FindAsync(userDTO.AccessLevelId),
                    ProfilePicture = await _context.Images.FindAsync(userDTO.ProfilePictureId)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var favoritesCollection = new Collection
                {
                    Title = "Favorites",
                    Description = "Your favorite songs",
                    ViewingAccess = 0,
                    IsFavorite = false,
                    IsBanned = false,
                    Author = user,
                    Songs = new List<Song>()
                };

                _context.Collections.Add(favoritesCollection);
                await _context.SaveChangesAsync();

                user.FavoritesCollectionId = favoritesCollection.Id;
                user.FavoritesCollection = favoritesCollection;
                await _context.SaveChangesAsync();
            }
        }

        public int GetPublicIdUnique()
        {
            Random rand = new Random();
            int id = 0;
            do
            {
                id = rand.Next(10000000, 99999999);
                
            } while (_context.Users.Select(u => u.PublicId).Contains(id));
            return id;
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
