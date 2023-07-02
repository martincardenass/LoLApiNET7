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
        //ReviewView GetReviewByName(string name); // Get a review object by its name Not necessary anymore.
        ICollection<ReviewView> GetChampionReviewsByName(string name); // Review view to find champions reviews by its name
        bool NameHasReview(string name);
        bool CreateReview(byte Rating, int ChampionId, Review review);
        bool UpdateReview(int ReviewId, byte NewRating, Review review);
        bool DeleteReview(int ReviewId, Review review); // We need a ReviewId to get the UserId within it. And compare it to the token that is trying to delete it
        string GetToken(); // I might need to move this to the user service. I dont think it belongs here.
        bool CompareUserIds(int ReviewId);
        bool IsUserAdmin();
        bool Save();
    }
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _accesor;

        public ReviewService(AppDbContext context, IUserService userService, IHttpContextAccessor accesor)
        { // Injecting the services that we need. HttpContext its to get access to the http headers.
            _context = context;
            _userService = userService;
            _accesor = accesor;

        }

        public bool CompareUserIds(int ReviewId)
        {
            var reviewToUpdate = _context.Reviews.Find(ReviewId); // Find the review were going to update

            var userIdFromToken = _userService.DecodeToken(GetToken());
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
                User_Id = _userService.DecodeToken(GetToken()), //Gets the user directly from the bearer token
                Champion_id = ChampionId, // Assign the champion who is being reviewed
                Title = review.Title,
                Text = review.Text,
                Created = DateTime.Now, // Data of creation will be current date
            };

            _context.Add(reviewInsert);

            return Save();
        }

        public bool DeleteReview(int ReviewId, Review review) // I might not need "review" here since I am deleting from its ID.
        {
            var reviewToDelete = _context.Reviews.Find(ReviewId); // Get the review we want to delete

            // Check if the userId of the token its the same as the UserId in the Review, OR if the user is an admin
            if (CompareUserIds(ReviewId) == true || IsUserAdmin())
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
            //var IdFromName = GetReviewByName(name).Champion_Id; // Extract the Id of the chamnpion
            //var reviewer = GetReviewByName(name).Reviewer;

            //return _context.Reviews.Where(r => r.Champion_id == IdFromName).ToArray(); // Find the reviews with the championId
            //// Using the name string didnt worked in the view. Why? Idk. But this approach fixed it
            /// Fixed the error it was because I accidentaly setted the champion_id as the primary key of the view which ofc broke it
            return _context.ReviewView.Where(n => n.Name == name).ToArray();
        }

        public Review GetReviewById(int id)
        {
            return _context.Reviews.Where(r => r.Review_id == id).FirstOrDefault();
        }

        //public ReviewView GetReviewByName(string name) // Need to create this to extract the champion_id from here
        //{ Not necessary anymore
        //    return _context.ReviewView.Where(n => n.Name == name).FirstOrDefault(); // Only returns one. Does not matter. I just want to extract the champion_Id
        //}

        public ICollection<Review> GetReviews()
        {
            return _context.Reviews.OrderBy(r => r.Review_id).ToList();
        }

        public string GetToken()
        {
            string bearerToken = _accesor.HttpContext.Request.Headers.Authorization.ToString(); // Get the token from the Authorization Headers
            bearerToken = bearerToken.Replace("Bearer", "").Trim(); // Its gote some characters that we dont need. We remove them

            return bearerToken; // Return the token
        }

        public bool IsUserAdmin()
        {
            string userRoleFromToken = _userService.DecodeTokenRole(GetToken()).ToString(); // Gets a string that contains either user or admin

            if (userRoleFromToken == "admin")
                return true; // User is an admin
            else 
                return false; // User its not an admin

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

            if (CompareUserIds(ReviewId) == true || IsUserAdmin()) // If the comparasion between the Ids results false
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
