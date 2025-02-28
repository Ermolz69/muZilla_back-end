using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;

using muZilla.Application.DTOs;
using muZilla.Application.Interfaces;

namespace muZilla.Application.Services
{
    public class CollectionService
    {
        private readonly IGenericRepository _repository;

        public CollectionService(IGenericRepository repository)
        {
            _repository = repository;
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
                Author = await _repository.GetByIdAsync<User>(collectionDTO.AuthorId)
            };

            if (collectionDTO.CoverId == null)
            {

                var defaultImage = await _repository.GetAllAsync<Image>().Result
                    .FirstOrDefaultAsync(i => i.ImageFilePath == "DEFAULT/default.jpg");

                if (defaultImage == null)
                {
                    defaultImage = new Image
                    {
                        ImageFilePath = "DEFAULT/default.jpg",
                        DomainColor = "125,125,125"
                    };
                    await _repository.AddAsync<Image>(defaultImage);
                    await _repository.SaveChangesAsync();
                }
                collection.Cover = defaultImage;
            }
            else if (collectionDTO.CoverId.HasValue)
            { 
                #pragma warning disable CS8601 // Possible null reference assignment.
                collection.Cover = await _repository.GetByIdAsync<Image>(collectionDTO.CoverId.Value);
                #pragma warning restore CS8601 // Possible null reference assignment.
            }

            collection.Songs = new List<Song>();
            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _repository.GetByIdAsync<Song>(songId);
                if (song != null)
                    collection.Songs.Add(song);
            }

            await _repository.AddAsync<Collection>(collection);
            await _repository.SaveChangesAsync();

            return collection.Id;
        }


        /// <summary>
        /// Retrieves a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection.</param>
        /// <returns>The collection details, including its author, cover, and associated songs.</returns>
        public async Task<Collection?> GetCollectionByIdAsync(int id)
        {
            var collection = await _repository.GetAllAsync<Collection>().Result
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
        public async Task<bool> UpdateCollectionByIdAsync(int id, CollectionDTO collectionDTO)
        {
            var collection = await GetCollectionByIdAsync(id);
            if (collection == null) return false;

            collection.Title = collectionDTO.Title;
            collection.Description = collectionDTO.Description;
            collection.ViewingAccess = collectionDTO.ViewingAccess;
            collection.IsFavorite = collectionDTO.IsFavorite;
            collection.IsBanned = collectionDTO.IsBanned;

            #pragma warning disable CS8601 // Possible null reference assignment.
            collection.Author = await _repository.GetByIdAsync<User>(collectionDTO.AuthorId);
            collection.Cover = await _repository.GetByIdAsync<Image>(collectionDTO.CoverId.Value);
            #pragma warning restore CS8601 // Possible null reference assignment.

            collection.Songs.Clear();
            foreach (var songId in collectionDTO.SongIds)
            {
                var song = await _repository.GetByIdAsync<Song>(songId);
                if (song != null)
                    collection.Songs.Add(song);
            }

            await _repository.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a collection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the collection to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteCollectionByIdAsync(int id)
        {
            var collection = await _repository.GetByIdAsync<Collection>(id);
            if (collection != null)
            {
                await _repository.RemoveAsync<Collection>(collection);
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Toggles the like status of a collection for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user performing the toggle operation.</param>
        /// <param name="collectionId">The ID of the collection to toggle the like status for.</param>
        /// <returns>An asynchronous task representing the toggle operation result.</returns>
        public async Task<bool> ToggleLikeCollectionAsync(int userId, int collectionId)
        {
            var user = await _repository.GetAllAsync<User>().Result
                .Include(u => u.LikedCollections)
                .FirstOrDefaultAsync(u => u.Id == userId);
            var collection = await _repository.GetByIdAsync<Collection>(collectionId);
            if (user == null || collection == null) return false;

            if (!user.LikedCollections.Contains(collection))
            {
                user.LikedCollections.Add(collection);
                collection.Likes++;
                await _repository.SaveChangesAsync();
                return true;
            }
            else
            {
                user.LikedCollections.Remove(collection);
                collection.Likes--;
                await _repository.SaveChangesAsync();
                return true;
            }
        }
    }
}