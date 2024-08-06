import { Button, Input, Modal, Space } from "antd";
import { State } from "@/shared/types/common";
import { FeedSource } from "@/shared/types/models";
import client from "@/shared/client";
import { FeedInfoResponse } from "@/shared/types/responses";

export function AddFeedForm({ feedState } : { feedState: State<FeedSource> }) {
  const [feedSource, setFeedSource] = feedState;
  const queryFeedInfo = async () => {
    const resp = await client
      .get(
        "feeds/meta",
        {
          searchParams: {
            url: feedSource.url,
          }
        })
      .json<FeedInfoResponse>();
    setFeedSource(prevValue => ({
      ...prevValue,
      name: resp.title,
    }));
  };
  const updateUrl: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    setFeedSource(prev => ({
      ...prev,
      url: e.target.value,
    }));
  };
  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Space.Compact style={{ width: '100%' }}>
        <Input placeholder="Url" value={feedSource.url} onChange={updateUrl}/>
        <Button onClick={queryFeedInfo}>Query</Button>
      </Space.Compact>
      <Input placeholder="Name" value={feedSource.name} />
      <Input placeholder="Update Interval (min)" value={feedSource.updateInterval} />
    </Space>
  );
}

export function AddFeedFormModal({ isOpen, feedState }: { isOpen: boolean, feedState: State<FeedSource> }) {
  return (
    <Modal
      title="AddFeed"
      open={isOpen}>
      <AddFeedForm feedState={feedState} />
    </Modal>
  );
}


export default AddFeedFormModal;