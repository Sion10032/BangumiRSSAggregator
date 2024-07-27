namespace BangumiRSSAggregator.Server.Models;

/// <summary>
/// 订阅源
/// </summary>
public class FeedSource
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    /// <summary>
    /// Update interval(min)
    /// </summary>
    public double UpdateInterval { get; set; }
    /// <summary>
    /// Last updated time
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    public ICollection<FeedItem> FeedItems { get; set; } = new List<FeedItem>();
    public ICollection<FeedRule> FeedRules { get; set; } = new List<FeedRule>();
    public ICollection<FeedGroup> FeedGroups { get; set; } = new List<FeedGroup>();
}
