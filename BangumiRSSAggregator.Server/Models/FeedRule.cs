namespace BangumiRSSAggregator.Server.Models;

public class FeedRule : EntityBase
{
    public int Id { get; set; }
    public required string Pattern {  get; set; }
    public required string Replacement { get; set; }

    public ICollection<FeedSource> FeedSources { get; set; } = new List<FeedSource>();
}
