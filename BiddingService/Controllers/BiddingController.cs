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
        public IActionResult PostBid([FromBody] Bid bid)
        {
            // Check if the bid is null or invalid
            if (bid.BidOwnerID == null || !ModelState.IsValid)
            {
                return BadRequest("Husk, at dit bud skal indeholde både BidderName og Amount. Se dokumentationen for yderligere info");
            }

            // Submit the bid
            _service.SubmitBid(bid);

            // Return success response
            return Ok("Bid submitted");
        }
        [HttpPost("get/{auctionID}")]
        public async Task<IActionResult> GetAuctionBids([FromRoute] Guid auctionID)
        {
            var bids = await _service.GetAuctionBids(auctionID);
            return Ok(bids);
        }
    }
}