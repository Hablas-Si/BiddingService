namespace BiddingService.Models
{
    public class HighBid
    {
        public Guid AuctionId { get; set; }
        public string? userName { get; set; }
        public int Amount { get; set; }

        public HighBid(string user, int amount, Guid auctionId) //Used inBidMessage
        {
            AuctionId = auctionId;
            userName = user;
            Amount = amount;
        }
        public HighBid(string user, int amount) //Stored in auction to define highest bid
        {
            userName = user;
            Amount = amount;
        }
        public HighBid()
        { }
    }
}
