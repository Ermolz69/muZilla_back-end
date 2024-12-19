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

        /// <summary>
        /// Creates a new collection based on the provided data transfer object (DTO).
        /// </summary>
        /// <param name="collectionDTO">The data transfer object containing collection details.</param>
        /// <returns>The unique identifier of the newly created collection.</returns>
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

            if (collectionDTO.CoverId == null)
            {

                var defaultImage = await _context.Images.FirstOrDefaultAsync(i => i.ImageFilePath == "DEFAULT/default.jpg");
                if (defaultImage == null)
                {
                    defaultImage = new Image
                    {
                        ImageFilePath = "DEFAULT/default.jpg",
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


        /// <summary>
        /// Retrieves a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection.</param>
        /// <returns>The collection details, including its author, cover, and associated songs.</returns>
        public async Task<Collection> GetCollectionByIdAsync(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Author)
                .Include(c => c.Cover)
                .Include(c => c.Songs)
                .FirstOrDefaultAsync(c => c.Id == id);

            return collection;
        }

        /// <summary>
        /// Updates an existing collection based on the provided data transfer object (DTO).
        /// </summary>
        /// <param name="id">The unique identifier of the collection to update.</param>
        /// <param name="collectionDTO">The data transfer object containing updated collection details.</param>
        /// <returns>An asynchronous task representing the update operation.</returns>
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

            collection.Songs.Clear();
            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _context.Songs.FindAsync(songId);
                if (song != null)
                    collection.Songs.Add(song);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteCollectionByIdAsync(int id)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection != null)
            {
                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Toggles the like status of a collection for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user performing the toggle operation.</param>
        /// <param name="collectionId">The ID of the collection to toggle the like status for.</param>
        /// <returns>An asynchronous task representing the toggle operation.</returns>
        public async Task ToggleLikeCollectionAsync(int userId, int collectionId)
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
            else
            {
                user.LikedCollections.Remove(collection);
                collection.Likes--;
                await _context.SaveChangesAsync();
            }
        }
    }
}