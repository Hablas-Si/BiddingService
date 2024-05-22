using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http.HttpResults;
using static MongoDB.Driver.WriteConcern;

namespace BiddingService.Models
{
    public class Auction
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public HighBid? HighBid { get; set; } // Holds the highest current bid
        public Item? Item { get; set; }

        public Auction(DateTime start, DateTime slut, Item item)
        {
            Id = Guid.NewGuid();
            Start = start;
            End = slut;
            Item = item;
            HighBid = new HighBid("Initial Price", item.Price);
            
        }
        public Auction()
        { }
    }
   
}
