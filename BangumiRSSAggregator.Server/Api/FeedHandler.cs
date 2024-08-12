using BangumiRSSAggregator.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Api;

public static class FeedHandler
{
    public static RouteGroupBuilder MapFeedApis(this RouteGroupBuilder group)
    {
        group.MapGet("/meta", GetFeedInfo);
        group.MapGet("/{id}/fetch", Fetch);
        group.MapGet("/{id}/fetch-and-update", FetchAndUpdateFeed);
        group.MapPost("/{id}/test-rule", TestFeedRule);

        return group;
    }

    private static Task<FeedInfoResponse> GetFeedInfo(
        [FromQuery] string url)
    {
        return RSSUpdater.GetFeedInfo(url);
    }

    private static async Task<bool> Fetch(
        [FromRoute] int id,
        [FromServices] RSSUpdater updater)
    {
        var result = await updater.FetchFeed(id);
        return result.Feed != null;
    }

    private static Task<bool> FetchAndUpdateFeed(
        [FromRoute] int id,
        [FromServices] RSSUpdater updater)
    {
        return updater.FetchAndUpdate(id);
    }

    private static async Task<Dictionary<string, List<FeedItem>>> TestFeedRule(
        [FromRoute] int id,
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
