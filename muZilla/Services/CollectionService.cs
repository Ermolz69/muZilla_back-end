using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;

namespace muZilla.Services
{
    public class CollectionService
    {
        private readonly MuzillaDbContext _context;

        public CollectionService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateCollectionAsync(CollectionDTO collectionDTO)
        {
            var collection = new Collection
            {
                Title = collectionDTO.Title,
                Description = collectionDTO.Description,
                ViewingAccess = collectionDTO.ViewingAccess,
                IsFavorite = collectionDTO.IsFavorite,
                IsBanned = collectionDTO.IsBanned,
                Author = await _context.Users.FindAsync(collectionDTO.AuthorId)
            };

            // Если CoverId не указан, подставляем дефолтную картинку
            if (collectionDTO.CoverId == null)
            {
                // Ищем в БД дефолтную картинку по пути DEFAULT/default.png
                var defaultImage = await _context.Images.FirstOrDefaultAsync(i => i.ImageFilePath == "DEFAULT/default.png");
                if (defaultImage == null)
                {
                    // Если дефолтной картинки нет в БД, создадим её
                    defaultImage = new Image
                    {
                        ImageFilePath = "DEFAULT/default.png",
                        DomainColor = "125,125,125"
                    };
                    _context.Images.Add(defaultImage);
                    await _context.SaveChangesAsync();
                }
                collection.Cover = defaultImage;
            }
            else
            {
                collection.Cover = await _context.Images.FindAsync(collectionDTO.CoverId);
            }

            collection.Songs = new List<Song>();
            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _context.Songs.FindAsync(songId);
                if (song != null)
                    collection.Songs.Add(song);
            }

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            return collection.Id;
        }


        public async Task<Collection> GetCollectionByIdAsync(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Author)
                .Include(c => c.Cover)
                .Include(c => c.Songs)
                .FirstOrDefaultAsync(c => c.Id == id);

            return collection;
        }

        public async Task UpdateCollectionByIdAsync(int id, CollectionDTO collectionDTO)
        {
            var collection = await _context.Collections
                .Include(c => c.Songs)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (collection == null) return;

            collection.Title = collectionDTO.Title;
            collection.Description = collectionDTO.Description;
            collection.ViewingAccess = collectionDTO.ViewingAccess;
            collection.IsFavorite = collectionDTO.IsFavorite;
            collection.IsBanned = collectionDTO.IsBanned;
            collection.Author = await _context.Users.FindAsync(collectionDTO.AuthorId);
            collection.Cover = await _context.Images.FindAsync(collectionDTO.CoverId);

            // Обновляем список песен
            collection.Songs.Clear();
            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _context.Songs.FindAsync(songId);
                if (song != null)
                    collection.Songs.Add(song);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCollectionByIdAsync(int id)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection != null)
            {
                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Collection>> GetCollectionsByKeyWord(string search, bool showBanned = false)
        {
            var query = _context.Collections
                .Include(c => c.Author)
                .Include(c => c.Songs)
                .Where(c =>
                    c.Title.Contains(search)
                    || c.Description.Contains(search)
                    || c.Author.Username.Contains(search));

            if (!showBanned)
            {
                query = query.Where(c => c.IsBanned == false);
            }

            var results = await query.ToListAsync();
            return results;
        }


        public async Task LikeCollectionAsync(int userId, int collectionId)
        {
            var user = await _context.Users
                .Include(u => u.LikedCollections)
                .FirstOrDefaultAsync(u => u.Id == userId);
            var collection = await _context.Collections.FindAsync(collectionId);
            if (user == null || collection == null) return;

            if (!user.LikedCollections.Contains(collection))
            {
                user.LikedCollections.Add(collection);
                collection.Likes++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnlikeCollectionAsync(int userId, int collectionId)
        {
            var user = await _context.Users
                .Include(u => u.LikedCollections)
                .FirstOrDefaultAsync(u => u.Id == userId);
            var collection = await _context.Collections.FindAsync(collectionId);
            if (user == null || collection == null) return;

            if (user.LikedCollections.Contains(collection))
            {
                user.LikedCollections.Remove(collection);
                collection.Likes--;
                await _context.SaveChangesAsync();
            }
        }

    }
}