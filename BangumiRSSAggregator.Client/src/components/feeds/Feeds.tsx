import { useEffect, useRef, useState } from 'react';
import { Card, message } from 'antd';
import { MdAdd, MdDelete, MdEdit, MdRefresh } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { FeedSource } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import AddFeedFormModal from './EditFeed';
import { getRestClient } from '@/shared/rest-client';
import client from '@/shared/client';

function Feeds() {
  const [ messageApi, contextHolder ] = message.useMessage();
  const [ feeds, setFeeds ] = useState<FeedSource[]>([]);
  const [ refreshData, setRefreshData ] = useState<boolean>(false);
  const selectedRowKeys = useRef<React.Key[]>([]);

  const restClient = getRestClient<FeedSource, number>("feeds");

  const refreshFeeds = async () => {
    setFeeds(await restClient.getAll());
  };

  const [ isOpen, setIsOpen ] = useState(false);
  const openEditModal = () => {
    setIsOpen(true);
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
      setIsOpen(false);
      setRefreshData(!refreshData);
    }
    else {
      messageApi.warning("add feed failed");
    }
  };
  const onAddCancel = async () => setIsOpen(false);

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

  return (
    <>
      {contextHolder}
      <Card
        title="Feeds"
        className="flexible-card"
        hoverable={true}
        actions={[
          <MdAdd onClick={openEditModal}/>,
          <MdEdit />,
          <MdDelete onClick={deleteFeed}/>,
          <MdRefresh onClick={fetchFeed}/>,
        ]}>
        <AutoHeightTable
          offset={8}
          dataSource={feeds}
          columns={columns}
          keySelector={o => o.id}
          onSelectionChanged={async keys => {selectedRowKeys.current = keys}}
          size='small'
          pagination={false}>
        </AutoHeightTable>
        <AddFeedFormModal
          isOpen={isOpen}
          onConfirm={onAddConfirm}
          onCancel={onAddCancel}/>
      </Card>
    </>
  );
}

export default Feeds;