using LoLApiNET7.Models;

namespace LoLApiNET7.Services
{
    public interface IReviewService
    {
        ICollection<Review> GetReviews();
        Review GetReviewById(int id);
        bool ReviewIdExists(int id);
        Review[] GetChampionReviews(int id);
        bool CreateReview(byte Rating, int UserId, int ChampionId, Review review);
        bool Save();
    }
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateReview(byte Rating, int UserId, int ChampionId, Review review)
        {
            if(string.IsNullOrEmpty(review.Title)) //If no title for the review is provided
            {
                string[] words = review.Text.Split(' '); //Separates until the first space
                review.Title = words[0]; // assign the first word of text to the value of title
            }

            var reviewInsert = new Review()
            {
                Rating = Rating, // Assign the rating. 0 - 5 TINYINT
                User_Id = UserId, // Assign the user who is posting the review. WILL ADD AUTH LATER
                Champion_id = ChampionId, // Assign the champion who is being reviewed
                Title = review.Title,
                Text = review.Text,
                Created = DateTime.Now, // Data of creation will be current date
            };

            _context.Add(reviewInsert);
            return Save();
        }

        public Review[] GetChampionReviews(int id)
        {
            return _context.Reviews.Where(r => r.Champion_id == id).ToArray(); //Returns an array of the reviews of a certain champion.
        }

        public Review GetReviewById(int id)
        {
            return _context.Reviews.Where(r => r.Review_id == id).FirstOrDefault();
        }

        public ICollection<Review> GetReviews()
        {
            return _context.Reviews.OrderBy(r => r.Review_id).ToList();
        }

        public bool ReviewIdExists(int id)
        {
            return _context.Reviews.Find(id) != null;
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
