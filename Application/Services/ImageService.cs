using System.Drawing;
using muZilla.Application.DTOs;
using muZilla.Application.Interfaces;
using muZilla.Entities.Models;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public async Task<int> CreateImageAsync(ImageDTO imageDTO)
        {
            int imageId;

            if (imageDTO.DomainColor == null)
            {
                string[] separetedFilePath = imageDTO.ImageFilePath.Split('/');
                var imageBytes = (await _fileStorageService.ReadFileFromSongAsync(separetedFilePath[0], int.Parse(separetedFilePath[1]), separetedFilePath[2], null))!;

                using var memoryStream = new MemoryStream(imageBytes);

                using var image = new Bitmap(memoryStream);

                Color color = FileStorageService.GetDominantColor(image);

                var _image = new Entities.Models.Image()
                {
                    ImageFilePath = imageDTO.ImageFilePath,
                    DomainColor = imageDTO.DomainColor != null ? imageDTO.DomainColor : color.ToString(),
                };

                imageId = (await _repository.AddAsync<Entities.Models.Image>(_image)).Id;
                await _repository.SaveChangesAsync();
            }
            else
            {
                var _image = new Entities.Models.Image()
                {
                    ImageFilePath = imageDTO.ImageFilePath,
                    DomainColor = imageDTO.DomainColor
                };

                imageId = (await _repository.AddAsync<Entities.Models.Image>(_image)).Id;
                await _repository.SaveChangesAsync();
            }
            return imageId;
        }


        /// <summary>
        /// Retrieves an image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the image.</param>
        /// <returns>The image if found, or null if not found.</returns>
        public async Task<Entities.Models.Image?> GetImageById(int id)
        {
            var image = await _repository.GetByIdAsync<Entities.Models.Image>(id);

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
            var image = await _repository.GetByIdAsync<Entities.Models.Image>(id);
            if (image != null)
            {
                image.ImageFilePath = imageDTO.ImageFilePath;
                image.DomainColor = imageDTO.DomainColor;
                    
                await _repository.UpdateAsync<Entities.Models.Image>(image);
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
            var image = await _repository.GetByIdAsync<Entities.Models.Image>(id);
            if (image != null)
            {
                await _repository.RemoveAsync<Entities.Models.Image>(image);
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
            var latestImage = _repository.GetAllAsync<Entities.Models.Image>().Result
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