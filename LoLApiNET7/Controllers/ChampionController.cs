using LoLApiNET7.Dto;
using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChampionController : Controller
    {
        private readonly IChampionService _championService;
        private readonly IRegionService _regionService;
        private readonly IRoleService _roleService;

        public ChampionController(IChampionService championService, IRegionService regionService, IRoleService roleService) //Getting the dependencies that we need
        {
            _championService = championService;
            _regionService = regionService;
            _roleService = roleService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Champion>))]
        public IActionResult GetChampions()
        {
            var champions = _championService.GetChampions();
            var championsDto = new List<ChampionDto>();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var item in champions)
            {
                var championsMap = new ChampionDto
                {
                    Champion_id = item.Champion_id,
                    Name = item.Name,
                    Release_date = item.Release_date,
                    Image = item.Image
                };

                championsDto.Add(championsMap);
            }

            return Ok(championsDto);
        }

        [HttpGet("id/{champId}")]
        [ProducesResponseType(200, Type = typeof(Champion))]
        [ProducesResponseType(400)]
        public IActionResult GetChampionById(int champId)
        {
            if (!_championService.ChampionIdExists(champId))
            {
                return NotFound();
            }

            var champion = _championService.GetChampionById(champId);

            var championDto = new ChampionDto
            {
                Champion_id = champion.Champion_id,
                Name = champion.Name,
                Release_date = champion.Release_date,
                Image = champion.Image,
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(championDto);
        }

        [HttpGet("name/{champName}")]
        [ProducesResponseType(200, Type = typeof(Champion))]
        [ProducesResponseType(400)]
        public IActionResult GetChampionByName(string champName)
        {
            if (!_championService.ChampionNameExists(champName))
            {
                return NotFound();
            }
            var champion = _championService.GetChampionByName(champName);

            var championDto = new ChampionDto
            {
                Champion_id = champion.Champion_id,
                Name = champion.Name,
                Release_date = champion.Release_date,
                Image = champion.Image,
            };


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(championDto);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateChampion([FromQuery] int RegionId, [FromQuery] int RoleId, [FromBody] Champion champion)
        {
            if (RegionId == 0 || RoleId == 0)
                return BadRequest("To create a champion you must insert: \n RegionId \n RoleId \n And the champion's information \n" + ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_championService.CreateChampion(RegionId, RoleId, champion))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while creating the champion");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("id/{champId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteChampion(int champId)
        {
            if (!_championService.ChampionIdExists(champId))
                return BadRequest("The id " + champId + " does not exist or was already deleted.");

            var championToDelete = _championService.GetChampionById(champId);

            if (!_championService.DeleteChampion(championToDelete))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while deleting the champion.");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpPatch("id/{champId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateChampion(int champId, [FromQuery] int RegionId, [FromQuery] int RoleId, [FromBody] ChampionDto updatedChampion)
        {
            if (!_championService.ChampionIdExists(champId)) //checking if the champId from query exists
            {
                var errorMsg = "A champion with Id: " + champId + " Does not exist"; // could use string interpolation here
                return NotFound(new { Message = errorMsg });
            }

            if (!_regionService.RegionIdExists(RegionId)) //checking if the RegionId from query exists
            {
                var errorMsg = "A region with Id: " + RegionId + " Does not exist"; // could use string interpolation here
                return NotFound(new { Message = errorMsg });
            }

            if (!_roleService.RoleIdExists(RoleId)) //checking if the RoleId from query exists
            {
                var errorMsg = "A role with Id: " + RoleId + " Does not exist"; // could use string interpolation here
                return NotFound(new { Message = errorMsg });
            }

            if (champId != updatedChampion.Champion_id) return BadRequest(); // if the ids from query and body dont match, return bad request

            var championMap = _championService.GetChampionById(champId); //getting the original value of the champion before editing it

            championMap.Name = updatedChampion.Name ?? championMap.Name; // Validating each field. If no value is provided, it wont be updated.
            championMap.Image = updatedChampion.Image ?? championMap.Image;
            championMap.Release_date = updatedChampion.Release_date ?? championMap.Release_date;

            if (!_championService.UpdateChampion(RegionId, RoleId, championMap))
            {
                ModelState.AddModelError("", "Something went wrong while updating the champion");
                return StatusCode(500, ModelState);
            }

            //_championService.UpdateChampion(RegionId, RoleId, championMap);

            return NoContent();
        }
    }
}
