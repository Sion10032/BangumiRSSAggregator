using BangumiRSSAggregator.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Api;

public static class FeedHandler
{
    public static RouteGroupBuilder MapFeedApis(this RouteGroupBuilder group)
    {
        group.MapGet("/{id}/fetch_and_update", FetchAndUpdateFeed);
        group.MapGet("/meta", GetFeedInfo);
        group.MapPost("/{id}/test-rule", TestFeedRule);

        //group.MapGet("/{id}", GetFeed);

        return group;
    }

    private static Task<bool> FetchAndUpdateFeed(
        [FromRoute] int id,
        [FromServices] RSSUpdater updater)
    {
        return updater.FetchAndUpdate(id);
    }

    private static Task<FeedInfoResponse> GetFeedInfo(
        [FromQuery] string url)
    {
        return RSSUpdater.GetFeedInfo(url);
    }

    private static async Task<Dictionary<string, List<FeedItem>>> TestFeedRule(
        [FromQuery] int id,
        [FromBody] FeedTestRuleRequest testRule,
        [FromServices] BangumiDb db)
    {
        var feedItems = db.FeedItems.Where(it => it.FeedSourceId == id).ToList();
        return RSSUpdater
            .Grouping(
                feedItems,
                [
                    new FeedRule { Pattern = testRule.Pattern, Replacement = testRule.Replacement }
                ])
            .ToDictionary(
                it => it.Key,
                it => it.Value.Select(it => it.FeedItem).ToList());
    }

    private static async Task<FeedSource?> GetFeed(
        int id,
        [FromServices] BangumiDb db)
    {
        var feed = await db.FeedSources.FirstOrDefaultAsync(x => x.Id == id);
        if (feed == null)
        {
            return null;
        }

        await db.Entry(feed)
            .Collection(item => item.FeedRules)
            .LoadAsync();

        await db.Entry(feed)
            .Collection(item => item.FeedItems)
            .Query()
            .OrderByDescending(x => x.Id)
            .Take(10)
            .ToListAsync();

        return feed;
    }
}
