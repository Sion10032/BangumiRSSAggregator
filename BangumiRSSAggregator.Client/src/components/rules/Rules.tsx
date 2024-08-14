import { useEffect, useRef, useState } from 'react';
import { Button, Card, message, Space } from 'antd';
import { MdAdd, MdDelete, MdEdit, MdPlayArrow, MdPlayDisabled } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { FeedRule } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import AddRuleFormModal from './EditRule';
import { getRestClient } from '@/shared/rest-client';
import client from '@/shared/client';
import { UpdateRulesForFeedRequest } from '@/shared/types/requests';

type RulesProps = {
  currentFeedId? : number;
};

function Rules({ currentFeedId } : RulesProps) {
  const [ messageApi, contextHolder ] = message.useMessage();
  const [ feeds, setRules ] = useState<FeedRule[]>([]);
  const [ refreshData, setRefreshData ] = useState<boolean>(false);
  const selectedRowKeys = useRef<React.Key[]>([]);

  const restClient = getRestClient<FeedRule, number>("rules");

  const refreshItems = async () => {
    setRules(await restClient.getAll());
  };

  const [ isOpen, setIsOpen ] = useState(false);
  const openEditItemModal = () => {
    setIsOpen(true);
  };
  const deleteItem = async () => {
    if (selectedRowKeys.current.length <= 0) {
      return;
    }

    for (const feedId of selectedRowKeys.current) {
      if (typeof feedId === 'number') {
        await restClient.del(feedId);
      }
    }
    setRefreshData(!refreshData);
  };
  const updateStatusForSelectedRules = async (status : boolean) => {
    if (!currentFeedId) {
      messageApi.info("Please select a feed.");
      return;
    }

    const path = `feed-rules/${status ? "enable" : "disable"}`;
    const param : UpdateRulesForFeedRequest = {
      feedId: currentFeedId,
      ruleIds: selectedRowKeys.current as number[], 
    };
    await client.post(path, { json: param })
  };
  
  const onAddConfirm = async (value : FeedRule) => {
    if (await restClient.add(value)) {
      messageApi.success("Add rule success.");
      setIsOpen(false);
      setRefreshData(!refreshData);
    }
    else {
      messageApi.warning("Add rule failed");
    }
  };
  const onAddCancel = async () => setIsOpen(false);

  useEffect(
    () => {
      refreshItems();
    },
    [ refreshData ]);

  const columns: TableColumnsType<FeedRule> = [
    {
      title: 'Pattern',
      dataIndex: 'pattern',
    },
    {
      title: 'Replacement',
      dataIndex: 'replacement',
    }
  ];

  return (
    <>
      {contextHolder}
      <Card
        title="Rules"
        className="flexible-card"
        hoverable={true}>
        <AutoHeightTable
          offset={8}
          dataSource={feeds}
          columns={columns}
          selectionType='single'
          keySelector={o => o.id}
          onSelectionChanged={async keys => {selectedRowKeys.current = keys}}
          size='small'
          pagination={false}>
        </AutoHeightTable>
        <AddRuleFormModal
          isOpen={isOpen}
          onConfirm={onAddConfirm}
          onCancel={onAddCancel}/>
      </Card>
      <Space.Compact className='horizontal-action-space-compact'>
        <Button icon={<MdAdd/>} onClick={openEditItemModal}/>
        <Button icon={<MdEdit/>} onClick={undefined}/>
        <Button icon={<MdDelete/>} onClick={deleteItem}/>
        <Button icon={<MdPlayArrow/>} onClick={() => updateStatusForSelectedRules(true)}/>
        <Button icon={<MdPlayDisabled/>} onClick={() => updateStatusForSelectedRules(false)}/>
      </Space.Compact>
    </>
  );
}

export default Rules;