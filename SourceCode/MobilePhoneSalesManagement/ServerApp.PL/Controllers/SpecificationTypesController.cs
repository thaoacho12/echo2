using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Exceptions;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
namespace ServerApp.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class SpecificationTypesController : ControllerBase
    {
        private readonly ISpecificationTypeService _specificationTypeService;

        public SpecificationTypesController(ISpecificationTypeService specificationTypeService)
        {
            _specificationTypeService = specificationTypeService;
        }

        [HttpGet("get-all-specificationTypes")]
        public async Task<ActionResult<IEnumerable<SpecificationTypeVm>>> GetSpecificationTypes()
        {
            var result = await _specificationTypeService.GetAllSpecificationTypeAsync();

            return Ok(result); // 200 OK nếu có dữ liệu.
        }

        [HttpGet("get-specificationType-by-id/{id}")]
        public async Task<ActionResult<SpecificationTypeVm>> GetSpecificationType(int id)
        {
            var result = await _specificationTypeService.GetBySpecificationTypeIdAsync(id);

            if (result == null)
            {
                return NotFound(new { Message = $"SpecificationType with ID {id} not found." });
            }

            return Ok(result); // 200 OK nếu tìm thấy.
        }

        [HttpPost("add-new-specificationType")]
        public async Task<ActionResult<SpecificationTypeVm>> PostSpecificationType(InputSpecificationTypeVm specificationTypeVm)
        {
            var result = await _specificationTypeService.AddSpecificationTypeAsync(specificationTypeVm);

            if (result == null)
            {
                return BadRequest(new { Message = "Failed to create the specificationType." });
            }

            return CreatedAtAction(nameof(GetSpecificationType), new { id = result.SpecificationTypeId }, result);
        }

        [HttpPut("update-specificationType/{id}")]
        public async Task<IActionResult> PutSpecificationType(int id, InputSpecificationTypeVm specificationTypeVm)
        {
            var result = await _specificationTypeService.UpdateSpecificationTypeAsync(id, specificationTypeVm);
            if (result == null)
            {
                return NotFound(new { Message = $"SpecificationType with ID {id} not found." });
            }

            return Ok(result);
        }

        [HttpDelete("delete-specificationType-by-id/{id}")]
        public async Task<IActionResult> DeleteSpecificationType(int id)
        {
            var result = await _specificationTypeService.DeleteSpecificationTypeAsync(id);
            if (result == null)
            {
                return NotFound(new { Message = $"SpecificationType with ID {id} not found." });
            }

            return NoContent();
        }
    }
}
