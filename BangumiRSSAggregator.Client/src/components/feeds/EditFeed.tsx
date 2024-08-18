import { Button, Input, InputNumber, Modal, Space } from "antd";
import { FormModalDefaultProps, State } from "@/shared/types/common";
import { FeedSource } from "@/shared/types/models";
import client from "@/shared/client";
import { FeedInfoResponse } from "@/shared/types/responses";
import { useState } from "react";
import { cloneAndUpdateProperty } from "@/shared/utils";

export function EditFeedForm({ feedState } : { feedState: State<FeedSource> }) {
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
    setFeedSource(prevValue => cloneAndUpdateProperty(prevValue, "name", resp.title));
  };
  const updateProperty = <TKey extends keyof FeedSource>(propertyName: TKey, value : FeedSource[TKey]) => {
    setFeedSource(prevValue => cloneAndUpdateProperty(prevValue, propertyName, value));
  };
  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Space.Compact style={{ width: '100%' }}>
        <Input 
          placeholder="Url" 
          value={feedSource?.url}
          onChange={e => updateProperty("url", e.target.value)}/>
        <Button onClick={queryFeedInfo}>Query</Button>
      </Space.Compact>
      <Input 
        placeholder="Name"
        value={feedSource?.name}
        onChange={e => updateProperty("name", e.target.value)}/>
      <InputNumber 
        placeholder="Update Interval (min)" 
        value={feedSource?.updateInterval}
        onChange={e => updateProperty("updateInterval", e ?? 15)}
        style={{ width: '100%' }}/>
    </Space>
  );
}

export function AddFeedFormModal({ isOpen, defaultValue, onConfirm, onCancel } : FormModalDefaultProps<FeedSource>) {
  const feedSourceState = useState(defaultValue ?? {
    id : 0,
    url: "",
    name: "",
    updateInterval: 15,
  });
  return (
    <Modal
      title="Add Feed"
      open={isOpen}
      onOk={_ => onConfirm(feedSourceState[0])}
      onCancel={_ => onCancel()}
      onClose={_ => onCancel()}>
      <EditFeedForm feedState={feedSourceState} />
    </Modal>
  );
}


export default AddFeedFormModal;