using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : Controller
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Region>))]
        public IActionResult GetRegions()
        {
            var regions = _regionService.GetRegions();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(regions);
        }

        [HttpGet("id/{regId}")]
        [ProducesResponseType(200, Type = typeof(Region))]
        [ProducesResponseType(400)]
        public IActionResult GetRegionById(int regId)
        {
            if (!_regionService.RegionIdExists(regId))
            {
                return NotFound();
            }

            var region = _regionService.GetRegionById(regId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(region);
        }

        [HttpGet("name/{regName}")]
        [ProducesResponseType(200, Type = typeof(Region))]
        [ProducesResponseType(400)]
        public IActionResult GetRegionByName(string regName)
        {
            if (!_regionService.RegionNameExists(regName))
            {
                return NotFound();
            }

            var region = _regionService.GetRegionByName(regName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(region);
        }

        [HttpPost]
        [Authorize(Policy = "UserAllowed")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateRegion([FromBody] Region region)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_regionService.CreateRegion(region))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while creating the region");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("id/{regId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteRegion(int regId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(!_regionService.RegionIdExists(regId))
                return BadRequest("The id " + regId + " does not exist or was already deleted.");
            
            var regionToDelete = _regionService.GetRegionById(regId);

            if(!_regionService.DeleteRegion(regionToDelete))
            {
                ModelState.AddModelError("", "Something happened while deleting region");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpPatch("id/{regId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateRegion(int regId, [FromBody] Region updatedRegion)
        {
            if (updatedRegion == null)
                return BadRequest(ModelState);

            if (!_regionService.RegionIdExists(regId))
                return BadRequest("Role does not exist " + ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var regionMap = _regionService.GetRegionById(regId);

            regionMap.Name = updatedRegion.Name ?? regionMap.Name; // Validating each field. If no value is provided, it wont be updated.
            regionMap.Description = updatedRegion.Description ?? regionMap.Description;

            if (!_regionService.UpdateRegion(regionMap))
            {
                ModelState.AddModelError("", "Something went wrong while updating the region");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
