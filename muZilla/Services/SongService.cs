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

        public async Task UpdateCoverIdOnly(int songId, int coverId)
        {
            Song song = _context.Songs.Select(s => s).Where(s => s.Id == songId).FirstOrDefault();
            song.Cover = _context.Images.Select(i => i).Where(i => i.Id == coverId).FirstOrDefault();
            _context.Update(song);
            await _context.SaveChangesAsync();
        }

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
            song.Length = songDTO.Length;
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

        public async Task DeleteSongByIdAsync(int id)
        {
            Song? song = await _context.Songs.FindAsync(id);
            if (song != null) _context.Songs.Remove(song);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Song>> GetSongsByKeyWord(string search, FilterDTO filterDTO)
        {
            List<Song> filteredSongs = _context.Songs
                .Include(s => s.Authors)
                .Where(s =>
                    (s.Title.Contains(search)
                    || s.Description.Contains(search)
                    || s.Authors.Any(a => a.Username.Contains(search))
                    || s.Genres.Contains(search))
                )
                .AsEnumerable()
                .Where(s => s.Genres.Split(", ").ToList().Count == filterDTO.Genres.Split(", ").ToList().Count
                && !s.Genres.Split(", ").ToList().Except(filterDTO.Genres.Split(", ").ToList()).Any())
                .Where(s =>
                    (filterDTO.Remixes == null
                    || (filterDTO.Remixes == true ? s.Original != null : s.Original == null))
                    && (filterDTO.ShowBanned == false ? s.IsBanned == false : true)
                )
                .OrderBy(s => s.Likes * s.Views)
                .ToList();

            return filteredSongs;
        }

        public async Task AddOneView(int songId)
        {
            Song? song = await GetSongByIdAsync(songId);
            if (song != null) song.Views += 1;
            else return;
            _context.Songs.Update(song);
            await _context.SaveChangesAsync();
        }
      
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
