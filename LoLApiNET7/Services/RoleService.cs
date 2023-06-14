using LoLApiNET7.Models;
using Microsoft.EntityFrameworkCore;

namespace LoLApiNET7.Services
{
    public interface IRoleService
    {
        ICollection<Role> GetRoles();
        Role GetRoleById(int id);
        Role GetRoleByName(string name);
        bool RoleIdExists(int id);
        bool RoleNameExists(string name);
    }

    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public Role GetRoleById(int id)
        {
            return _context.Roles.Where(r => r.Role_id == id).FirstOrDefault();
        }

        public Role GetRoleByName(string name)
        {
            return _context.Roles.Where(rn => EF.Functions.Like(rn.Name, $"%{name}%")).FirstOrDefault();
        }

        public ICollection<Role> GetRoles()
        {
            return _context.Roles.OrderBy(r => r.Role_id).ToList();
        }

        public bool RoleIdExists(int id)
        {
            return _context.Roles.Any(r => r.Role_id == id);
        }

        public bool RoleNameExists(string name)
        {
            return _context.Roles.Any(r => EF.Functions.Like(r.Name, $"%{name}%"));
        }
    }
}
