using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Migrations;
using muZilla.Models;
using System.Linq;
using System.Security.Cryptography.Xml;

namespace muZilla.Services
{
    public class SongDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public string Genres { get; set; }
        public bool RemixesAllowed { get; set; }
        public DateTime PublishDate { get; set; }
        public int? OriginalId { get; set; }
        public bool HasExplicitLyrics { get; set; }
        public int? ImageId { get; set; }
        public List<int> AuthorIds { get; set; }
    }
    public class FilterDTO
    {
        public string? Genres { get; set; }
        public bool? Remixes { get; set; }
        public bool ShowBanned { get; set; } = false;
    }

    public class SongService
    {
        private readonly MuzillaDbContext _context;

        public SongService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateSongAsync(SongDTO songDTO)
        {
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
                Cover = await _context.Images.FindAsync(songDTO.ImageId),
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
            Song song = await _context.Songs
                .Include(s => s.Cover)
                .Include(s => s.Authors)
                .FirstOrDefaultAsync(s => s.Id == id);
            foreach (var xs in _context.Songs.Select(s => s).Where(s => s.OriginalId == song.Id).ToList())
                song.Remixes.Add(xs);
            return song;
        }

        public async Task UpdateSongByIdAsync(int id, SongDTO songDTO)
        {
            Song song = await _context.Songs.FindAsync(id);
            song.Title = songDTO.Title;
            song.Description = songDTO.Description;
            song.Length = songDTO.Length;
            song.Genres = songDTO.Genres;
            song.PublishDate = songDTO.PublishDate;
            song.OriginalId = songDTO.OriginalId;
            song.RemixesAllowed = songDTO.RemixesAllowed;
            song.HasExplicitLyrics = songDTO.HasExplicitLyrics;
            song.Cover = await _context.Images.FindAsync(songDTO.ImageId);
            foreach (int i in songDTO.AuthorIds)
                song.Authors.Add(await _context.Users.FindAsync(i));

            await _context.SaveChangesAsync();
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
    }
}
