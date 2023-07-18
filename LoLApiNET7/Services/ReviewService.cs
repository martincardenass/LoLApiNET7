using LoLApiNET7.Models;
using Microsoft.EntityFrameworkCore;

namespace LoLApiNET7.Services
{
    public interface IReviewService
    {
        ICollection<Review> GetReviews();
        Review GetReviewById(int id);
        bool ReviewIdExists(int id);
        Review[] GetChampionReviews(int id);
        ICollection<ReviewView> GetChampionReviewsByName(string name); // Review view to find champions reviews by its name
        ReviewView GetReviewsById(int id);
        ReviewView[] GetReviewsByUser(string username); 
        bool NameHasReview(string name);
        bool CreateReview(byte Rating, int ChampionId, Review review);
        bool CreateReviewWithChampionName(byte rating, string championName, Review review);
        bool UpdateReview(int ReviewId, byte NewRating, Review review);
        bool DeleteReview(int ReviewId, Review review); // We need a ReviewId to get the UserId within it. And compare it to the token that is trying to delete it
        bool CompareUserIds(int ReviewId);
        bool Save();
    }
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IChampionService _championService;

        public ReviewService(AppDbContext context, IUserService userService, IChampionService championService) // Injecting the services that we need
        {
            _context = context;
            _userService = userService;
            _championService = championService;
        }

        public bool CompareUserIds(int ReviewId)
        {
            var reviewToUpdate = _context.Reviews.Find(ReviewId); // Find the review were going to update

            var userIdFromToken = _userService.DecodeToken(_userService.GetToken());
            var userIdFromReview = reviewToUpdate.User_Id;

            if (userIdFromReview != userIdFromToken) // If the userId contained in the review does not match the userid contained in the token
                return false; // return false, which will cause a server error because its expecting true
            else
                return true; //If they do match, return true
        }

        public bool CreateReview(byte Rating, int ChampionId, Review review)
        {
            if (string.IsNullOrEmpty(review.Title)) //If no title for the review is provided
            {
                string words = review.Text[..16];  //Gets the first 16 words
                review.Title = words + "..."; //asign the to the title if no title is provided
            }

            var reviewInsert = new Review()
            {
                Rating = Rating, // Assign the rating. 0 - 5 TINYINT
                User_Id = _userService.DecodeToken(_userService.GetToken()), //Gets the user directly from the bearer token
                Champion_id = ChampionId, // Assign the champion who is being reviewed
                Title = review.Title,
                Text = review.Text,
                Created = DateTime.Now, // Data of creation will be current date
            };

            _context.Add(reviewInsert);

            return Save();
        }

        public bool CreateReviewWithChampionName(byte rating, string championName, Review review)
        {
            var champion = _championService.GetChampionByName(championName); // Gets the champion object by its name

            if (string.IsNullOrEmpty(review.Title))
            {
                string words = review.Text[..16]; 
                review.Title = words + "...";
            }

            var reviewInsert = new Review()
            {
                Rating = rating,
                User_Id = _userService.DecodeToken(_userService.GetToken()),
                Champion_id = champion.Champion_Id, // Extract the ID from the champion object of above
                Title = review.Title,
                Text = review.Text,
                Created = DateTime.Now
            };

            _context.Add(reviewInsert);

            return Save();
        }

        public bool DeleteReview(int ReviewId, Review review) // I might not need "review" here since I am deleting from its ID.
        {
            var reviewToDelete = _context.Reviews.Find(ReviewId); // Get the review we want to delete

            // Check if the userId of the token its the same as the UserId in the Review, OR if the user is an admin
            if (CompareUserIds(ReviewId) == true || _userService.IsUserAdmin())
            {
                _context.Remove(reviewToDelete); // If any of the two conditions are met. Delete.
                return Save(); // And save.
            }
            else
                return false; // If none of the two are met, return false and will cause a 500 server error. Nothing will be deleted.
        }

        public Review[] GetChampionReviews(int id)
        {
            return _context.Reviews.Where(r => r.Champion_id == id).ToArray(); //Returns an array of the reviews of a certain champion.
        }

        public ICollection<ReviewView> GetChampionReviewsByName(string name) // Insert the champions name
        {
            return _context.ReviewView.Where(n => n.Name == name).ToArray();
        }

        public Review GetReviewById(int id)
        {
            return _context.Reviews.Where(r => r.Review_id == id).FirstOrDefault();
        }

        public ICollection<Review> GetReviews()
        {
            return _context.Reviews.OrderBy(r => r.Review_id).ToList();
        }

        public ReviewView GetReviewsById(int id)
        {
            return _context.ReviewView.Where(r => r.Review_Id == id).FirstOrDefault();
        }

        public ReviewView[] GetReviewsByUser(string username)
        {
            return _context.ReviewView.Where(u => u.Reviewer == username).ToArray();
        }

        public bool NameHasReview(string name) // Check if a Review_Title value of a champ name is null, if it is, the champion does not have reviews.
        {
            return _context.ReviewView.Where(n => n.Name == name).Any(r => r.Review_Title != null);
        }

        public bool ReviewIdExists(int id)
        {
            return _context.Reviews.Find(id) != null;
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }

        public bool UpdateReview(int ReviewId, byte NewRating, Review review)
        {
            var reviewToUpdate = _context.Reviews.Find(ReviewId); // Find the review were going to update

            if (CompareUserIds(ReviewId) == true || _userService.IsUserAdmin()) // If the comparasion between the Ids results false
            {
                reviewToUpdate.Rating = NewRating; // Set the new rating

                _context.Update(reviewToUpdate);
                return Save();
            }
            else
                return false; // Return false which will cause a server error
        }
    }
}
