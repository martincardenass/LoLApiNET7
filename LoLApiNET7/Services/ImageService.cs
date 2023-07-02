using LoLApiNET7.Models;

namespace LoLApiNET7.Services
{
    public interface IImageService
    {
        ICollection<Images> GetImages();
        bool CreateImage(int ChampionId, Images image); // Create Img needs a championId (so the image can know to who champion it belongs) and the actual image
        bool DeleteImage(Images image);
        Images GetImageById(int ImageId);
        bool Save();
    }
    public class ImageService : IImageService
    {
        private readonly AppDbContext _context;
        private readonly ICIService _cIService;

        public ImageService(AppDbContext context, ICIService cIService)
        {
            _context = context;
            _cIService = cIService;
        }

        public bool CreateImage(int ChampionId, Images image)
        {
            _context.Add(image); // Add the image to the image table
            _context.SaveChanges(); // Save the changes otherwise we get an error
            
            var relation = new ChampionsImage() //This is so we know the relationship of the just uploaded image with the championId
            {
                Champion_Id = ChampionId,
                Image_Id = image.Image_Id
            };

            _context.Add(relation);  // Add the relationship to the relations table
            return Save();
        }

        public bool DeleteImage(Images image)
        {
            _context.Remove(_cIService.GetRelationByImageId(image.Image_Id)); // Deletes the relation
            
            _context.SaveChanges(); // Save the changes otherwise we get an error

            _context.Remove(image); // Deletes the actual image from the images table
            return Save();
        }

        public Images GetImageById(int ImageId)
        {
            return _context.Images.Where(i => i.Image_Id == ImageId).FirstOrDefault();
        }

        public ICollection<Images> GetImages()
        {
            return _context.Images.OrderBy(i => i.Image_Id).ToList();
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
