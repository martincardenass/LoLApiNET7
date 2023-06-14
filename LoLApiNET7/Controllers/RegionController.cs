using LoLApiNET7.Models;
using LoLApiNET7.Services;
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
    }
}
