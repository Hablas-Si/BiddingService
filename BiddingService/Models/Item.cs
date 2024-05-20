namespace BiddingService.Models
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public bool? ProductAvailable { get; set; }
        public string? Seller { get; set; }

        public Item()
        { }
    }
}
