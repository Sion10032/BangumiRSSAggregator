/* 处理流程：
 * 1. 添加RSS订阅；
 * 2. 添加正则规则；
 * 3. 为RSS源启用规则，生成分组；
 * 4. 勾选需要的分组；
 * 5. 订阅该程序生成的RSS源。
 */

using BangumiRSSAggregator.Server;
using BangumiRSSAggregator.Server.Api;
using BangumiRSSAggregator.Server.Models;
using BangumiRSSAggregator.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddCors(options => {
    options.AddPolicy(
        "dev-allow-all",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BangumiDb>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("BangumiDb")), 
    ServiceLifetime.Transient, 
    ServiceLifetime.Transient);
builder.Services.AddTransient<Func<BangumiDb>>(ctx => () => ctx.GetService<BangumiDb>()!);
builder.Services.AddTransient<RSSUpdater>();
builder.Services.AddTransient<Func<RSSUpdater>>(ctx => () => ctx.GetService<RSSUpdater>()!);

builder.Services.AddSingleton<BangumiBackgroudService>();
builder.Services.AddHostedService<BangumiBackgroudService>(provider => provider.GetService<BangumiBackgroudService>()!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("dev-allow-all");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/rss.xml", GetFeed);

var apiGroup = app.MapGroup("/api");
apiGroup.MapGroup("/feeds")
    .MapSimpleRestApi<FeedSource, int, BangumiDb>();
apiGroup.MapGroup("/feeds")
    .MapFeedApis();
apiGroup.MapGroup("/rules")
    .MapSimpleRestApi<FeedRule, int, BangumiDb>();
apiGroup.MapGroup("/feed-rules")
    .MapFeedRuleApis();
apiGroup.MapGroup("/feed-items")
    .MapSimpleRestApi<FeedItem, string, BangumiDb>();
apiGroup.MapGroup("/bangumi/groups")
    .MapSimpleRestApi<FeedGroup, int, BangumiDb>()
    .MapGroupApis();
apiGroup.MapGroup("/bangumi/items")
    .MapBangumiItemApis();

app.UseDefaultFiles();
app.UseStaticFiles();

// migration
if (!Directory.Exists("data"))
{
    Directory.CreateDirectory("data");
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BangumiDb>();
    db.Database.Migrate();
}

app.Run();

async Task<string> GetFeed(HttpContext context, [FromServices] RSSUpdater rssUpdater)
{
    context.Response.ContentType = "text/xml";
    var result = await rssUpdater.GetGeneratedFeed();
    return result;
}