namespace BangumiRSSAggregator.Server.Models;

public class FeedGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool Enabled { get; set; }

    public ICollection<FeedSource> FeedSources { get; set; } = new List<FeedSource>();
    public ICollection<FeedItem> FeedItems { get; set; } = new List<FeedItem>();
}
