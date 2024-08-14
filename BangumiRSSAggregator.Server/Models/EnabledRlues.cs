namespace BangumiRSSAggregator.Server.Models;

public class EnabledRule : EntityBase
{
    public int FeedRuleId { get; set; }
    public int FeedSourceId { get; set; }

    public FeedRule FeedRule { get; set; } = null!;
    public FeedSource FeedSource { get; set; } = null!;
}
