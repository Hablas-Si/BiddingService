namespace BiddingService.Models
{
    //Class used by rabbitMQ to send bids to AuctionService
    public class BidMessage
    {
        public Guid AuctionId { get; set; }
        public int Amount { get; set; }
        public string Bidder { get; set; }
    }
}
