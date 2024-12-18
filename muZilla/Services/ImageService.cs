using muZilla.Models;
using muZilla.Data;
using muZilla.DTOs;
using System.Drawing;


namespace muZilla.Services
{

    public class ImageService
    {
        private readonly MuzillaDbContext _context;
        private readonly FileStorageService _fileStorageService;

        public ImageService(MuzillaDbContext context, FileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task CreateImageAsync(ImageDTO imageDTO)
        {
            string[] st = imageDTO.ImageFilePath.Split('/');
            var imageBytes = await _fileStorageService.ReadFileFromSongAsync(st[0], int.Parse(st[1]), st[2], null);
            var pixels = new List<Color>();

            using var memoryStream = new MemoryStream(imageBytes);

            using var image = new Bitmap(memoryStream);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(image.GetPixel(x, y));
                }
            }

            Color color = _fileStorageService.GetDominantColor(pixels);

            var _image = new Models.Image()
            {
                ImageFilePath = imageDTO.ImageFilePath,
                DomainColor = imageDTO.DomainColor != null ? imageDTO.DomainColor : $"{color.R},{color.G},{color.B}"};

            _context.Images.Add(_image);
            await _context.SaveChangesAsync();
        }

        public async Task<Models.Image?> GetImageById(int id)
        {
            var image = await _context.Images.FindAsync(id);

            return image;
        }

        public async Task UpdateImageByIdAsync(int id, ImageDTO imageDTO)
        {
            var image = await _context.Images.FindAsync(id);
            if (image != null)
            {
                image.ImageFilePath = imageDTO.ImageFilePath;
                image.DomainColor = imageDTO.DomainColor;
                    
                _context.Images.Update(image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteImageByIdAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        public int GetNewestAsync()
        {
            var latestImage = _context.Images
                .OrderByDescending(i => i.Id)
                .FirstOrDefault();

            if (latestImage == null)
            {
                throw new InvalidOperationException("No images found.");
            }

            return latestImage.Id;
        }

    }
}