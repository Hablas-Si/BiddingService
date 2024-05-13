using BiddingService.Models;
namespace BiddingService.Repositories
{
    public interface IBiddingRepository
    {
        Task SubmitBid(Bid bid);
        bool ValidateBid(Bid bid);
        Task<List<Bid>> GetAuctionBids(Guid auctionID);

    }
}