using Microsoft.AspNetCore.Mvc;

using muZilla.Services;
using muZilla.Models;
using muZilla.DTOs;
using System.Drawing;

namespace muZilla.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        private readonly ImageService _imageService;
        private readonly FileStorageService _fileStorageService;

        public ImageController(ImageService imageService, FileStorageService fileStorageService)
        {
            _imageService = imageService;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Creates a new image record.
        /// </summary>
        /// <param name="imageDTO">The data transfer object containing image details.</param>
        /// <returns>A 200 OK response upon successful creation, or a 400 Bad Request if the input is invalid.</returns>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateImage(ImageDTO imageDTO)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("BAD REQUEST!");
                return BadRequest(ModelState);
            }

            await _imageService.CreateImageAsync(imageDTO);
            return Ok();
        }

        /// <summary>
        /// Retrieves an image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the image.</param>
        /// <returns>The image details if found, or null if not found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Models.Image?> GetImageById(int id)
        {
            return await _imageService.GetImageById(id);
        }

        /// <summary>
        /// Updates an existing image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the image to update.</param>
        /// <param name="imageDTO">The updated data for the image.</param>
        /// <returns>A 200 OK response upon successful update, or a 400 Bad Request if the input is invalid.</returns>
        [HttpPatch("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateImageById(int id, ImageDTO imageDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _imageService.UpdateImageByIdAsync(id, imageDTO);
            return Ok();
        }

        /// <summary>
        /// Deletes an image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the image to delete.</param>
        /// <returns>A 200 OK response upon successful deletion.</returns>
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteImageById(int id)
        {
            await _imageService.DeleteImageByIdAsync(id);
            return Ok();
        }
    }

}