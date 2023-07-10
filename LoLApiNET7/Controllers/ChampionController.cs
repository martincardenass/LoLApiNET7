using LoLApiNET7.Dto;
using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{   //AUTHORIZE = Login and add Bearer Token, also send body
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
                    Release_date = item.Release_date
                };

                championsDto.Add(championsMap);
            }

            return Ok(championsDto);
        }

        [HttpGet("info")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChampionInfo>))]
        public IActionResult GetChampionsInfo()
        {
            var championsInfo = _championService.GetChampionsInfo();

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(championsInfo);
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
                Image = champion.Image
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(championDto);
        }

        [HttpGet("name/{champName}")]
        [ProducesResponseType(200, Type = typeof(ChampionInfo))]
        [ProducesResponseType(400)]
        public IActionResult GetChampionByName(string champName)
        {
            if (!_championService.ChampionNameExists(champName))
                return NotFound();

            var images = _championService.GetChampionImages(champName);
            var champion = _championService.GetChampionByName(champName);

            var championDto = new ChampionDto
            {
                Champion_id = champion.Champion_Id,
                Name = champion.Name,
                Image = images.FirstOrDefault(),
                AdditionalImages = images.Skip(1).ToList(), // Skips the first image (because we showed it in "Image" and displays the rest as additional images
                Release_date = champion.Release_Date,
                Region_Name = champion.Region_Name,
                Region_Emblem = champion.Region_Emblem,
                Role_Name = champion.Role_Name,
                Role_Icon = champion.Role_Icon,
                Champ_Icons = champion.Champ_Icons,
                Catchphrase = champion.Catchphrase,
                Description = champion.Description
            };

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(championDto);
        }

        [HttpGet("image/{champName}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChampionInfo>))]
        [ProducesResponseType(400)]
        public IActionResult GetChampionImages(string champName)
        {
            var champion = _championService.GetChampionImages(champName);

            if (!_championService.ChampionNameExists(champName))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(champion);
        }

        [HttpGet("role/id/{roleId}")] // To get the champions by its role ID
        [ProducesResponseType(200, Type = typeof(IEnumerable<Champion>))]
        public IActionResult GetChampionsByRole(int roleId)
        {
            var championsRole = _championService.GetChampionsByRole(roleId);

            if (!_roleService.RoleIdExists(roleId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(championsRole);
        }

        [HttpGet("role/name/{roleName}")] // To get the champions by its role name
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChampionInfo>))]
        public IActionResult GetChampionsByRoleName(string roleName)
        {
            var championsRole = _championService.GetChampionsByRoleName(roleName);

            if (!_roleService.RoleNameExists(roleName))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(championsRole);
        }

        [HttpGet("region/name/{regionName}")] // To get the champions by its role name
        [ProducesResponseType(200, Type = typeof(IEnumerable<Champion>))]
        public IActionResult GetChampionsByRegionName(string regionName)
        {
            var championsRegion = _championService.GetChampionsByRegionName(regionName);

            if(!_regionService.RegionNameExists(regionName))
                return NotFound();

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(championsRegion);
        }

        [HttpGet("related/{regionName}/{roleName}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChampionInfo>))]
        public IActionResult GetRelatedChampions(string regionName, string roleName)
        {
            var relatedChampions = _championService.GetRelatedChampions(regionName, roleName);

            if (!_regionService.RegionNameExists(regionName))
                return NotFound($"The region: '{regionName}' does not exist");

            if (!_roleService.RoleNameExists(roleName))
                return NotFound($"The role: '{roleName}' does not exist");

            if (relatedChampions.Count == 0) // If no champions met the filter criteria. Example > Ixtal Support. Theres none
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(relatedChampions);
        }

        [HttpPost]
        [Authorize(Policy = "UserAllowed")] //users and admins can create champions
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
        [Authorize(Policy = "AdminOnly")] //Only admins can delete
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
        [Authorize(Policy = "AdminOnly")] //Only admins can delete
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateChampion(int champId, [FromQuery] int RegionId, [FromQuery] int RoleId, [FromBody] ChampionDto updatedChampion)
        { // dont forget to pass champion_id in the send body, otherwise it will not work
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
