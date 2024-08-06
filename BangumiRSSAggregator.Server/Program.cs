/* �������̣�
 * 1. ���RSS���ģ�
 * 2. ����������
 * 3. ΪRSSԴ���ù������ɷ��飻
 * 4. ��ѡ��Ҫ�ķ��飻
 * 5. ���ĸó������ɵ�RSSԴ��
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
