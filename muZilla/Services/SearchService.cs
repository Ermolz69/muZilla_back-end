using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;
using Microsoft.EntityFrameworkCore;

namespace muZilla.Services
{
    public class SearchService
    {
        private readonly MuzillaDbContext _context;

        public SearchService(MuzillaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Searches for users based on their username and/or email address.
        /// </summary>
        /// <param name="username">The username to search for (optional).</param>
        /// <param name="email">The email address to search for (optional).</param>
        /// <returns>A list of users that match the search criteria.</returns>
        public async Task<List<User>> SearchUsersAsync(string? username, string? email)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(u => u.Username.Contains(username));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email.Contains(email));

            return await query.ToListAsync();
        }

        /// <summary>
        /// Searches for songs based on title, genres, explicit content, and date range.
        /// </summary>
        /// <param name="title">The title to search for (optional).</param>
        /// <param name="genres">A comma-separated list of genres to search for (optional).</param>
        /// <param name="hasExplicit">A flag indicating whether to filter by explicit content (optional).</param>
        /// <param name="fromDate">The earliest publish date to include (optional).</param>
        /// <param name="toDate">The latest publish date to include (optional).</param>
        /// <returns>A list of songs that match the search criteria.</returns>
        public async Task<List<Song>> SearchSongsAsync(
            string? title,
            string? genres,
            bool? hasExplicit,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Songs
                .Include(s => s.Authors)
                .Include(s => s.Original)
                .Include(s => s.Cover)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(s => EF.Functions.Like(s.Title, $"%{title}%"));

            if (!string.IsNullOrWhiteSpace(genres))
            {
                var requestedGenres = genres.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                            .Select(g => g.Trim())
                                            .ToList();


                foreach (var genre in requestedGenres)
                {
                    var genrePattern = $"%{genre}%";
                    query = query.Where(s => EF.Functions.Like(s.Genres, genrePattern));
                }
            }

            if (hasExplicit.HasValue)
                query = query.Where(s => s.HasExplicitLyrics == hasExplicit.Value);

            if (fromDate.HasValue)
                query = query.Where(s => s.PublishDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.PublishDate <= toDate.Value);

            query = query.OrderByDescending(s => s.PublishDate);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Searches for collections based on their title and/or author ID.
        /// </summary>
        /// <param name="title">The title to search for (optional).</param>
        /// <param name="authorId">The unique identifier of the author to filter by (optional).</param>
        /// <returns>A list of collections that match the search criteria.</returns>
        public async Task<List<Collection>> SearchCollectionsAsync(string? title, int? authorId)
        {
            var query = _context.Collections
                .Include(c => c.Author)
                .Include(c => c.Songs)
                .Where(c => c.IsFavorite == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(c => c.Title.Contains(title));

            if (authorId.HasValue)
                query = query.Where(c => c.Author.Id == authorId.Value);

            return await query.ToListAsync();
        }
    }
}
