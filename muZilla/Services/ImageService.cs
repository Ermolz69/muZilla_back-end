using Microsoft.AspNetCore.Mvc;
using muZilla.Data;
using muZilla.Models;
using System.ComponentModel.DataAnnotations;


namespace muZilla.Services
{
    public class ImageDTO
    {
        public string ImageFilePath { get; set; }
        public string DomainColor { get; set; }
    }

    public class ImageService
    {
        private readonly MuzillaDbContext _context;

        public ImageService(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task CreateImageAsync(ImageDTO imageDTO)
        {
            var image = new Image()
            {
                ImageFilePath = imageDTO.ImageFilePath,
                DomainColor = imageDTO.DomainColor,
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }

        public async Task<Image?> GetImageById(int id)
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