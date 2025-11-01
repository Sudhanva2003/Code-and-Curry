using Microsoft.AspNetCore.Mvc;
using Code_Curry.Services;  // Import the DistanceService
using System.Threading.Tasks;

namespace Code_Curry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistanceController : ControllerBase
    {
        private readonly DistanceService _distanceService;

        // Inject DistanceService into the controller
        public DistanceController(DistanceService distanceService)
        {
            _distanceService = distanceService;
        }

        // GET api/distance/calculate?address1=New%20York,%20NY&address2=Los%20Angeles,%20CA
        [HttpGet("calculate")]
        public async Task<ActionResult<int>> GetDistance([FromQuery] string address1, [FromQuery] string address2)
        {
            // Validate input addresses
            if (string.IsNullOrEmpty(address1) || string.IsNullOrEmpty(address2))
            {
                return BadRequest("Both addresses must be provided.");
            }

            try
            {
                // Use the DistanceService to calculate the distance
                int distance = await _distanceService.GetDistanceAsync(address1, address2);
                return Ok(distance);  // Return the distance in kilometers
            }
            catch (Exception ex)
            {
                // Handle any errors and return an error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}