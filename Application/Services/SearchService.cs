using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;

using muZilla.Application.DTOs;
using muZilla.Application.Interfaces;

namespace muZilla.Application.Services
{
    public class SearchService
    {
        private readonly IGenericRepository _repository;

        public SearchService(IGenericRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Searches for users based on their username, email address, or publicId, excluding duplicates.
        /// </summary>
        /// <param name="username">Part of (or entire) username to search for (optional).</param>
        /// <param name="email">Part of (or entire) email address to search for (optional).</param>
        /// <param name="publicId">Exact publicId to search for (optional).</param>
        /// <returns>A list of unique users that match any of the search criteria.</returns>
        public async Task<List<User>> SearchUsersAsync(string? username, string? email, int? publicId)
        {
            var query = await _repository.GetAllAsync<User>();

            var users = new HashSet<User>();

            if (!string.IsNullOrWhiteSpace(username))
                users.UnionWith(query.Where(u => u.Username.Contains(username)));

            if (!string.IsNullOrWhiteSpace(email))
                users.UnionWith(query.Where(u => u.Email.Contains(email)));

            if (publicId.HasValue)
                users.UnionWith(query.Where(u => u.PublicId == publicId.Value));

            return users.ToList();
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
            DateTime? toDate
            )
        {
            return await _repository.GetAllAsync<Song>().Result
                .Include(s => s.Authors)
                .Include(s => s.Original)
                .Include(s => s.Cover)
                .Where(s => string.IsNullOrWhiteSpace(title) || EF.Functions.Like(s.Title, $"%{title}%"))
                .Where(s => string.IsNullOrWhiteSpace(genres) ||
                            genres.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                  .Select(g => g.Trim())
                                  .Any(genre => EF.Functions.Like(s.Genres, $"%{genre}%")))
                .Where(s => !hasExplicit.HasValue || s.HasExplicitLyrics == hasExplicit.Value)
                .Where(s => !fromDate.HasValue || s.PublishDate >= fromDate.Value)
                .Where(s => !toDate.HasValue || s.PublishDate <= toDate.Value)
                .OrderByDescending(s => s.PublishDate)
                .ToListAsync();
        }


        /// <summary>
        /// Searches for collections based on their title and/or author ID.
        /// </summary>
        /// <param name="title">The title to search for (optional).</param>
        /// <param name="authorId">The unique identifier of the author to filter by (optional).</param>
        /// <returns>A list of collections that match the search criteria.</returns>
        public async Task<List<Collection>> SearchCollectionsAsync(string? title, int? authorId)
        {
            return await _repository.GetAllAsync<Collection>().Result
                .Include(c => c.Author)
                .Include(c => c.Songs)
                .Where(c => !c.IsFavorite) // False
                .Where(c => string.IsNullOrWhiteSpace(title) || c.Title.Contains(title))
                .Where(c => !authorId.HasValue || c.Author.Id == authorId.Value)
                .ToListAsync();
        }

    }
}
