using LoLApiNET7.Models;
using LoLApiNET7.Services;
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
    }
}
