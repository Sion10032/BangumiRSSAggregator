export type FeedTestRuleRequest = {
  pattern : string;
  replacement : string;
};

export type UpdateGroupStatusRequest = {
  groupIds : number[];
};

export type UpdateRulesForFeedRequest = {
  feedId : number;
  ruleIds : number[];
};