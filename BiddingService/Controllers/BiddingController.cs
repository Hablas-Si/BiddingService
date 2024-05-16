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

        [HttpPost("{auctionID}")]
        public IActionResult SubmitBid(Guid auctionID, [FromBody] Bid bid)
        {
            // Submit the bid
            _service.SubmitBid(bid, auctionID);

            // Return success response
            return Ok("Bid submitted");
        }

        [HttpGet("get/{auctionID}")]
        public async Task<IActionResult> GetAuctionBids([FromRoute] Guid auctionID)
        {
            var bids = await _service.GetAuctionBids(auctionID);
            return Ok(bids);
        }
    }
}