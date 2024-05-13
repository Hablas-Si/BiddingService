using System.ComponentModel.DataAnnotations;

namespace BiddingService.Models;

public class Bid
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "BidderName skal udfyldes")]
    public string BidderName { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Dit bud skal v�re mindst 0")]
    public int Amount { get; set; }
    [Required(ErrorMessage = "Udfyld venligst hvilken auktion du �nsker at byde p�")]
    public Guid Auction {  get; set; }
    public bool Accepted { get; set; }

    // Constructor
    public Bid(string bidderName, int amount, Guid auction)
    {
        Id = Guid.NewGuid();
        BidderName = bidderName;
        Amount = amount;
        Auction = auction;
        Accepted = false; // False som default. Skifter til true hvis validering gennemf�res
    }
    public Bid()
    {
    }
}