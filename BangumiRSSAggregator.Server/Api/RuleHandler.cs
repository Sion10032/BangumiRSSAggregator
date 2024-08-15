using BangumiRSSAggregator.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Api;

public static class RuleHandler
{
    public static RouteGroupBuilder MapFeedRuleApis(this RouteGroupBuilder group)
    {
        group.MapPost("enable", EnableRulesForFeed);
        group.MapPost("disable", DisableRulesForFeed);
        return group;
    }

    private static async Task EnableRulesForFeed(
        [FromBody] UpdateRulesForFeedRequest param,
        [FromServices] BangumiDb db,
        [FromServices] RSSUpdater rssUpdater)
    {
        var existRules = await db.EnabledRules
            .Where(it => it.FeedSourceId == param.FeedId)
            .Select(it => it.FeedRuleId)
            .ToListAsync();
        var needAddRuleIds = param.RuleIds.Except(existRules).ToList();
        if (needAddRuleIds.Count > 0)
        {
            await db.EnabledRules.AddRangeAsync(needAddRuleIds
                .Select(it => new EnabledRule { FeedSourceId = param.FeedId, FeedRuleId = it })
                .ToList());

            // 启用规则后，应用新启用的规则
            await rssUpdater.ApplyNewRulesForFeed(param.FeedId, needAddRuleIds);
        }
    }

    private static Task DisableRulesForFeed(
        [FromBody] UpdateRulesForFeedRequest param,
        [FromServices] BangumiDb db)
    {
        // 禁用规则不影响已经生成的Group与BangumiItem
        return db.EnabledRules
            .Where(e => e.FeedSourceId == param.FeedId)
            .Where(e => param.RuleIds.Contains(e.FeedRuleId))
            .ExecuteDeleteAsync();
    }
}
