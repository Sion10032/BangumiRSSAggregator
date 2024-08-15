using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace BangumiRSSAggregator.Server;

public class BangumiBackgroudService : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(10);
    private readonly ConcurrentQueue<(string Name, BangumiBackgroudTask Task)> _taskQueue = new ConcurrentQueue<(string Name, BangumiBackgroudTask Task)>();
    private readonly IServiceProvider _services;
    private readonly ILogger<BangumiBackgroudService> _logger;
    private readonly Func<BangumiDb> _bangumiDbFactory;
    private readonly Func<RSSUpdater> _rssUpdateFactory;

    public BangumiBackgroudService(
        IServiceProvider services,
        ILogger<BangumiBackgroudService> logger,
        Func<BangumiDb> bangumiDbFactory,
        Func<RSSUpdater> rssUpdateFactory)
    {
        _services = services;
        _logger = logger;
        _bangumiDbFactory = bangumiDbFactory;
        _rssUpdateFactory = rssUpdateFactory;
    }

    public void EnqueueFetchAndUpdateAll()
    {
        _taskQueue.Enqueue(GetDefaultTask());
    }
    public void EnqueueFetchAndUpdate(int feedId)
    {
        _taskQueue.Enqueue((
            "FetchAndUpdate",
            async _ =>
            {
                var rssUpdater = _rssUpdateFactory.Invoke();

                // todo 添加日志
                await rssUpdater.FetchAndUpdate(feedId);
            }
        ));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_period);
        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!_taskQueue.TryDequeue(out var taskInfo)
                || taskInfo.Task == null)
            {
                _logger.LogInformation("no queued task, start default task.");
                taskInfo = GetDefaultTask();
            }

            try
            {
                _logger.LogInformation($"task start: {taskInfo.Name}");
                var startTime = DateTime.Now;
                using (var scope = _services.CreateScope())
                {
                    await taskInfo.Task.Invoke(null!);
                }
                var endTime = DateTime.Now;
                _logger.LogInformation($"task completed: {taskInfo.Name}, used time: {(endTime - startTime).TotalMilliseconds:F2}ms");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"task failed: {taskInfo.Name}, exception: {ex}");
            }
        }
    }

    private (string Name, BangumiBackgroudTask Task) GetDefaultTask()
    {
        return (
            "FetchAndUpdateAll",
            async (_) =>
            {
                var db = _bangumiDbFactory.Invoke();
                var rssUpdater = _rssUpdateFactory.Invoke();

                // todo 添加日志
                var feedIds = await db.FeedSources
                    .Where(e => e.LastUpdated == null
                        || e.LastUpdated.Value.AddMinutes(e.UpdateInterval) < DateTime.Now)
                    .Select(it => it.Id)
                    .ToListAsync();
                foreach (var feedId in feedIds)
                {
                    await rssUpdater.FetchAndUpdate(feedId);
                }
            }
        );
    }
}

public delegate Task BangumiBackgroudTask(BangumiDb db);