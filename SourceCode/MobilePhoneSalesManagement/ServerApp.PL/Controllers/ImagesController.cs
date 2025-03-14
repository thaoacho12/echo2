using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Data;
using ServerApp.DAL.Infrastructure;

namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ShopDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ImagesController(IImageService iageService, ShopDbContext context, IUnitOfWork unitOfWork)
        {
            _imageService = iageService;
            _context = context;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("add-image")]
        public async Task<IActionResult> AddImageAsync([FromBody] ImageRequest imageRequest)
        {
            return Ok(await _imageService.AddImageAsync(imageRequest));
            return BadRequest("Invalid product data");
        }

        [HttpPut("update-image/{id}")]
        public async Task<IActionResult> UpdateImage(int id, [FromBody] ImageRequest imageRequest)
        {
            return Ok(await _imageService.UpdateImageAsync(id, imageRequest));
            return BadRequest("Invalid product data");
        }

        [HttpGet("get-image-by-id/{id}")]
        public IActionResult GetImage(int id)
        {
            var image = _imageService.GetByImageIdAsync(id);

            return Ok(new { image });

        }
        [HttpGet("get-all-images-by-page")]
        public IActionResult GetAllImage([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var imageIds = _imageService.GetAllImageId(pageNumber, pageSize);

            return Ok(new { imageIds });

        }
    }
}