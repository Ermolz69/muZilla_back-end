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

        [HttpPost("create")]
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

        [HttpGet("{id}")]
        public async Task<Models.Image?> GetImageById(int id)
        {
            return await _imageService.GetImageById(id);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateImageById(int id, ImageDTO imageDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _imageService.UpdateImageByIdAsync(id, imageDTO);
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteImageById(int id)
        {
            await _imageService.DeleteImageByIdAsync(id);
            return Ok();
        }
    }

}