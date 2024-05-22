using BiddingService.Repositories;
using Microsoft.AspNetCore.Mvc;
using BiddingService.Models;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("bids")]
    public class BiddingController : ControllerBase
    {
        private readonly ILogger<BiddingController> _logger;
        private readonly IConfiguration _config;
        private readonly IBiddingRepository _service;

        public BiddingController(ILogger<BiddingController> logger,IConfiguration config, IBiddingRepository service)
        {
            _logger = logger;
            _config = config;
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBid([FromBody] Bid bid)
        {
            // Submit the bid
            bool bidAccepted = await _service.SubmitBid(bid);

            // Check if the bid was accepted
            if (bidAccepted)
            {
                // Return success response
                return Ok("Bid submitted");
            }
            else
            {
                // Return failure response
                return BadRequest("Please doublecheck your bid amount and the end date of the auction");
            }
        }

        [HttpGet("get/{auctionID}")]
        public async Task<IActionResult> GetAuctionBids([FromRoute] Guid auctionID)
        {
            var bids = await _service.GetAuctionBids(auctionID);
            return Ok(bids);
        }
    }
}