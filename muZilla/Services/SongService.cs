using Microsoft.EntityFrameworkCore;

using muZilla.Data;
using muZilla.Models;
using muZilla.DTOs;

namespace muZilla.Services
{
    public class SongService
    {
        private readonly MuzillaDbContext _context;

        public SongService(MuzillaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Updates the cover image of a song by its ID.
        /// </summary>
        /// <param name="songId">The unique identifier of the song.</param>
        /// <param name="coverId">The unique identifier of the new cover image.</param>
        /// <returns>An asynchronous task representing the update operation.</returns>
        public async Task UpdateCoverIdOnly(int songId, int coverId)
        {
            Song song = _context.Songs.Select(s => s).Where(s => s.Id == songId).FirstOrDefault();
            song.Cover = _context.Images.Select(i => i).Where(i => i.Id == coverId).FirstOrDefault();
            _context.Update(song);
            await _context.SaveChangesAsync();
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
            Image? image = await _context.Images.FindAsync(songDTO.ImageId);

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
                if (GetSongByIdAsync(songDTO.OriginalId.Value).Result.RemixesAllowed == false)
                    return -1;
                else
                    GetSongByIdAsync(songDTO.OriginalId.Value).Result.Remixes.Add(song);
            }

            song.Authors = new List<User>();
            foreach (int i in songDTO.AuthorIds)
                song.Authors
                    .Add(await _context.Users
                    .Include(u => u.AccessLevel)
                    .FirstOrDefaultAsync(u => u.Id == i));

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
            int id = _context.Songs.OrderBy(s => s.Id).LastOrDefault().Id;
            return id;
        }

        /// <summary>
        /// Retrieves a song by its unique identifier, including its remixes.
        /// </summary>
        /// <param name="id">The unique identifier of the song.</param>
        /// <returns>The song object if found, otherwise null.</returns>
        public async Task<Song> GetSongByIdAsync(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Remixes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
            {
                return null;
            }

            var remixes = await _context.Songs
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
            var song = await _context.Songs
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
            song.Cover = songDTO.ImageId.HasValue ? await _context.Images.FindAsync(songDTO.ImageId.Value) : null;

            song.Authors.Clear();
            foreach (var authorId in songDTO.AuthorIds)
            {
                var author = await _context.Users
                    .Include(u => u.AccessLevel)
                    .FirstOrDefaultAsync(u => u.Id == authorId);

                if (author != null)
                {
                    song.Authors.Add(author);
                }
            }

            _context.Songs.Update(song);
            await _context.SaveChangesAsync();

            return 200;
        }

        /// <summary>
        /// Deletes a song by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the song to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteSongByIdAsync(int id)
        {
            Song? song = await _context.Songs.FindAsync(id);
            if (song != null) _context.Songs.Remove(song);

            await _context.SaveChangesAsync();
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
            _context.Songs.Update(song);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Toggles the like status of a song for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user liking or unliking the song.</param>
        /// <param name="songId">The ID of the song to toggle like status.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task ToggleLikeSongAsync(int userId, int songId)
        {
            var user = await _context.Users
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

                _context.Collections.Add(favoritesCollection);
                await _context.SaveChangesAsync();

                user.FavoritesCollectionId = favoritesCollection.Id;
                user.FavoritesCollection = favoritesCollection;
            }

            var song = await _context.Songs.FindAsync(songId);

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

            await _context.SaveChangesAsync();
        }
    }
}
