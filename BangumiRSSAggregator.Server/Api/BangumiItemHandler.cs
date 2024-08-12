
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Api;

public static class BangumiItemHandler
{
    public static RouteGroupBuilder MapBangumiItemApis(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetBangumiItems);

        return group;
    }

    private static async Task<IReadOnlyList<BangumiItemResponse>> GetBangumiItems(
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromServices] BangumiDb db)
    {
        var bangumiItems = await db.BangumiItems
            .Include(it => it.FeedItem)
            .OrderByDescending(it => it.FeedItem.Id) // todo 按时间排序
            .Skip((page ?? 0) * (size ?? 50))
            .Take(size ?? 50)
            .ToListAsync();
        
        return bangumiItems
            .Select(it => new BangumiItemResponse(
                it.FeedItem.Id, it.FeedItem.Name, 
                it.FeedItem.Url, it.FeedGroupId))
            .ToList();
    }
}