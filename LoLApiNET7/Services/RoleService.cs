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
        bool CreateRole(Role role);
        bool DeleteRole(Role role);
        bool UpdateRole(Role role);
        bool Save();
    }

    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateRole(Role role)
        {
            var roleInsert = new Role()
            {
                Name = role.Name
            };

            _context.Add(roleInsert);
            return Save();
        }

        public bool DeleteRole(Role role)
        {
            _context.Remove(role);
            return Save();
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

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }

        public bool UpdateRole(Role role)
        {
            _context.Update(role);
            return Save();
        }
    }
}
