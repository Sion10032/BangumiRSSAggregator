﻿namespace BangumiRSSAggregator.Server.Models;

public class FeedRule
{
    public int Id { get; set; }
    public required string Pattern {  get; set; }
    public required string Map { get; set; }

    public ICollection<FeedSource> FeedSources { get; set; } = new List<FeedSource>();
}
