using Microsoft.EntityFrameworkCore;
using muZilla.Data;
using muZilla.Models;

namespace muZilla.Services
{
    public class SongDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public string Genres { get; set; }
        public bool RemixesAllowed { get; set; }
        public int? OriginalId { get; set; }
        public bool HasExplicitLyrics { get; set; }
        public int ImageId { get; set; }
        public List<int> AuthorIds { get; set; }
    }
    public class SongService
    {
        private readonly MuzillaDbContext _context;

        public SongService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task CreateSongAsync(SongDTO songDTO)
        {
            Song song = new Song()
            {
                Title = songDTO.Title,
                Description = songDTO.Description,
                Length = songDTO.Length,
                Genres = songDTO.Genres,
                OriginalId = songDTO.OriginalId,
                HasExplicitLyrics = songDTO.HasExplicitLyrics,
                Cover = await _context.Images.FindAsync(songDTO.ImageId),
            };


            song.Authors = new List<User>();
            foreach (int i in songDTO.AuthorIds)
                song.Authors
                    .Add(await _context.Users
                    .Include(u => u.AccessLevel)
                    .FirstOrDefaultAsync(u => u.Id == i));

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
        }

        public async Task<Song> GetSongByIdAsync(int id)
        {
            return await _context.Songs
                .Include(s => s.Remixes)
                .Include(s => s.Original)
                .Include(s => s.Cover)
                .Include(s => s.Authors)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSongByIdAsync(int id, SongDTO songDTO)
        {
            Song song = await _context.Songs.FindAsync(id);
            song.Title = songDTO.Title;
            song.Description = songDTO.Description;
            song.Length = songDTO.Length;
            song.Genres = songDTO.Genres;
            song.OriginalId = songDTO.OriginalId;
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
        }
    }
}
