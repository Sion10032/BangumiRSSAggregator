import { useEffect, useRef, useState } from 'react';
import { Button, Card, message, Space } from 'antd';
import { MdAdd, MdDelete, MdEdit, MdRefresh, MdScience } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { FeedSource } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import AddFeedFormModal from './EditFeed';
import { getRestClient } from '@/shared/rest-client';
import client from '@/shared/client';
import { TestRuleModal } from './TestRule';

type FeedsProps = {
  onSelectedFeedChanged? : (feedIds : number[]) => Promise<void>;
};

function Feeds({ onSelectedFeedChanged } : FeedsProps) {
  const [ messageApi, contextHolder ] = message.useMessage();
  const [ feeds, setFeeds ] = useState<FeedSource[]>([]);
  const [ refreshData, setRefreshData ] = useState<boolean>(false);
  const selectedRowKeys = useRef<React.Key[]>([]);

  const restClient = getRestClient<FeedSource, number>("feeds");

  const refreshFeeds = async () => {
    setFeeds(await restClient.getAll());
  };

  const [ isEditModalOpen, setIsEditModalOpen ] = useState(false);
  const openEditModal = () => {
    setIsEditModalOpen(true);
  };
  const deleteFeed = async () => {
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
  const fetchFeed = async () => {
    if (selectedRowKeys.current.length <= 0) {
      return;
    }

    const resp = await client
      .get(`feeds/${selectedRowKeys.current[0]}/fetch`)
      .json<boolean>();
    if (resp) {
      messageApi.success(`Fetching for feed-${selectedRowKeys.current[0]} success.`);
    }
    else {
      messageApi.warning(`Fetching for feed-${selectedRowKeys.current[0]} failed.`);
    }
  };

  const onAddConfirm = async (value : FeedSource) => {
    if (await restClient.add(value)) {
      messageApi.success("add feed success.");
      setIsEditModalOpen(false);
      setRefreshData(!refreshData);
    }
    else {
      messageApi.warning("add feed failed");
    }
  };
  const onAddCancel = async () => setIsEditModalOpen(false);

  const [ isTestRuleModalOpen, setIsTestRuleModalOpen ] = useState(false);
  const [ feedIdForTestRule, setFeedIdForTestRule ] = useState(0);
  const openTestRuleModal = () => {
    if (selectedRowKeys.current.length <= 0) {
      return;
    }
    const key = selectedRowKeys.current[0];
    if (typeof key !== "number") {
      return;
    }

    setFeedIdForTestRule(key);
    setIsTestRuleModalOpen(true);
  };

  useEffect(
    () => {
      refreshFeeds();
    },
    [ refreshData ]);

  const columns: TableColumnsType<FeedSource> = [
    {
      title: 'Name',
      dataIndex: 'name',
    }
  ];
  const onSelectionChanged = async (keys : React.Key[]) => {
    selectedRowKeys.current = keys;
    onSelectedFeedChanged?.(keys as number[]);
  };

  return (
    <>
      {contextHolder}
      <Card
        title="Feeds"
        className="flexible-card"
        hoverable={true}>
        <AutoHeightTable
          offset={8}
          dataSource={feeds}
          columns={columns}
          selectionType='single'
          keySelector={o => o.id}
          onSelectionChanged={onSelectionChanged}
          size='small'
          pagination={false}>
        </AutoHeightTable>
        <AddFeedFormModal
          isOpen={isEditModalOpen}
          onConfirm={onAddConfirm}
          onCancel={onAddCancel}/>
        <TestRuleModal
          sourceId={feedIdForTestRule}
          isOpen={isTestRuleModalOpen}
          onClose={async () => setIsTestRuleModalOpen(false)}
          onRuleAdded={async () => {
            setIsTestRuleModalOpen(false);
            window.location.reload();
          }}/>
      </Card>
      <Space.Compact className='horizontal-action-space-compact'>
        <Button icon={<MdAdd/>} onClick={openEditModal}/>
        <Button icon={<MdEdit/>} onClick={undefined}/>
        <Button icon={<MdDelete/>} onClick={deleteFeed}/>
        <Button icon={<MdRefresh/>} onClick={fetchFeed}/>
        <Button icon={<MdScience/>} onClick={openTestRuleModal}/>
      </Space.Compact>
    </>
  );
}

export default Feeds;