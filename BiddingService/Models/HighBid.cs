namespace BiddingService.Models
{
    public class HighBid
    {
        public string? userName { get; set; }
        public int Amount { get; set; }

        public HighBid(string user, int amount)
        {
            userName = user;
            Amount = amount;
        }
        public HighBid()
        { }
    }
}
