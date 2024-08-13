import { useEffect, useRef, useState } from 'react';
import { Card, message } from 'antd';
import { MdAdd, MdDelete, MdEdit, MdPlayArrow, MdPlayDisabled } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { FeedRule } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import AddRuleFormModal from './EditRule';
import { getRestClient } from '@/shared/rest-client';
import client from '@/shared/client';
import { UpdateRulesForFeedRequest } from '@/shared/types/requests';

function Rules() {
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
    const path = `feed-rules/${status ? "enable" : "disable"}`;
    const param : UpdateRulesForFeedRequest = {
      feedId: 0,
      ruleIds: selectedRowKeys.current as number[], 
    };
    await client.post(path, { json: param })
  };
  
  const onAddConfirm = async (value : FeedRule) => {
    if (await restClient.add(value)) {
      messageApi.success("add rule success.");
      setIsOpen(false);
      setRefreshData(!refreshData);
    }
    else {
      messageApi.warning("add rule failed");
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
        hoverable={true}
        actions={[
          <MdAdd onClick={openEditItemModal}/>,
          <MdEdit />,
          <MdDelete onClick={deleteItem}/>,
          <MdPlayArrow onClick={() => updateStatusForSelectedRules(true)} />,
          <MdPlayDisabled onClick={() => updateStatusForSelectedRules(false)} />,
        ]}>
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
    </>
  );
}

export default Rules;