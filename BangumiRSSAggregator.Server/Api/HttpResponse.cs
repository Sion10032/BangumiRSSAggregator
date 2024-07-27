namespace BangumiRSSAggregator.Server.Api;

public record HttpResponse<T>(bool Success, string Message, T Data);
