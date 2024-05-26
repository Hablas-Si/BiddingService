using BiddingService.Repositories;
using Microsoft.AspNetCore.Mvc;
using BiddingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiddingController : ControllerBase
    {
        private readonly ILogger<BiddingController> _logger;
        private readonly IConfiguration _config;
        private readonly IBiddingRepository _service;
        private readonly IVaultRepository _vaultService;

        public BiddingController(ILogger<BiddingController> logger, IConfiguration config, IBiddingRepository service, IVaultRepository vaultService)
        {
            _logger = logger;
            _config = config;
            _service = service;
            _vaultService = vaultService;
        }

        [HttpPost, Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> SubmitBid([FromBody] Bid bid)
        {
            try
            {
                bool bidAccepted = await _service.SubmitBid(bid);
                if (bidAccepted)
                {
                    return Ok("Bud modtaget");
                }
                else
                {
                    return BadRequest("Doublecheck venligst din indtastning. Bud skal være højere end det eksisterende, og du kan ikke byde på afsluttede auktioner");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting bid");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get/{auctionID}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuctionBids([FromRoute] Guid auctionID)
        {
            try
            {
                var bids = await _service.GetAuctionBids(auctionID);
                return Ok(bids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auction bids for Auction ID: {AuctionID}", auctionID);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("authorized"), Authorize(Roles = "Admin")]
        public IActionResult Authorized()
        {
            return Ok("Du har ret til at se denne ressource");
        }

        [AllowAnonymous]
        [HttpGet("getsecret/{path}")]
        public async Task<IActionResult> GetSecret(string path)
        {
            try
            {
                _logger.LogInformation($"Henter hemmelighed via {path}");
                var secretValue = await _vaultService.GetSecretAsync(path);
                if (secretValue != null)
                {
                    return Ok(secretValue);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kunne ikke hente hemmelighed: {Path}", path);
                return StatusCode(500, "Hemmelighed kunne ikke hentes.");
            }
        }
    }
}
