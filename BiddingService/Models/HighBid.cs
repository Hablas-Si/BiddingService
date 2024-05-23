namespace BiddingService.Models
{
    public class HighBid
    {
        public Guid AuctionId { get; set; }
        public string? userName { get; set; }
        public int Amount { get; set; }

        public HighBid(string user, int amount, Guid auctionId)
        {
            AuctionId = auctionId;
            userName = user;
            Amount = amount;
        }
        public HighBid(string user, int amount)
        {
            userName = user;
            Amount = amount;
        }
        public HighBid()
        { }
    }
}
