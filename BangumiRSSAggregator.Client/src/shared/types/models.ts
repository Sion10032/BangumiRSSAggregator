export type FeedSource = {
  id: number;
  name: string;
  url: string;
  updateInterval: number;
};

export type FeedItem = {
  id: string;
  name: string;
  url: string;
};

export type BangumiGroup = {
  id: number;
  name: string;
  enabled: boolean;
};

export type FeedRule = {
  id: number;
  pattern: string;
  replacement: string;
};

export type BangumiItem = FeedItem & {
  groupId : number;
};
