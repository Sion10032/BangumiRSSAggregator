using System.Text.RegularExpressions;
using System.Data;
using Microsoft.EntityFrameworkCore;
using BangumiRSSAggregator.Server.Models;
using BangumiRSSAggregator.Server.Utils;
using BangumiRSSAggregator.Server.Api;
using System.ServiceModel.Syndication;
using System.Xml;

namespace BangumiRSSAggregator.Server;

public class RSSUpdater
{
    private readonly BangumiDb _context;

    public RSSUpdater(BangumiDb context)
    {
        _context = context;
    }

    #region WebApi
    /// <summary>
    /// 拉取FeedSourceId，将拉取的FeedItem存储到数据库，并应用已经启用的Rule生成相应的BangumiItem
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    public async Task<bool> FetchAndUpdate(int sourceId)
    {
        try
        {
            var (_, newItems) = await FetchFeed(sourceId);

            var rules = await GetRulesByFeedIdFromDb(sourceId);

            await ApplyRulesForItems(sourceId, newItems, rules);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// 拉取FeedSourceId，将拉取的FeedItem存储到数据库
    /// </summary>
    /// <param name="feedId"></param>
    /// <returns></returns>
    public async Task<(FeedSource? Feed, IReadOnlyList<FeedItem> Items)> FetchFeed(int feedId)
    {
        var source = await _context.FeedSources.FindAsync(feedId);
        if (source == null)
        {
            return (null, Array.Empty<FeedItem>());
        }

        var items = await GetFeedItemFromWeb(source.Url);
        var itemIds = items.Select(it => it.Id).ToList();
        var existsId = _context.FeedItems
            .Where(it => itemIds.Contains(it.Id))
            .Select(it => it.Id)
            .ToHashSet();
        var newItems = items.Where(it => !existsId.Contains(it.Id)).ToList();
        newItems.ForEach(it => it.FeedSourceId = feedId);

        // save new items
        source.LastUpdated = DateTime.Now;
        await _context.FeedItems.AddRangeAsync(newItems);
        await _context.SaveChangesAsync();

        return (source, newItems);
    }

    /// <summary>
    /// 尝试对数据库中已经存在的FeedSource测试规则
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="pattern"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, List<FeedItem>>> TestFeedRule(
        int sourceId, string pattern, string replacement)
    {
        var feedItems = await _context.FeedItems.Where(it => it.FeedSourceId == sourceId).ToListAsync();
        return 
            Grouping(
                feedItems,
                [ new FeedRule { Pattern = pattern, Replacement = replacement } ])
            .ToDictionary(
                it => it.Key,
                it => it.Value.Select(it => it.FeedItem).ToList());
    }

    /// <summary>
    /// 对指定Feed应用指定Rules
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="ruleIds"></param>
    /// <returns></returns>
    public async Task ApplyNewRulesForFeed(
        int sourceId, ICollection<int> ruleIds)
    {
        var rules = await _context.FeedRules
            .Where(e => ruleIds.Contains(e.Id))
            .ToListAsync();
        await ApplyRulesForItems(sourceId, null, rules);
    }

    public async Task<string> GetGeneratedFeed()
    {
        var bangumiItems = await _context.BangumiItems
            .Include(it => it.FeedItem)
            .Include(it => it.FeedGroup)
            .Where(it => it.FeedGroup.Enabled)
            .OrderByDescending(it => it.FeedItem.PubDate) // 按发布时间倒叙排序
            .Take(200)
            .ToListAsync();

        var rssFormatter = new Rss20FeedFormatter(new SyndicationFeed(
            "Bangumi RSS",
            "",
            new Uri("http://bangumi-rss/rss.xml"), // todo link 
            bangumiItems
                .Select(it => 
                {
                    var item = SyndicationItem.Load(XmlReader.Create(new StringReader(it.FeedItem.RawContent)));
                    item.Summary = new TextSyndicationContent(
                        item.Summary.Text?.Replace("\n", "&#xA;") ?? string.Empty);
                    return item;
                })
                .ToList()));

        using (var stringWriter = new StringWriter())
        using (var xmlWriter = XmlWriter.Create(stringWriter))
        {
            rssFormatter.WriteTo(xmlWriter);
            xmlWriter.Flush();
            stringWriter.Flush();
            return stringWriter.ToString();
        }
    }
    #endregion

    private async Task ApplyRulesForItems(
        int sourceId, 
        IReadOnlyList<FeedItem>? items = null, 
        IReadOnlyList<FeedRule>? rules = null)
    {
        var processedItems = Grouping(
            items ?? await GetFeedItemsByFeedIdFromDb(sourceId), 
            rules ?? await GetRulesByFeedIdFromDb(sourceId));

        await ApplyGroups(sourceId, processedItems);
    }

    private async Task ApplyGroups(int sourceId, Dictionary<string, ICollection<(FeedItem FeedItem, FeedRule Rule)>> processedItems)
    {
        var groups = await GetGroupsByFeedIdFromDb(sourceId);
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

    private static Dictionary<string, ICollection<(FeedItem FeedItem, FeedRule Rule)>> Grouping(
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

    #region Query data from db
    public async Task<IReadOnlyList<FeedGroup>> GetGroupsByFeedIdFromDb(int feedId)
    {
        var groupIds = await _context.SourceGroups
            .Where(it => it.FeedSourceId == feedId)
            .Select(it => it.FeedGroupId)
            .ToListAsync();

        return await _context.FeedGroups
            .Where(it => groupIds.Contains(it.Id))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FeedRule>> GetRulesByFeedIdFromDb(int feedId)
    {
        var ruleIds = await _context.EnabledRules
            .Where(it => it.FeedSourceId == feedId)
            .Select(it => it.FeedRuleId)
            .ToListAsync();

        return await _context.FeedRules
            .Where(it => ruleIds.Contains(it.Id))
            .ToListAsync();
    }
    
    private Task<List<FeedItem>> GetFeedItemsByFeedIdFromDb(int sourceId)
    {
        return _context.FeedItems
            .Where(e => e.FeedSourceId == sourceId)
            .ToListAsync();
    }
    #endregion

    private async Task<IReadOnlyList<FeedItem>> GetFeedItemFromWeb(string url)
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
                PubDate = it.PublishingDate ?? DateTime.Now,
                RawContent = it.Element.ToXml(),
            })
            .ToList();
    }

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
