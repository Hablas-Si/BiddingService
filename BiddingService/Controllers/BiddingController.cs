using BiddingService.Repositories;
using Microsoft.AspNetCore.Mvc;
using BiddingService.Models;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiddingController : ControllerBase
    {
        private readonly ILogger<BiddingController> _logger;
        private readonly IBiddingRepository _service;

        public BiddingController(ILogger<BiddingController> logger, IBiddingRepository service)
        {
            _logger = logger;
            this._service = service;
        }

        [HttpPost]
        public IActionResult PostBid([FromBody] Bid bid)
        {
            // Check if the bid is null or invalid
            if (bid.BidderName == null || !ModelState.IsValid)
            {
                return BadRequest("Husk, at dit bud skal indeholde både BidderName og Amount. Se dokumentationen for yderligere info");
            }

            // Submit the bid
            _service.SubmitBid(bid);

            // Return success response
            return Ok("Bid submitted successfully");
        }
    }
}