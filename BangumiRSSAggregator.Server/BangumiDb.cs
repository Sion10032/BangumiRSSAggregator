using BangumiRSSAggregator.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server;

/*
 *      [EnabledRule]
 *| Rule | <-many-> | Source | -many-> | Item |
 *                       ^                ^
 *                       |                |
 *                      many              |
 *                       |                |
 *                       v                |
 *                   | group | <---------many [BangumiItem]
 */

public class BangumiDb : DbContext
{
    public BangumiDb(DbContextOptions<BangumiDb> options) : base(options)
    {
    }

    public DbSet<FeedSource> FeedSources { get; set; }
    public DbSet<FeedItem> FeedItems { get; set; }

    public DbSet<FeedRule> FeedRules { get; set; }
    public DbSet<EnabledRule> EnabledRules { get; set; }

    public DbSet<FeedGroup> FeedGroups { get; set; }
    public DbSet<SourceGroup> SourceGroups { get; set; }

    public DbSet<BangumiItem> BangumiItems { get; set; }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is EntityBase && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.Now;

            if (entity.State == EntityState.Added)
            {
                ((EntityBase)entity.Entity).CreatedDate = now;
            }
            ((EntityBase)entity.Entity).UpdatedDate = now;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FeedSource>()
            .HasMany(e => e.FeedRules)
            .WithMany(e => e.FeedSources)
            .UsingEntity<EnabledRule>();

        modelBuilder.Entity<FeedSource>()
            .HasMany(e => e.FeedGroups)
            .WithMany(e => e.FeedSources)
            .UsingEntity<SourceGroup>();

        modelBuilder.Entity<FeedGroup>()
            .HasMany(e => e.FeedItems)
            .WithMany(e => e.FeedGroups)
            .UsingEntity<BangumiItem>();
    }
}
