import { Input, Modal, Space } from "antd";
import { FormModalDefaultProps, State } from "@/shared/types/common";
import { FeedRule } from "@/shared/types/models";
import { useState } from "react";
import { cloneAndUpdateProperty } from "@/shared/utils";

export function EditRuleForm({ feedState } : { feedState: State<FeedRule> }) {
  const [feedRule, setFeedRule] = feedState;
  const updateProperty = <TKey extends keyof FeedRule>(propertyName: TKey, value : FeedRule[TKey]) => {
    setFeedRule(prevValue => cloneAndUpdateProperty(prevValue, propertyName, value));
  };
  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Input 
        placeholder="Pattern"
        value={feedRule.pattern}
        onChange={e => updateProperty("pattern", e.target.value)}/>
      <Input 
        placeholder="replacement"
        value={feedRule.replacement}
        onChange={e => updateProperty("replacement", e.target.value)}/>
    </Space>
  );
}

export function AddRuleFormModal({ isOpen, defaultValue, onConfirm, onCancel } : FormModalDefaultProps<FeedRule>) {
  const feedRuleState = useState(defaultValue ?? {
    id : 0,
    pattern: "",
    replacement: "",
  });
  return (
    <Modal
      title="AddFeed"
      open={isOpen}
      onOk={e => onConfirm(feedRuleState[0])}
      onCancel={e => onCancel()}
      onClose={e => onCancel()}>
      <EditRuleForm feedState={feedRuleState} />
    </Modal>
  );
}


export default AddRuleFormModal;