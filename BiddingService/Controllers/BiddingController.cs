using BiddingService.Repositories;
using Microsoft.AspNetCore.Mvc;
using BiddingService.Models;
using Microsoft.AspNetCore.Authorization;

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
            // Submit the bid
            bool bidAccepted = await _service.SubmitBid(bid);

            // Check if the bid was accepted
            if (bidAccepted)
            {
                // Return success response
                return Ok("Bud modtaget");
            }
            else
            {
                // Return failure response
                return BadRequest("Doublecheck venligst din indtastning. Bud skal være højere end det eksisterende, og du kan ikke byde på afsluttede auktioner");
            }
        }


        [HttpGet("get/{auctionID}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuctionBids([FromRoute] Guid auctionID)
        {
            var bids = await _service.GetAuctionBids(auctionID);
            return Ok(bids);
        }

        //TEST ENVIRONMENT
        // OBS: TIlføj en Authorize attribute til metoderne nedenunder Kig ovenfor i jwt token creation.
        [HttpGet("authorized"), Authorize(Roles = "Admin")]
        public IActionResult Authorized()
        {

            // Hvis brugeren har en gyldig JWT-token og rollen "Admin", vil denne metode blive udført
            return Ok("Du har ret til at se denne ressource");
        }

        // En get der henter secrets ned fra vault
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
                _logger.LogError($"Kunne ikke hente hemmelighed: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Hemmelighed kunne ikke hentes.");
            }
        }
    }
}