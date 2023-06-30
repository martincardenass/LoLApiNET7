using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Role>))]
        public IActionResult GetRoles()
        {
            var roles = _roleService.GetRoles();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(roles);
        }

        [HttpGet("id/{roleId}")]
        [ProducesResponseType(200, Type = typeof(Role))]
        [ProducesResponseType(400)]
        public IActionResult GetRoleById(int roleId)
        {
            if (!_roleService.RoleIdExists(roleId))
            {
                return NotFound();
            }

            var role = _roleService.GetRoleById(roleId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(role);
        }

        [HttpGet("name/{roleName}")]
        [ProducesResponseType(200, Type = typeof(Role))]
        [ProducesResponseType(400)]
        public IActionResult GetRoleByName(string roleName)
        {
            if (!_roleService.RoleNameExists(roleName))
            {
                return NotFound();
            }

            var role = _roleService.GetRoleByName(roleName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(role);
        }

        [HttpGet("count")]
        [ProducesResponseType(200, Type = typeof(RoleChampionsCount))]
        [ProducesResponseType(400)]
        public IActionResult GetRoleChampionsCounts()
        {
            var roles = _roleService.GetRoleChampionsCounts();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(roles);
        }

        [HttpGet("count/{roleName}")]
        [ProducesResponseType(200, Type = typeof(RoleChampionsCount))]
        [ProducesResponseType(400)]
        public IActionResult GetRoleAndCountByName(string roleName)
        {
            var role = _roleService.GetRoleAndCountByName(roleName);

            if (!_roleService.RoleNameExists(roleName))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = "UserAllowed")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateRole([FromBody]  Role role)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(!_roleService.CreateRole(role))
            {
                ModelState.AddModelError("", "Something happened while adding role");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("id/{roleId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteRole(int roleId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_roleService.RoleIdExists(roleId))
                return BadRequest("The id " + roleId + " does not exist or was already deleted.");

            var roleToDelete = _roleService.GetRoleById(roleId);

            if(!_roleService.DeleteRole(roleToDelete))
            {
                ModelState.AddModelError("", "Something happened while deleting role");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpPatch("id/{roleId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateRole(int roleId, [FromBody] Role updatedRole)
        {
            if (updatedRole == null || updatedRole.Name == "")
                return BadRequest("Body cannot be empty " + ModelState);

            if (!_roleService.RoleIdExists(roleId))
                return BadRequest("Role does not exist " + ModelState);

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var roleMap = _roleService.GetRoleById(roleId);

            roleMap.Name = updatedRole.Name;

            if (!_roleService.UpdateRole(roleMap))
            {
                ModelState.AddModelError("", "Something went wrong while updating the role");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
