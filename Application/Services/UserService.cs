using Microsoft.EntityFrameworkCore;

using System.Net.Mail;
using System.Net;

using muZilla.Entities.Models;
using muZilla.Entities.Enums;

using muZilla.Application.DTOs;
using muZilla.Application.DTOs.User;
using muZilla.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace muZilla.Application.Services
{
    public class UserService
    {
        private readonly IGenericRepository _repository;
        private readonly CollectionService _collectionService;
        private readonly IConfiguration _config;

        public UserService(IGenericRepository repository, IConfiguration config, CollectionService collectionService)
        {
            _repository = repository;
            _config = config;
            _collectionService = collectionService;
        }

        /// <summary>
        /// Checks if the provided user data is valid for creating a new user.
        /// </summary>
        /// <param name="UserDTO">The DTO object containing user data to validate.</param>
        /// <returns>
        /// <c>true</c> if the user data is valid (i.e., the login does not already exist in the database); otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method verifies if the user's login is unique by checking the database.
        /// </remarks>
        public bool IsUserValid(LoginDTO LoginDTO)
        {
            if (_repository.GetAllAsync<User>().Result.Select(u => u).Where(u => u.Login == LoginDTO.Login).Any())
                return false;

            return true;
        }
        public bool IsUserValid(uint id)
        {
            if (_repository.GetAllAsync<User>().Result.Select(u => u).Where(u => u.Id == id).Any())
                return false;

            return true;
        }

        /// <summary>
        /// Creates a new user based on the provided data.
        /// </summary>
        /// <param name="UserDTO">The DTO object containing user data for creation.</param>
        /// <returns>An asynchronous task representing the user creation process.</returns>
        /// <remarks>
        /// 1. Validates the user data using the <see cref="IsUserValid(UserDTO)"/> method.
        /// 2. Creates a new user and adds them to the database.
        /// 3. Automatically creates a "Favorites" collection for the new user.
        /// 4. Links the created collection to the user.
        /// </remarks>
        public async Task CreateUserAsync(RegisterDTO registerDTO)
        {
            if (IsUserValid(registerDTO.LoginDTO))
            {
                User user = new User()
                {
                    Username = registerDTO.UserDTO.UserPublicData.Username,
                    Email = registerDTO.UserDTO.Email,
                    Login = registerDTO.LoginDTO.Login,
                    PhoneNumber = registerDTO.UserDTO.PhoneNumber,
                    Password = registerDTO.LoginDTO.Password,
                    DateOfBirth = registerDTO.UserDTO.DateOfBirth,
                    ReceiveNotifications = registerDTO.UserDTO.ReceiveNotifications,
                    IsBanned = false,
                    PublicId = GetPublicIdUnique(),
                    TwoFactoredAuthentification = false,
                    AccessLevel = (await _repository.GetByIdAsync<AccessLevel>(registerDTO.UserDTO.UserPublicData.AccessLevelId))!,
                    ProfilePicture = (await _repository.GetByIdAsync<Image>(registerDTO.UserDTO.UserPublicData.ProfilePictureId))!
                };

                await _repository.AddAsync<User>(user);
                await _repository.SaveChangesAsync();

                int collectionId = await _collectionService.CreateCollectionAsync(new CollectionDTO()
                {
                    Title = "Favourite Collection",
                    Description = "Favourite",
                    ViewingAccess = 0,
                    IsBanned = false,
                    IsFavorite = true,
                    AuthorId = await GetIdByLoginAsync(user.Login),
                    CoverId = null,
                    SongIds = new List<int>()
                });

                user.FavoritesCollectionId = collectionId;
                user.FavoritesCollection = (await _collectionService.GetCollectionByIdAsync(collectionId))!;
                await _repository.SaveChangesAsync();
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
                
            } while (_repository.GetAllAsync<User>().Result.Select(u => u.PublicId).Contains(id));
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
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Updates the details of an existing user by their ID using the provided user data.
        /// </summary>
        /// <param name="id">The unique identifier of the user to be updated.</param>
        /// <param name="UserDTO">The DTO object containing updated user data.</param>
        /// <returns>An asynchronous task representing the user update operation.</returns>
        /// <remarks>
        /// 1. Validates the new user data using the <see cref="IsUserValid(UserDTO)"/> method.
        /// 2. Retrieves the user from the database using the provided ID.
        /// 3. Updates the user's details, including their username, email, login, phone number, password, date of birth, notification preferences, access level, and profile picture.
        /// 4. Saves the changes to the database.
        /// </remarks>
        public async Task UpdateUserByIdAsync(int id, RegisterDTO registerDTO)
        {
            if (IsUserValid(registerDTO.LoginDTO))
            {
                User user = (await _repository.GetByIdAsync<User>(id))!;
                user.Username = registerDTO.UserDTO.UserPublicData.Username;
                user.Email = registerDTO.UserDTO.Email;
                user.Login = registerDTO.LoginDTO.Login;
                user.PhoneNumber = registerDTO.UserDTO.PhoneNumber;
                user.Password = registerDTO.LoginDTO.Password;
                user.DateOfBirth = registerDTO.UserDTO.DateOfBirth;
                user.ReceiveNotifications = registerDTO.UserDTO.ReceiveNotifications;
                user.AccessLevel = (await _repository.GetByIdAsync<AccessLevel>(registerDTO.UserDTO.UserPublicData.AccessLevelId))!;
                user.ProfilePicture = (await _repository.GetByIdAsync<Image>(registerDTO.UserDTO.UserPublicData.ProfilePictureId))!;

                await _repository.SaveChangesAsync();
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
        /// 3. Changes are not immediately saved; ensure to call <c>_repository.SaveChangesAsync()</c> after this operation to persist the changes.
        /// </remarks>
        public async Task DeleteUserByIdAsync(int id)
        {
            User? user = await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .Include(u => u.FavoritesCollection)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return;

            await _repository.RemoveAsync<Collection>(user.FavoritesCollection);
            await _repository.SaveChangesAsync();

            await _repository.RemoveAsync<User>(user);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Verifies if a user can log in with the provided credentials.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>
        /// An integer value indicating the result:
        /// <list type="bullet">
        /// <item><description><c>LoginResultType.NotFound</c> - User not found.</description></item>
        /// <item><description><c>LoginResultType.IncorrectData</c> - Incorrect password.</description></item>
        /// <item><description><c>LoginResultType.Banned</c> - User is banned.</description></item>
        /// <item><description><c>LoginResultType.Success</c> - Login successful.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method retrieves the user by login, checks the password, and verifies if the user is not banned.
        /// </remarks>
        public LoginResultType CanLogin(LoginDTO LoginDTO)
        {
            User? user = _repository.GetAllAsync<User>().Result.Select(a => a).Where(a => a.Login == LoginDTO.Login || a.Email == LoginDTO.Login).FirstOrDefault();

            if (user == null) return LoginResultType.NotFound;
            if (user.Password != LoginDTO.Password) return LoginResultType.IncorrectData;
            if (user.IsBanned) return LoginResultType.Banned;

            return LoginResultType.Success;
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
        public async Task<int> GetIdByLoginAsync(string login)
        {
           var user = await GetUserByLoginAsync(login);
           return user == null ? -1 : user.Id;
        }

        /// <summary>
        /// Retrieves a user by their login.
        /// </summary>
        /// <param name="login">The login of the user to retrieve.</param>
        /// <returns>
        /// The user object if found, including their access level, or null if no matching user is found.
        /// </returns>
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            return await _repository.GetAllAsync<User>().Result
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Login == login);
        }

        /// <summary>
        /// Sends a welcome email to the specified recipient using Gmail's SMTP server.
        /// </summary>
        /// <param name="login">The username of the recipient, included in the welcome message.</param>
        /// <param name="email">The recipient's email address to which the welcome email is sent.</param>
        /// <remarks>
        /// Configures Gmail's SMTP server with SSL enabled for secure email transmission.
        /// </remarks>
        public void SendEmail(string login, string email)
        {
            try
            {
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string senderEmail = _config["EmailSMTP:Email"]!;
                string senderPassword = _config["EmailSMTP:Password"]!;

                string recipientEmail = email;
                string subject = "Welcome to MUZILLA q(≧▽≦q)";
                string body = $"Welcome aboard(/≧▽≦)/, {login}! You have registered in our website.\n\nLet's work together!";

                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(recipientEmail);

                smtpClient.Send(mailMessage);

                Console.WriteLine("post sent success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error on sending: {ex.Message}");
            }
        }
    }
}
