# Bangumi RSS Aggregator

*Bangumi RSS Aggregator* 是一个RSS订阅处理器，用于从BT RSS源拉取下载项目，应用相应的规则对下载项目进行处理，然后生成新的BT RSS订阅源供 *qBittorrent* 等下载器使用。

## 已有功能

* 订阅RSS源并定时拉取更新；
* 对RSS源中的下载项目标题应用正则提取分组；
* 对RSS源进行规则测试；
* 启用/禁用生成的分组；
* 根据启用的分组生成新的RSS下载源；
* 展示当前处理成功的下载项。

## Todos

* [ ] 增加身份验证；
* [ ] 增加参数设置界面；
* [ ] 各种信息的分页查询、排序；
* [ ] 当前处理成功的下载项，启用状态/分组展示；
* [ ] 优化界面显示，增加夜间模式；
* [ ] 其余分组方式，如按Tag等；
* [ ] ...

## 部署

### docker-compose部署

```docker
version: "3"
services:
  bangumi-rss-aggregator:
    image: bangumi-rss-aggregator:latest
    container_name: bangumi-rss-aggregator
    ports:
      - 20000:8080
    volumes:
      - ./data:/app/data
    restart: always
```
