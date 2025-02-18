using System.Drawing;


using muZilla.Entities.Models;

using muZilla.Application.DTOs;
using muZilla.Application.Interfaces;


namespace muZilla.Application.Services
{

    public class ImageService
    {
        private readonly IGenericRepository _repository;
        private readonly FileStorageService _fileStorageService;

        public ImageService(IGenericRepository repository, FileStorageService fileStorageService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Creates a new image record in the database, calculating its dominant color if not provided.
        /// </summary>
        /// <param name="imageDTO">The data transfer object containing image details, including file path and optional domain color.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the image file cannot be read.</exception>
        public async Task CreateImageAsync(ImageDTO imageDTO)
        {
            if (imageDTO.DomainColor == null)
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
                    DomainColor = imageDTO.DomainColor != null ? imageDTO.DomainColor : $"{color.R},{color.G},{color.B}"
                };

                _repository.Images.Add(_image);
                await _repository.SaveChangesAsync();
            }
            else
            {
                var _image = new Models.Image()
                {
                    ImageFilePath = imageDTO.ImageFilePath,
                    DomainColor = imageDTO.DomainColor
                };

                _repository.Images.Add(_image);
                await _repository.SaveChangesAsync();
            }
        }


        /// <summary>
        /// Retrieves an image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the image.</param>
        /// <returns>The image if found, or null if not found.</returns>
        public async Task<Models.Image?> GetImageById(int id)
        {
            var image = await _repository.Images.FindAsync(id);

            return image;
        }

        /// <summary>
        /// Updates an existing image record with new details.
        /// </summary>
        /// <param name="id">The unique identifier of the image to update.</param>
        /// <param name="imageDTO">The data transfer object containing updated image details.</param>
        /// <returns>An asynchronous task representing the update operation.</returns>
        public async Task UpdateImageByIdAsync(int id, ImageDTO imageDTO)
        {
            var image = await _repository.Images.FindAsync(id);
            if (image != null)
            {
                image.ImageFilePath = imageDTO.ImageFilePath;
                image.DomainColor = imageDTO.DomainColor;
                    
                _repository.Images.Update(image);
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes an image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the image to delete.</param>
        /// <returns>An asynchronous task representing the deletion operation.</returns>
        public async Task DeleteImageByIdAsync(int id)
        {
            var image = await _repository.Images.FindAsync(id);
            if (image != null)
            {
                _repository.Images.Remove(image);
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves the ID of the most recently added image in the database.
        /// </summary>
        /// <returns>The unique identifier of the latest image.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no images are found.</exception>
        public int GetNewestAsync()
        {
            var latestImage = _repository.Images
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