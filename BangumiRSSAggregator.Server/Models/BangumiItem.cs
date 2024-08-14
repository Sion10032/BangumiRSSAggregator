namespace BangumiRSSAggregator.Server.Models;

public class BangumiItem : EntityBase
{
    public required string FeedItemId { get; set; }
    public int FeedGroupId { get; set; }

    public FeedItem FeedItem { get; set; } = null!;
    public FeedGroup FeedGroup { get; set; } = null!;
}