using LoLApiNET7.Models;

namespace LoLApiNET7.Services
{
    public interface ICIService // Champions Image Service
    {
        ICollection<ChampionsImage> GetImagesByChampionId(int id);
        ChampionsImage GetRelationByImageId(int id);
        bool CreateRelation(int ChampionId, int ImageId);
        bool Save();
    }
    public class CIService : ICIService
    {
        private readonly AppDbContext _context;

        public CIService(AppDbContext context)
        {
            _context = context;
        }

        //Might not need this but idc
        public bool CreateRelation(int ChampionId, int ImageId)
        {
            var relation = new ChampionsImage()
            {
                Champion_Id = ChampionId,
                Image_Id = ImageId
            };

            _context.Add(relation);
            return Save();
        }

        public ICollection<ChampionsImage> GetImagesByChampionId(int id)
        {
            return _context.ChampionsImage.Where(ci => ci.Champion_Id == id).ToList();
        }

        public ChampionsImage GetRelationByImageId(int id)
        {
            return _context.ChampionsImage.Where(i => i.Image_Id == id).FirstOrDefault();
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
