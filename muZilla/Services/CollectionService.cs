using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace muZilla.Services
{
    public class CollectionDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ViewingAccess { get; set; }
        public int? CoverId { get; set; }
        public List<int> SongIds { get; set; }
    }

    public class CollectionService
    {
        private readonly MuzillaDbContext _context;

        public CollectionService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateCollectionAsync(CollectionDTO collectionDTO, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return -1;

            var collection = new Collection()
            {
                Title = collectionDTO.Title,
                Description = collectionDTO.Description,
                ViewingAccess = collectionDTO.ViewingAccess,
                IsFavorite = false,
                IsBanned = false,
                Author = user,
                Cover = collectionDTO.CoverId.HasValue ? await _context.Images.FindAsync(collectionDTO.CoverId.Value) : null,
                Songs = new List<Song>()
            };

            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _context.Songs.FindAsync(songId);
                if (song != null) collection.Songs.Add(song);
            }

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();
            return collection.Id;
        }

        public async Task<Collection> GetCollectionByIdAsync(int id)
        {
            return await _context.Collections
                .Include(c => c.Songs)
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateCollectionAsync(int id, CollectionDTO collectionDTO)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null) return;

            collection.Title = collectionDTO.Title;
            collection.Description = collectionDTO.Description;
            collection.ViewingAccess = collectionDTO.ViewingAccess;
            collection.Cover = collectionDTO.CoverId.HasValue ? await _context.Images.FindAsync(collectionDTO.CoverId.Value) : null;

            collection.Songs.Clear();
            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _context.Songs.FindAsync(songId);
                if (song != null) collection.Songs.Add(song);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCollectionAsync(int id)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection != null)
            {
                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateFavoritesAsync(int userId, int songId, bool like)
        {
            var song = await _context.Songs.FindAsync(songId);
            if (song == null) return;

            var favoriteCollection = await _context.Collections
                .Include(c => c.Songs)
                .FirstOrDefaultAsync(c => c.IsFavorite && c.Author.Id == userId);

            if (favoriteCollection == null)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return;

                favoriteCollection = new Collection()
                {
                    Title = "Favorites",
                    Description = "Automatically generated favorites playlist",
                    ViewingAccess = 0,
                    IsFavorite = true,
                    Author = user,
                    Songs = new List<Song>(),
                    Cover = song.Cover
                };
                _context.Collections.Add(favoriteCollection);
            }

            if (like)
            {
                if (!favoriteCollection.Songs.Contains(song))
                {
                    favoriteCollection.Songs.Add(song);
                }
            }
            else
            {
                if (favoriteCollection.Songs.Contains(song))
                {
                    favoriteCollection.Songs.Remove(song);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}