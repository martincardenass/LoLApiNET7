using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        public IActionResult CreateReview([FromQuery] byte Rating, [FromQuery] int UserId, [FromQuery] int ChampionId, [FromBody] Review review)
        {
            //The JWT Token UserId should match the UserId from query to be able to post the review

            //We get the BEARER TOKEN from the HEADERS
            string bearerToken = Request.Headers.Authorization.ToString(); //Get the bearer token from the auth headers, convert it to string and allocate it
            bearerToken = bearerToken.Replace("Bearer", "").Trim();

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler(); //We create a token handler 
            JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(bearerToken); //Read the token

            string userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value; //extract the users id
            int userIdInt; //by default its not an int so we cant compare. 
            int.TryParse(userIdString, out userIdInt);

            if (UserId != userIdInt) //if the userId fromQuery does not match the userId from the token
                return StatusCode(401);

            if (!_userService.UserExistsId(UserId)) //checking if the user exists
                return BadRequest("The user does not exist");

            if (Rating > 5) // my tinyint max value is 5. check if greater than 5
                return BadRequest("Rating can only contain numbers in the range of 0 to 5");

            if (!_championService.ChampionIdExists(ChampionId))
                return BadRequest("The champion you're trying to review does not exist");

            if(!_reviewService.CreateReview(Rating, UserId, ChampionId, review))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while creating this review");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return NoContent();
        }
    }
}
