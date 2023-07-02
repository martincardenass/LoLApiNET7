using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : Controller
    {
        private readonly IImageService _imageService; // Image service to upload the image
        private readonly ICIService _ciService;
        private readonly IChampionService _championService; // Champion service for some validations

        public ImageController(IImageService imageService, ICIService ciService, IChampionService championService)
        {
            _imageService = imageService;
            _ciService = ciService;
            _championService = championService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Images>))]
        public IActionResult GetImages()
        {
            var images = _imageService.GetImages();

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(images);
        }

        [HttpGet("/relations/{championId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChampionsImage>))]
        public IActionResult GetImagesByChampionId(int championId)
        {
            var relations = _ciService.GetImagesByChampionId(championId);

            return Ok(relations);
        }

        [HttpPost] // Add auth later
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateImage([FromQuery] int ChampionId, [FromBody] Images image)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_championService.ChampionIdExists(ChampionId)) // If desired champion does not exist
                return NotFound();

            if(!_imageService.CreateImage(ChampionId, image))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while adding the image");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("id/{imageId}")] // Add auth later
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteImage(int imageId)
        {
            //var championRelation = _ciService.GetRelationById()
            if (!ModelState.IsValid)
                return BadRequest();

            var imageToDelete = _imageService.GetImageById(imageId);

            if(!_imageService.DeleteImage(imageToDelete))
            {
                ModelState.AddModelError("", "Sorry. Something went wrong while deleting the image");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //[HttpGet("relationbyimageid")] // Works as expected. Delete this
        //public IActionResult GetRelationByImageId(int id)
        //{
        //    var relation = _ciService.GetRelationByImageId(id);

        //    return Ok(relation);
        //}
    }
}
