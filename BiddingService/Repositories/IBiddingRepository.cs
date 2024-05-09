using BiddingService.Models;
namespace BiddingService.Repositories
{
    public interface IBiddingRepository
    {
        Task SubmitBid(Bid bid)
        {
            return Task.CompletedTask;
        }
    }
}