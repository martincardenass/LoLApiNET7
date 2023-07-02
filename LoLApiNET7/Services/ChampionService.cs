using LoLApiNET7.Models;
using Microsoft.EntityFrameworkCore;

namespace LoLApiNET7.Services
{
    public interface IChampionService
    {
        ICollection<Champion> GetChampions(); //Get the list of champions
        ICollection<ChampionInfo> GetChampionsInfo(); // Gets the VIEW of the champions. which contains its regions and roles names instead of its ids.
        ICollection<Champion> GetChampionsByRole(int Role_id); // Gets the champions by their role ID
        ICollection<ChampionInfo> GetChampionsByRoleName(string name);
        ICollection<ChampionInfo> GetChampionsByRegionName(string name);
        List<string> GetChampionImages(string name);
        Champion GetChampionById(int id); //Get a champion by its Id
        ChampionInfo GetChampionByName(string name); //Get a champion by its Name
        int GetChampionCount(string name);
        bool ChampionIdExists(int id);
        bool ChampionNameExists(string name);
        bool CreateChampion(int Region_id, int Role_id, Champion champion);
        bool DeleteChampion(Champion champion);
        bool UpdateChampion(int Region_id, int Role_id, Champion champion);
        bool Save();
    }

    public class ChampionService : IChampionService
    {
        //private IQueryable<Champion> JoinedChampionTables() < code for joining tables
        //{
        //    var JoinedTable = _context.Champions
        //        .Join(
        //            _context.Regions,
        //            c => c.Region_id,
        //            r => r.Region_id,
        //            (c, r) => new
        //            {
        //                Champion = c,
        //                Region = r
        //            }
        //        ).Join(
        //            _context.Roles,
        //            joined => joined.Champion.Role_id,
        //            role => role.Role_id,
        //            (joined, role) => new Champion
        //            {
        //                Champion_id = joined.Champion.Champion_id,
        //                Name = joined.Champion.Name,
        //                Release_date = joined.Champion.Release_date,
        //                Image = joined.Champion.Image,
        //                //Regions = joined.Region,
        //                //Roles = new List<Role> { role}
        //            }
        //        );
        //    return JoinedTable;
        //}

        private readonly AppDbContext _context;

        public ChampionService(AppDbContext context)
        {
            _context = context;
        }

        public bool ChampionIdExists(int id)
        {
            return _context.Champions.Any(c => c.Champion_id == id); //< this is better
            //return _context.Champions.Find(id) != null;
        }

        public bool ChampionNameExists(string name)
        {
            return _context.Champions.Any(c => EF.Functions.Like(c.Name, $"%{name}%"));
        }

        public Champion GetChampionById(int id)
        {
            //var champById = JoinedChampionTables().Where(c => c.Champion_id == id).FirstOrDefault();
            //return champById;
            return _context.Champions.Where(c => c.Champion_id == id).FirstOrDefault();
        }

        public ChampionInfo GetChampionByName(string name)
        {
            //return JoinedChampionTables().Where(cn => EF.Functions.Like(cn.Name, $"%{name}%")).FirstOrDefault();
            return _context.ChampionsInfo.Where(cn => EF.Functions.Like(cn.Name, $"%{name}%")).FirstOrDefault();
        }

        public ICollection<Champion> GetChampions()
        {
            //var champs = JoinedChampionTables().ToList();
            //return champs;
            return _context.Champions.OrderBy(c => c.Champion_id).ToList();
        }

        public bool CreateChampion(int RegionId, int RoleId, Champion champion)
        {
            if (string.IsNullOrEmpty(champion.Image)) //If no image is provided. Add a default one
                champion.Image = "https://i.imgur.com/MLq1YvK.jpg"; //< default image link

            var championInsert = new Champion()
            {
                Region_id = RegionId,
                Role_id = RoleId,
                Name = champion.Name,
                Image = champion.Image,
                Release_date = champion.Release_date
            };

            _context.Add(championInsert);
            return Save();
        }

        public bool Save()
        {
            //return _context.SaveChanges() > 0 ? true : false;
            return _context.SaveChanges() > 0;
        }

        public bool DeleteChampion(Champion champion)
        {
            _context.Remove(champion);
            return Save();
        }

        public bool UpdateChampion(int Region_id, int Role_id, Champion champion)
        {
            var existingChampion = _context.Champions.Find(champion.Champion_id);
            if (existingChampion == null)
                return false;

            existingChampion.Region_id = Region_id; // < set the regionId from query
            existingChampion.Role_id = Role_id; // < set the role id from query

            _context.Update(existingChampion);
            return Save();
        }

        public ICollection<ChampionInfo> GetChampionsInfo()
        {
            return _context.ChampionsInfo.OrderBy(ci => ci.Champion_Id).GroupBy(ci => ci.Champion_Id).Select(group => group.FirstOrDefault()).ToList();  // Filter repeated Ids
        }

        public ICollection<Champion> GetChampionsByRole(int Role_id)
        {
            return _context.Champions.Where(rid => rid.Role_id == Role_id).ToList();
        }

        public ICollection<ChampionInfo> GetChampionsByRoleName(string name) // Fails
        {
            return _context.ChampionsInfo.Where(rn => rn.Role_Name == name).GroupBy(n => n.Champion_Id).Select(g => g.FirstOrDefault()).ToList(); // Filter repeated Ids
        }

        public ICollection<ChampionInfo> GetChampionsByRegionName(string name)
        {
            return _context.ChampionsInfo.Where(rn => rn.Region_Name == name).GroupBy(n => n.Champion_Id).Select(g => g.FirstOrDefault()).ToList(); // Filter repeated Ids
        }

        public int GetChampionCount(string name)
        {
            return _context.ChampionsInfo.Where(n => n.Name == name).Count();
        }

        public List<string> GetChampionImages(string name) // List because we need indexing
        {
            return _context.ChampionsInfo.Where(i => i.Name == name).Select(i => i.Image).Take(GetChampionCount(name)).ToList(); // Will only return the image urls
        }
    }
}
