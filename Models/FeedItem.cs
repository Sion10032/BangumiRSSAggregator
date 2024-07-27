namespace BangumiRSSAggregator.Server.Models;

/// <summary>
/// 从订阅源获取到的数据
/// </summary>
public class FeedItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string RawContent { get; set; }

    public int? FeedSourceId { get; set; }
    public FeedSource? FeedSource { get; set; }

    public ICollection<FeedGroup> FeedGroups { get; set; } = new List<FeedGroup>();
}
