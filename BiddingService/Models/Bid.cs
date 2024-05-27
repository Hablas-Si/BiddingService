using System.ComponentModel.DataAnnotations;

namespace BiddingService.Models;

public class Bid
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Der skal refereres til brugeren som afgiver buddet")]
    public string BidOwner { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Dit bud skal være mindst 0")]
    public int Amount { get; set; }
    [Required(ErrorMessage = "Udfyld venligst hvilken auktion du ønsker at byde på")]
    public Guid Auction {  get; set; }
    public bool Accepted { get; set; } = false;
    public DateTime Created { get; set; } = DateTime.UtcNow.AddHours(2); //There is a better way to do this, but this is easier. See TimeZoneInfo.ConvertTimeFromUtc

    // Constructor
    public Bid(string userID, int amount, Guid auction)
    {
        Id = Guid.NewGuid();
        BidOwner = userID;
        Amount = amount;
        Auction = auction;
    }
    public Bid()
    {
    }
}