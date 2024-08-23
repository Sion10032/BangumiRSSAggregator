

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Api;

public static class GroupHandler
{
    public static RouteGroupBuilder MapGroupApis(this RouteGroupBuilder group)
    {
        group.MapPost("enable", EnableGroups);
        group.MapPost("disable", DisableGroups);

        return group;
    }


    private static Task<int> EnableGroups(
        [FromBody] UpdateGroupStatusRequest param,
        [FromServices] BangumiDb db)
    {
        return SetGroupStatus(db, param.GroupIds, true);
    }
    private static Task<int> DisableGroups(
        [FromBody] UpdateGroupStatusRequest param,
        [FromServices] BangumiDb db)
    {
        return SetGroupStatus(db, param.GroupIds, false);
    }

    private static async Task<int> SetGroupStatus(
        BangumiDb db,
        ICollection<int> groupIds,
        bool status) 
    {
        await db.FeedGroups
            .Where(e => groupIds.Contains(e.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Enabled, e => status));
        return await db.SaveChangesAsync();
    }
}