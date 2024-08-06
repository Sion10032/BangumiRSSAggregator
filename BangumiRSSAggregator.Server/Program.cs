/* 处理流程：
 * 1. 添加RSS订阅；
 * 2. 添加正则规则；
 * 3. 为RSS源启用规则，生成分组；
 * 4. 勾选需要的分组；
 * 5. 订阅该程序生成的RSS源。
 */

using BangumiRSSAggregator.Server;
using BangumiRSSAggregator.Server.Models;
using BangumiRSSAggregator.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BangumiDb>(options => options.UseSqlite(@"Data Source=.\db.sqlite"));
builder.Services.AddScoped<RSSUpdater>();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

var apiGroup = app.MapGroup("/api");
apiGroup.MapGroup("/rules")
    .MapSimpleRestApi<FeedRule, int, BangumiDb>();
apiGroup.MapGroup("/feeds")
    .MapSimpleRestApi<FeedSource, string, BangumiDb>();
apiGroup.MapGet(
    "/feeds/{id}/fetch_and_update", 
    ([FromRoute] int id, [FromServices] RSSUpdater updater) => updater.FetchAndUpdate(id));
apiGroup.MapGet(
    "/feeds/meta",
    ([FromQuery] string url) => RSSUpdater.GetFeedInfo(url));
apiGroup.MapGroup("/feed-items")
    .MapSimpleRestApi<FeedItem, string, BangumiDb>();
apiGroup.MapGroup("/bangumi/groups")
    .MapSimpleRestApi<FeedGroup, string, BangumiDb>();

//apiGroup.MapGroup("/feeds")
//    .MapFeedApis();

app.Run();
