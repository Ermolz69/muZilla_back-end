using Microsoft.EntityFrameworkCore;

using muZilla.Entities.Models;

using muZilla.Application.DTOs.Song;
using muZilla.Application.Interfaces;
using muZilla.Application.DTOs.User;

namespace muZilla.Application.Services
{
    public class SongService
    {
        private readonly IGenericRepository _repository;

        public SongService(IGenericRepository repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// Checks if the provided song id is valid.
        /// </summary>
        /// <param name=id">song id.</param>
        /// <returns>
        /// <c>true</c> if the song id is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method verifies if the song id exist in database.
        /// </remarks>
        public bool IsSongValid(int id)
        {
            if (_repository.GetAllAsync<Song>().Result.Select(u => u).Where(u => u.Id == id).Any())
                return false;

            return true;
        }
        /// <summary>
        /// Updates the cover image of a song by its ID.
        /// </summary>
        /// <param name="songId">The unique identifier of the song.</param>
        /// <param name="coverId">The unique identifier of the new cover image.</param>
        /// <returns>An asynchronous task representing the update operation.</returns>
        public async Task UpdateCoverIdOnly(int songId, int coverId)
        {
            Song? song = _repository.GetAllAsync<Song>().Result
                .Select(s => s)
                .Where(s => s.Id == songId)
                .FirstOrDefault();

            if (song == null)
                return;
            
            song.Cover = await _repository.GetByIdAsync<Image>(coverId);
            await _repository.UpdateAsync(song);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a new song based on the provided data transfer object (DTO).
        /// </summary>
        /// <param name="songDTO">The data transfer object containing song details.</param>
        /// <returns>
        /// The ID of the newly created song, or -1 if the song's original is not allowed to have remixes.
        /// </returns>
        public async Task<int> CreateSongAsync(SongDTO songDTO)
        {
            Image? image = await _repository.GetByIdAsync<Image>(songDTO.ImageId.Value);

            Song song = new Song()
            {
                Title = songDTO.Title,
                Description = songDTO.Description,
                Length = songDTO.Length,
                Genres = songDTO.Genres,
                OriginalId = songDTO.OriginalId,
                PublishDate = songDTO.PublishDate,
                RemixesAllowed = songDTO.RemixesAllowed,
                HasExplicitLyrics = songDTO.HasExplicitLyrics,
                Cover = image,
            };

            if (songDTO.OriginalId != null)
            {
                if (GetSongByIdAsync(songDTO.OriginalId.Value).Result!.RemixesAllowed == false)
                    return -1;
                else
                    GetSongByIdAsync(songDTO.OriginalId.Value).Result!.Remixes.Add(song);
            }

            song.Authors = new List<User>();
            foreach (int authorId in songDTO.AuthorIds)
            {
                var author = await _repository.GetByIdAsync<User>(authorId);
                if (author != null)
                    song.Authors.Add(author);
            }

            await _repository.AddAsync(song);
            await _repository.SaveChangesAsync();

            int id = (await _repository.GetAllAsync<Song>())
                .OrderBy(s => s.Id)
                .LastOrDefault()?.Id ?? 0;

            return id;

        }

        /// <summary>
        /// Retrieves a song by its unique identifier, including its remixes.
        /// </summary>
        /// <param name="id">The unique identifier of the song.</param>
        /// <returns>The song object if found, otherwise null.</returns>
        public async Task<Song?> GetSongByIdAsync(int id)
        {
            var song = await _repository.GetAllAsync<Song>().Result
                .Include(s => s.Remixes)
                .Include(s => s.Authors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
            {
                return null;
            }

            var remixes = await _repository.GetAllAsync<Song>().Result
                .Where(s => s.Original != null && s.Original.Id == id)
                .ToListAsync();

            foreach (var remix in remixes) {
                song.Remixes.Add(remix);
            }

            return song;
        }

        /// <summary>
        /// Updates an existing song by its ID with the provided details.
        /// </summary>
        /// <param name="id">The unique identifier of the song to update.</param>
        /// <param name="songDTO">The data transfer object containing updated song details.</param>
        /// <returns>
        /// A status code indicating the result: 
        /// 200 for success, 404 if the song is not found.
        /// </returns>
        public async Task<int> UpdateSongByIdAsync(int id, SongDTO songDTO)
        {
            var song = await _repository.GetAllAsync<Song>().Result
                .Include(s => s.Authors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
            {
                return 404;
            }

            song.Title = songDTO.Title;
            song.Description = songDTO.Description;
            song.Genres = songDTO.Genres;
            song.RemixesAllowed = songDTO.RemixesAllowed;
            song.PublishDate = songDTO.PublishDate;
            song.HasExplicitLyrics = songDTO.HasExplicitLyrics;
            song.Cover = songDTO.ImageId.HasValue ? await _repository.GetByIdAsync<Image>(songDTO.ImageId.Value) : null;

            song.Authors.Clear();
            foreach (var authorId in songDTO.AuthorIds)
            {
                var author = await _repository.GetAllAsync<User>().Result
                    .Include(u => u.AccessLevel)
                    .FirstOrDefaultAsync(u => u.Id == authorId);

                if (author != null)
                {
                    song.Authors.Add(author);
                }
            }

            await _repository.UpdateAsync(song);
            await _repository.SaveChangesAsync();

            return 200;
        }

        /// <summary>
        /// Deletes a song by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the song to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteSongByIdAsync(int id)
        {
            Song? song = await _repository.GetByIdAsync<Song>(id);
            if (song != null) await _repository.RemoveAsync(song);

            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Increments the view count of a song by its ID.
        /// </summary>
        /// <param name="songId">The unique identifier of the song.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task AddOneView(int songId)
        {
            Song? song = await GetSongByIdAsync(songId);
            if (song != null) song.Views += 1;
            else return;
            await _repository.UpdateAsync(song);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Toggles the like status of a song for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user liking or unliking the song.</param>
        /// <param name="songId">The ID of the song to toggle like status.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task ToggleLikeSongAsync(int userId, int songId)
        {
            var user = await _repository.GetAllAsync<User>().Result
                .Include(u => u.FavoritesCollection)
                    .ThenInclude(c => c.Songs)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return;

            if (user.FavoritesCollection == null)
            {
                var favoritesCollection = new Collection
                {
                    Title = "Favorites",
                    Description = "Your favorite songs",
                    ViewingAccess = 0,
                    IsFavorite = true,
                    IsBanned = false,
                    Author = user,
                    Songs = new List<Song>(),
                    Cover = new Image()
                    {
                        ImageFilePath = "DEFAULT/default.jpg",
                        DomainColor = "125,125,125"
                    }
                };

                await _repository.AddAsync(favoritesCollection);
                await _repository.SaveChangesAsync();

                user.FavoritesCollectionId = favoritesCollection.Id;
                user.FavoritesCollection = favoritesCollection;
            }

            var song = await _repository.GetByIdAsync<Song>(songId);

            if (song == null) return;

            if (user.FavoritesCollection.Songs.Any(s => s.Id == songId))
            {

                var songToRemove = user.FavoritesCollection.Songs.First(s => s.Id == songId);
                user.FavoritesCollection.Songs.Remove(songToRemove);
                song.Likes--;

            }
            else
            {
                user.FavoritesCollection.Songs.Add(song);
                song.Likes++;
            }

            await _repository.SaveChangesAsync();
        }
    }
}
