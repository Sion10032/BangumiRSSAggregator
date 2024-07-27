using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Utils;

public static class RestApiBuilderExtensions
{
    public static RouteGroupBuilder MapSimpleRestApi<TEntity, TKey, TDbContext>(this RouteGroupBuilder group)
        where TEntity : class
        where TDbContext : DbContext
    {
        var builder = new RestApiBuilder<TEntity, TKey, TDbContext>();
        return builder.MapApi(group);
    }
}
