using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IUserService _userService;
        private readonly IChampionService _championService;

        public ReviewController(IReviewService reviewService, IUserService userService, IChampionService championService)
        { //injecting userService as well to verify if a user exists before posting the review
            //also inject championService to verify if a champion exists before posting its review
            _reviewService = reviewService;
            _userService = userService;
            _championService = championService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        public IActionResult GetReviews()
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviews = _reviewService.GetReviews();

            return Ok(reviews);
        }

        [HttpGet("id/{reviewId}")]
        [ProducesResponseType(200, Type = typeof(Review))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewById(int reviewId)
        {
            if (!_reviewService.ReviewIdExists(reviewId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = _reviewService.GetReviewById(reviewId);

            return Ok(review);
        }

        [HttpGet("championReview/{championId}")]
        [ProducesResponseType(200, Type = typeof(Review))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsByChampion(int championId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_reviewService.GetChampionReviews(championId).Length == 0) //If array is empty
                return NotFound();

            var reviewsOfAChamp = _reviewService.GetChampionReviews(championId);

            return Ok(reviewsOfAChamp);
        }

        [HttpPost]
        [Authorize(Policy = "UserAllowed")] // Only logged users can post reviews
        [ProducesResponseType(204)] // No content response type
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)]
        public IActionResult CreateReview([FromQuery] byte Rating, [FromQuery] int ChampionId, [FromBody] Review review)
        {
            if (string.IsNullOrEmpty(review.Text) || review.Text.Length < 16)
                return BadRequest("Review lenght must be at least 16 characters");

            if (Rating > 5) // my tinyint max value is 5. check if greater than 5
                return BadRequest("Rating can only contain numbers in the range of 0 to 5");

            if (!_championService.ChampionIdExists(ChampionId)) // check if the champion to be reviewed exists.
                return BadRequest("The champion you're trying to review does not exist");

            if(!_reviewService.CreateReview(Rating, ChampionId, review))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while creating this review");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return NoContent();
        }

        [HttpPatch("{reviewId}")]
        [Authorize(Policy = "UserAllowed")] // Only logged users can edit their reviews
        [ProducesResponseType(204)] // No content response type
        [ProducesResponseType(400)] // Bad request
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult UpdateReview(int ReviewId, [FromQuery] byte NewRating, [FromBody] Review updatedReview)
        {
            if (ReviewId == 0)
                return BadRequest("Please provide a review id");

            if (!_reviewService.ReviewIdExists(ReviewId))
            {
                var errorMsg = $"The review {ReviewId} does not exist";
                return NotFound(new { Message = errorMsg });
            }
            
            string ratingFromQuery = HttpContext.Request.Query["NewRating"]; // Gets the NewRating value from query as a string
            var reviewMap = _reviewService.GetReviewById(ReviewId); // Getting the original review before its modified

            if (ratingFromQuery == null) // If no value has been provided in the query for the NewRating
                NewRating = reviewMap.Rating; // Assign its old value

            reviewMap.Title = updatedReview.Title ?? reviewMap.Title; // Set the review title to its new value, if not provided, keep the old value
            reviewMap.Text = updatedReview.Text ?? reviewMap.Text; // Set the review text to its new value, if not provided, keep the old value
            reviewMap.Rating = NewRating;

            if(!_reviewService.UpdateReview(ReviewId, NewRating, updatedReview))
            {
                ModelState.AddModelError("", "Something went wrong while updating the review");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return NoContent();
        }

        [HttpDelete("id/{reviewId}")]
        [Authorize(Policy = "UserAllowed")] // Only logged users can edit their reviews
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteReview(int reviewId)
        {
            if (!_reviewService.ReviewIdExists(reviewId))
                return NotFound();

            var reviewToDelete = _reviewService.GetReviewById(reviewId);

            if (!_reviewService.DeleteReview(reviewId, reviewToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while updating the review");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return NoContent();
        }
    }
}
