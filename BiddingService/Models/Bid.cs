using System.ComponentModel.DataAnnotations;

namespace BiddingService.Models;

public class Bid
{
    [Required(ErrorMessage = "BidderName skal udfyldes")]
    public string BidderName { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Dit bud skal være mindst 0")]
    public int Amount { get; set; }

    // Constructor
    public Bid(string bidderName, int amount)
    {
        BidderName = bidderName;
        Amount = amount;
    }
    public Bid()
    {
    }
}