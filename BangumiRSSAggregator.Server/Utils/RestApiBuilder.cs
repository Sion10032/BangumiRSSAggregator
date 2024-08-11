using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server.Utils;

public class RestApiBuilder<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    public delegate Task<IValueHttpResult<IReadOnlyList<TEntity>>> GetAllOperation(
        [FromServices] TDbContext db);
    public delegate Task<IValueHttpResult<TEntity>> GetOperation(
        [FromServices] TDbContext db,
        [FromRoute] TKey id);
    public delegate Task<IValueHttpResult<TEntity>> AddOperation(
        [FromServices] TDbContext db,
        [FromBody] TEntity e);
    public delegate Task<IValueHttpResult<TEntity>> UpdateOperation(
        [FromServices] TDbContext db,
        [FromRoute] TKey id,
        [FromBody] TEntity e);
    public delegate Task<IValueHttpResult<TEntity>> DeleteOperation(
        [FromServices] TDbContext db,
        [FromRoute] TKey id);

    private GetAllOperation _getAll;
    private AddOperation _add;
    private GetOperation _get;
    private UpdateOperation _update;
    private DeleteOperation _delete;

    public RestApiBuilder()
    {
        _getAll = async (TDbContext db) =>
        {
            return TypedResults.Ok(await db.Set<TEntity>().ToListAsync());
        };
        _add = async (TDbContext db, TEntity e) =>
        {
            await db.Set<TEntity>().AddAsync(e);
            await db.SaveChangesAsync();
            var updateCount = await db.SaveChangesAsync();
            return updateCount > 0 ? TypedResults.Ok(e) : TypedResults.BadRequest<TEntity>(null);
        };
        _get = async (TDbContext db, TKey id) =>
        {
            var entity = await db.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                return TypedResults.NotFound<TEntity>(null);
            }

            return TypedResults.Ok(entity);
        };
        _update = async (TDbContext db, TKey id, TEntity e) =>
        {
            var entity = await db.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                return TypedResults.NotFound<TEntity>(null);
            }

            var properties = typeof(TEntity).GetProperties()
                .Where(it => it.CanWrite)
                .Where(it => it.PropertyType.IsValueType || it.PropertyType == typeof(string))
                .Where(it => it.Name != "Id") // todo 更加合理的过滤主键的方法
                .ToList();
            foreach (var property in properties)
            {
                property.SetValue(entity, property.GetValue(e));
            }
            db.Set<TEntity>().Update(entity);
            var updateCount = await db.SaveChangesAsync();
            return updateCount > 0 ? TypedResults.Ok<TEntity>(entity) : TypedResults.BadRequest<TEntity>(null);
        };
        _delete = async (TDbContext db, TKey id) =>
        {
            var entity = await db.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                return TypedResults.NotFound<TEntity>(null);
            }
            db.Set<TEntity>().Remove(entity);
            var updateCount = await db.SaveChangesAsync();
            return updateCount > 0 ? TypedResults.Ok<TEntity>(entity) : TypedResults.BadRequest<TEntity>(null);
        };
    }

    public RouteGroupBuilder MapApi(RouteGroupBuilder group)
    {
        group.MapGet("/", _getAll);
        group.MapPost("/", _add);
        group.MapGet("/{id}", _get);
        group.MapPut("/{id}", _update);
        group.MapDelete("/{id}", _delete);

        return group;
    }
}

public record Response<T>(bool Success, T Value);
