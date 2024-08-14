namespace BangumiRSSAggregator.Server.Models;

public class SourceGroup : EntityBase
{
    public int FeedSourceId { get; set; }
    public int FeedGroupId { get; set; }

    public int RuleId { get; set; }

    public FeedSource FeedSource { get; set; } = null!;
    public FeedGroup FeedGroup { get; set; } = null!;
}
