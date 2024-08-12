using System.Net.Http;
using System.Text.RegularExpressions;
using System.Data;
using Microsoft.EntityFrameworkCore;
using BangumiRSSAggregator.Server.Models;
using BangumiRSSAggregator.Server.Utils;
using BangumiRSSAggregator.Server.Api;

namespace BangumiRSSAggregator.Server;

public class RSSUpdater
{
    private readonly BangumiDb _context;

    public RSSUpdater(BangumiDb context)
    {
        _context = context;
    }

    #region 主要逻辑：拉取、从规则生成分组、生成BangumiItem
    public async Task<bool> FetchAndUpdate(int sourceId)
    {
        try
        {
            var (_, newItems) = await FetchFeed(sourceId);

            var rules = await GetRulesByFeedId(sourceId);

            var processedItems = Grouping(newItems, rules);

            await ApplyGroups(sourceId, processedItems);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<(FeedSource? Feed, IReadOnlyList<FeedItem> Items)> FetchFeed(int feedId)
    {
        var source = await _context.FeedSources.FindAsync(feedId);
        if (source == null)
        {
            return (null, Array.Empty<FeedItem>());
        }

        var items = await GetFeedItem(source.Url);
        var itemIds = items.Select(it => it.Id).ToList();
        var existsId = _context.FeedItems
            .Where(it => itemIds.Contains(it.Id))
            .Select(it => it.Id)
            .ToHashSet();
        var newItems = items.Where(it => !existsId.Contains(it.Id)).ToList();
        newItems.ForEach(it => it.FeedSourceId = feedId);

        // save new items
        await _context.FeedItems.AddRangeAsync(newItems);
        await _context.SaveChangesAsync();

        return (source, newItems);
    }

    public async Task<IReadOnlyList<FeedRule>> GetRulesByFeedId(int feedId)
    {
        var ruleIds = await _context.EnabledRules
            .Where(it => it.FeedSourceId == feedId)
            .Select(it => it.FeedRuleId)
            .ToListAsync();

        return await _context.FeedRules
            .Where(it => ruleIds.Contains(it.Id))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FeedGroup>> GetGroupsByFeedId(int feedId)
    {
        var groupIds = await _context.SourceGroups
            .Where(it => it.FeedSourceId == feedId)
            .Select(it => it.FeedGroupId)
            .ToListAsync();

        return await _context.FeedGroups
            .Where(it => groupIds.Contains(it.Id))
            .ToListAsync();
    }

    private async Task ApplyGroups(int sourceId, Dictionary<string, ICollection<(FeedItem FeedItem, FeedRule Rule)>> processedItems)
    {
        var groups = await GetGroupsByFeedId(sourceId);
        var groupNames = processedItems.Keys.ToList();
        foreach (var group in processedItems)
        {
            var existGroup = groups.FirstOrDefault(it => it.Name == group.Key);
            if (existGroup == null)
            {
                existGroup = new FeedGroup
                {
                    Name = group.Key,
                    Enabled = false,
                };
                await _context.FeedGroups.AddAsync(existGroup);
                await _context.SourceGroups.AddAsync(new SourceGroup
                {
                    FeedSourceId = sourceId,
                    FeedGroup = existGroup,
                });
            }
            await _context.BangumiItems.AddRangeAsync(group.Value
                .Select(it => new BangumiItem
                {
                    FeedItemId = it.FeedItem.Id,
                    FeedGroup = existGroup,
                })
                .ToList());
        }
        await _context.SaveChangesAsync();
    }

    public static Dictionary<string, ICollection<(FeedItem FeedItem, FeedRule Rule)>> Grouping(
        IReadOnlyList<FeedItem> newItems, IReadOnlyList<FeedRule> rules)
    {
        // process items by rules
        var processedItems = new Dictionary<string, ICollection<(FeedItem FeedItem, FeedRule Rule)>>();
        foreach (var item in newItems)
        {
            var bestRule = rules
                .Select(it => new
                {
                    Score = Regex.Match(item.Name, it.Pattern).Value?.Length ?? 0,
                    Rule = it,
                })
                .OrderByDescending(it => it.Score)
                .FirstOrDefault();
            if (bestRule == null || bestRule.Score <= 0)
            {
                continue;
            }

            var groupName = Regex.Replace(item.Name, bestRule.Rule.Pattern, bestRule.Rule.Replacement);
            if (!processedItems.TryGetValue(groupName, out var group))
            {
                group = new List<(FeedItem FeedItem, FeedRule Rule)>();
                processedItems.Add(groupName, group);
            }
            group.Add((item, bestRule.Rule));
        }

        return processedItems;
    }

    private async Task<IReadOnlyList<FeedItem>> GetFeedItem(string url)
    {
        CodeHollow.FeedReader.Feed feed = await GetFeed(url);
        // todo 是否除了2.0也兼容？
        if (feed == null || feed.Type != CodeHollow.FeedReader.FeedType.Rss_2_0)
        {
            return Array.Empty<FeedItem>();
        }

        return feed.Items
            .Select(it => it.SpecificItem)
            .OfType<CodeHollow.FeedReader.Feeds.Rss20FeedItem>()
            .Select(it => new FeedItem
            {
                Id = it.Guid,
                Name = it.Title,
                Url = it.Enclosure.Url,
                RawContent = it.Element.ToXml(),
            })
            .ToList();
    }
    #endregion

    public static async Task<FeedInfoResponse> GetFeedInfo(string url)
    {
        var feed = await GetFeed(url);
        return new FeedInfoResponse(feed.Link, feed.Title, feed.Description);
    }

    private static async Task<CodeHollow.FeedReader.Feed> GetFeed(string url)
    {
        var content = await HttpHelper.GetHttpClient().GetResponseText(url);
        var feed = CodeHollow.FeedReader.FeedReader.ReadFromString(content);
        return feed;
    }
}
