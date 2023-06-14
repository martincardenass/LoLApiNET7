using LoLApiNET7.Models;
using Microsoft.EntityFrameworkCore;

namespace LoLApiNET7.Services
{
    public interface IRegionService
    {
        ICollection<Region> GetRegions(); //Get the list of regions
        Region GetRegionById(int id); //Gets a region by its Id
        Region GetRegionByName(string name); //Gets a region by its name
        bool RegionIdExists(int id); //Checks if the region exists by its Id
        bool RegionNameExists(string name); //Check if the region exists by its name
    }

    public class RegionService : IRegionService
    {
        private readonly AppDbContext _context;

        public RegionService(AppDbContext context)
        {
            _context = context;
        }

        public Region GetRegionById(int id)
        {
            return _context.Regions.Where(r => r.Region_id == id).FirstOrDefault();
        }

        public Region GetRegionByName(string name)
        {
            return _context.Regions.Where(rn => EF.Functions.Like(rn.Name, $"%{name}%")).FirstOrDefault();
        }

        public ICollection<Region> GetRegions()
        {
            return _context.Regions.OrderBy(r => r.Region_id).ToList();
        }

        public bool RegionIdExists(int id)
        {
            return _context.Regions.Any(r => r.Region_id == id); //return true or false
        }

        public bool RegionNameExists(string name)
        {
            return _context.Regions.Any(r => EF.Functions.Like(r.Name, $"%{name}%")); // return true or false
        }
    }
}
