import client from "@/shared/client";
import { Dict } from "@/shared/types/common";
import { FeedItem } from "@/shared/types/models";
import { Button, Divider, Input, message, Modal, Space, Tree, TreeDataNode } from "antd";
import { useState } from "react";

export function TestRule({ sourceId } : { sourceId : number }) {
  const [ messageApi, contextHolder ] = message.useMessage();

  const [ pattern, setPattern ] = useState("");
  const [ replacement, setReplacement ] = useState("");
  const [ treeData, setTreeData ] = useState<TreeDataNode[] | undefined>();

  const testRule = async () => {
    const resp = await client
      .post(
        `feeds/${sourceId}/test-rule`,
        {
          json: { pattern, replacement }
        });
    if (resp.ok) {
      const data = await resp.json<Dict<string, FeedItem[]>>();
      const newTreeData: TreeDataNode[] = [];
      for (let key in data) {
        newTreeData.push({
          title: key,
          key: key,
          children: data[key].map(item => ({
            title: item.name,
            key: item.url,
          }))
        })
      }
      setTreeData(newTreeData.length > 0 
        ? newTreeData
        : undefined);
    }
    else {
      messageApi.warning("Test rule failed.");
    }
  };

  return (<>
    {contextHolder}
    <Space direction="vertical" style={{ width: '100%' }}>
      <Input 
        placeholder="Pattern" 
        value={pattern}
        onChange={e => setPattern(e.target.value)}/>
      <Input 
        placeholder="Replacement"
        value={replacement}
        onChange={e => setReplacement(e.target.value)}/>
      <Space.Compact block={true}>
        <Button style={{ width: '50%' }} onClick={testRule}>Test Rule</Button>
        <Button style={{ width: '50%' }} onClick={testRule}>Add Rule</Button>
      </Space.Compact>
      {treeData
        && <>
          <Divider/>
          <Tree
            style={{ height: '400px', overflowY: 'auto' }} 
            treeData={treeData}/>
        </>}
    </Space>
  </>);
};

export function TestRuleModal({ sourceId, isOpen } : { sourceId : number, isOpen: boolean }) {
  return (
    <Modal
      title="Test Rule"
      open={isOpen}>
      <TestRule sourceId={sourceId} />
    </Modal>
  );
};