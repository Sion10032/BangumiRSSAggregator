namespace BangumiRSSAggregator.Server.Api;

public record FeedInfoResponse(string Link, string Title, string Description);

public record BangumiItemResponse(string Id, string Name, string Url, int GroupId);
