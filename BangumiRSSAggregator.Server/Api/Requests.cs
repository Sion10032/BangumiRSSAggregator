﻿namespace BangumiRSSAggregator.Server.Api;

public record FeedTestRuleRequest(string Pattern, string Replacement);

public record UpdateGroupStatusRequest(ICollection<int> GroupIds);
