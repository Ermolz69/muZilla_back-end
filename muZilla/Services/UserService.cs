using Microsoft.EntityFrameworkCore;

using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;

namespace muZilla.Services
{
    public class UserService
    {
        private readonly MuzillaDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The <see cref="MuzillaDbContext"/> instance used to interact with the database.</param>
        /// <remarks>
        /// This constructor sets up the service to access the database and manage user-related operations.
        /// </remarks>
        public UserService(MuzillaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks if the provided user data is valid for creating a new user.
        /// </summary>
        /// <param name="userDTO">The DTO object containing user data to validate.</param>
        /// <returns>
        /// <c>true</c> if the user data is valid (i.e., the login does not already exist in the database); otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method verifies if the user's login is unique by checking the database.
        /// </remarks>
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

        /// <summary>
        /// Generates a unique public ID for a user.
        /// </summary>
        /// <returns>A unique 8-digit integer ID that is not already used by any user in the database.</returns>
        /// <remarks>
        /// This method uses a random number generator to create an 8-digit ID and ensures its uniqueness 
        /// by checking it against the database before returning.
        /// </remarks>
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

        /// <summary>
        /// Retrieves a user by their ID, including their access level information.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>
        /// An asynchronous task that returns the <see cref="User"/> object if found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method queries the database for a user with the specified ID and includes the related access level data.
        /// </remarks>
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.AccessLevel)
        .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Updates the details of an existing user by their ID using the provided user data.
        /// </summary>
        /// <param name="id">The unique identifier of the user to be updated.</param>
        /// <param name="userDTO">The DTO object containing updated user data.</param>
        /// <returns>An asynchronous task representing the user update operation.</returns>
        /// <remarks>
        /// 1. Validates the new user data using the <see cref="IsUserValid(UserDTO)"/> method.
        /// 2. Retrieves the user from the database using the provided ID.
        /// 3. Updates the user's details, including their username, email, login, phone number, password, date of birth, notification preferences, access level, and profile picture.
        /// 4. Saves the changes to the database.
        /// </remarks>
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

        /// <summary>
        /// Deletes a user from the database by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to be deleted.</param>
        /// <returns>An asynchronous task representing the user deletion operation.</returns>
        /// <remarks>
        /// 1. Retrieves the user from the database using the provided ID.
        /// 2. If the user exists, removes them from the database.
        /// 3. Changes are not immediately saved; ensure to call <c>_context.SaveChangesAsync()</c> after this operation to persist the changes.
        /// </remarks>
        public async Task DeleteUserByIdAsync(int id)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user != null) _context.Users.Remove(user);
        }

        /// <summary>
        /// Verifies if a user can log in with the provided credentials.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>
        /// An integer value indicating the result:
        /// <list type="bullet">
        /// <item><description><c>0</c> - User not found.</description></item>
        /// <item><description><c>-1</c> - Incorrect password.</description></item>
        /// <item><description><c>-2</c> - User is banned.</description></item>
        /// <item><description><c>1</c> - Login successful.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method retrieves the user by login, checks the password, and verifies if the user is not banned.
        /// </remarks>
        public int CanLogin(string login, string password)
        {
            User? user = _context.Users.Select(a => a).Where(a => a.Login == login).FirstOrDefault();

            if (user == null) return 0;
            if (user.Password != password) return -1;
            if (user.IsBanned) return -2;

            return 1;
        }

        /// <summary>
        /// Bans a user by their ID if the operation is authorized.
        /// </summary>
        /// <param name="idToBlock">The ID of the user to be banned.</param>
        /// <param name="idOfAdmin">The ID of the admin attempting to ban the user.</param>
        /// <returns>
        /// An asynchronous task that returns <c>true</c> if the user was successfully banned; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// 1. Retrieves both the target user and the admin from the database by their IDs.
        /// 2. Verifies the admin's permissions to ban users:
        ///    - The admin must have the "CanBanUser" permission and must not be banned.
        ///    - The target user must not have the "CanBanUser" permission unless overridden by the admin's "CanManageAL" permission.
        /// 3. If the conditions are met, bans the user and saves the changes to the database.
        /// </remarks>
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

        /// <summary>
        /// Retrieves the ID of a user based on their login.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <returns>
        /// The ID of the user if found; otherwise, <c>-1</c> if the user does not exist or an error occurs.
        /// </returns>
        /// <remarks>
        /// This method attempts to find a user with the specified login and returns their ID. 
        /// If the user is not found or an exception occurs, it returns <c>-1</c>.
        /// </remarks>
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
