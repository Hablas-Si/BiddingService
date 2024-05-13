using System.ComponentModel.DataAnnotations;

namespace BiddingService.Models;

public class Bid
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Der skal refereres til brugeren som afgiver buddet")]
    public Guid BidOwnerID { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Dit bud skal være mindst 0")]
    public int Amount { get; set; }
    [Required(ErrorMessage = "Udfyld venligst hvilken auktion du ønsker at byde på")]
    public Guid Auction {  get; set; }
    public bool Accepted { get; set; }

    // Constructor
    public Bid(Guid bidderID, int amount, Guid auction)
    {
        Id = Guid.NewGuid();
        BidOwnerID = bidderID;
        Amount = amount;
        Auction = auction;
        Accepted = false; // False som default. Skifter til true hvis validering gennemføres
    }
    public Bid()
    {
    }
}