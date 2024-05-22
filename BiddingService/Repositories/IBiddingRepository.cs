using BiddingService.Models;
namespace BiddingService.Repositories
{
    public interface IBiddingRepository
    {
        Task<List<Bid>> GetAuctionBids(Guid auctionID);
        Task<bool> SubmitBid(Bid bid);
    }
}