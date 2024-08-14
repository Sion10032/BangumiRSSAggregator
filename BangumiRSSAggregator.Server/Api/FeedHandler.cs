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
        group.MapPost("/{id}/add-rule", AddFeedRule);

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

    private static Task<Dictionary<string, List<FeedItem>>> TestFeedRule(
        [FromRoute] int id,
        [FromBody] FeedTestRuleRequest testRule,
        [FromServices] RSSUpdater rssUpdater)
    {
        return rssUpdater.TestFeedRule(id, testRule.Pattern, testRule.Replacement);
    }

    private static async Task AddFeedRule(
        [FromRoute] int id,
        [FromBody] FeedTestRuleRequest rule,
        [FromServices] BangumiDb db)
    {
        // 优先使用已经存在的规则
        var feedRule = await db.FeedRules
            .Where(e => e.Pattern == rule.Pattern)
            .Where(e => e.Replacement == rule.Replacement)
            .FirstOrDefaultAsync();
        if (feedRule == null)
        {
            feedRule = new FeedRule
            {
                Pattern = rule.Pattern,
                Replacement = rule.Replacement,
            };
        }

        db.FeedRules.Add(feedRule);
        db.EnabledRules.Add(new EnabledRule
        {
            FeedSourceId = id,
            FeedRule = feedRule,
        });
        await db.SaveChangesAsync();

        // todo 请求在后台更新Groups和BangumiItems
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
