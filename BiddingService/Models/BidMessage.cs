namespace BiddingService.Models
{
    public class BidMessage
    {
        public Guid AuctionId { get; set; }
        public int Amount { get; set; }
        public string Bidder { get; set; }
    }
}
