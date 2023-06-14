using LoLApiNET7.Dto;
using LoLApiNET7.Models;
using Microsoft.EntityFrameworkCore;

namespace LoLApiNET7.Services
{
    public interface IChampionService
    {
        ICollection<Champion> GetChampions(); //Get the list of champions
        Champion GetChampionById(int id); //Get a champion by its Id
        Champion GetChampionByName(string name); //Get a champion by its Name
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
            //return _context.Champions.Any(c => c.Champion_id == id); < this does the same thing
            return _context.Champions.Find(id) != null;
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

        public Champion GetChampionByName(string name)
        {
            //return JoinedChampionTables().Where(cn => EF.Functions.Like(cn.Name, $"%{name}%")).FirstOrDefault();
            return _context.Champions.Where(cn => EF.Functions.Like(cn.Name, $"%{name}%")).FirstOrDefault();
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
            return _context.SaveChanges() > 0 ? true : false;
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

            existingChampion.Region_id = Region_id; // < set the regioId from query
            existingChampion.Role_id = Role_id; // < set the role id from query

            _context.Update(existingChampion);
            return Save();
        }
    }
}
