using LoLApiNET7.Models;

namespace LoLApiNET7.Services
{
    public interface IReviewService
    {
        ICollection<Review> GetReviews();
        Review GetReviewById(int id);
        bool ReviewIdExists(int id);
        Review[] GetChampionReviews(int id);
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
                //string[] words = review.Text.Split(' '); //Separates until the first space
                //review.Title = words[0]; // assign the first word of text to the value of title
                string words = review.Text[..16];  //Gets the first 16 words
                //if(review.Text.Length > 16)
                //{
                //    review.Title = words + "...";
                //}
                review.Title = words + "..."; //asign the to the title if no title is provided
            }

            //string bearerToken = _accesor.HttpContext.Request.Headers.Authorization.ToString();
            //bearerToken = bearerToken.Replace("Bearer", "").Trim();

            var reviewInsert = new Review()
            {
                Rating = Rating, // Assign the rating. 0 - 5 TINYINT
                //User_Id = UserId, // Assign the user who is posting the review. WILL ADD AUTH LATER
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

        public Review GetReviewById(int id)
        {
            return _context.Reviews.Where(r => r.Review_id == id).FirstOrDefault();
        }

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
