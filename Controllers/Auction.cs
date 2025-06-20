public class Auction
{
    public int AuctionID { get; set; }
    public int UserID { get; set; }
    public string? ItemName { get; set; }
    public string? DescriptionName { get; set; }
    public int StartingBid { get; set; }
    public int BidEndTime { get; set; }
}