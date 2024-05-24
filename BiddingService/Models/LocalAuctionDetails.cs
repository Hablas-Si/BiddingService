namespace BiddingService.Models
{
    //Info pulled from and saved in cache-based storage
    public class LocalAuctionDetails
    {
        public int HighestBid { get; set; }
        public DateTime EndTime { get; set; }
    }
}
