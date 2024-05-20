using BiddingService.Models;
namespace BiddingService.Repositories
{
    public interface IBiddingRepository
    {
        //Task SubmitBid(Bid bid, Guid auctionID);
        bool ValidateBid(Bid bid);
        Task<List<Bid>> GetAuctionBids(Guid auctionID);
        Task<bool> SubmitBid(Bid bid);
    }
}