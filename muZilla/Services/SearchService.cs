﻿using muZilla.Data;
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
        /// Searches for users by Username and Email only (Login removed from search).
        /// </summary>
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
        /// Searches for songs by Title, Genres, and HasExplicitLyrics as before.
        /// </summary>
        public async Task<List<Song>> SearchSongsAsync(string? title, string? genres, bool? hasExplicit)
        {
            var query = _context.Songs
                .Include(s => s.Authors)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(s => s.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(genres))
            {
                var requestedGenres = genres.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                            .Select(g => g.Trim())
                                            .ToList();

                query = query.Where(s =>
                    requestedGenres.All(rg => s.Genres.Contains(rg))
                );
            }

            if (hasExplicit.HasValue)
                query = query.Where(s => s.HasExplicitLyrics == hasExplicit.Value);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Searches for collections by Title and AuthorId, excluding favorite collections.
        /// </summary>
        public async Task<List<Collection>> SearchCollectionsAsync(string? title, int? authorId)
        {
            var query = _context.Collections
                .Include(c => c.Author)
                .Include(c => c.Songs)
                .Where(c => c.IsFavorite == false)  // Favorite collections
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(c => c.Title.Contains(title));

            if (authorId.HasValue)
                query = query.Where(c => c.Author.Id == authorId.Value);

            return await query.ToListAsync();
        }
    }
}
