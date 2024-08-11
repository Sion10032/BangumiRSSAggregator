import { useEffect, useRef, useState } from 'react';
import { Card, message } from 'antd';
import { MdPlayArrow, MdPlayDisabled } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { BangumiGroup } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import { getRestClient } from '@/shared/rest-client';
import { cloneAndUpdateProperty, getSelectedItems } from '@/shared/utils';

function Groups() {
  const [ messageApi, contextHolder ] = message.useMessage();
  const [ groups, setRules ] = useState<BangumiGroup[]>([]);
  const [ refreshData, setRefreshData ] = useState<boolean>(false);
  const selectedRowKeys = useRef<React.Key[]>([]);

  const restClient = getRestClient<BangumiGroup, number>("bangumi/groups");

  const refreshItems = async () => {
    setRules(await restClient.getAll());
  };

  useEffect(
    () => {
      refreshItems();
    },
    [ refreshData ]);

  const columns: TableColumnsType<BangumiGroup> = [
    {
      title: 'Name',
      dataIndex: 'name',
    },
    {
      title: 'Enabled',
      dataIndex: 'enabled',
    }
  ];

  const setGroupStatus = async (status : boolean) => {
    const selectedItems = getSelectedItems(groups, selectedRowKeys.current, o => o.id);
    if (selectedItems.length != 1) {
      return;
    }
    // todo 同时启用/禁用多个group
    const result = await restClient.update(
      selectedItems[0].id,
      cloneAndUpdateProperty(selectedItems[0], "enabled", status),
    );
    if (result) {
      messageApi.info(`Update group-${selectedItems[0].id} success.`)
    }
    else {
      messageApi.warning(`Update group-${selectedItems[0].id} failed.`)
    }
    setRefreshData(!refreshData);
  };

  return (
    <>
      {contextHolder}
      <Card
        title="Groups"
        className="flexible-card"
        hoverable={true}
        actions={[
          <MdPlayArrow onClick={() => setGroupStatus(true)}/>,
          <MdPlayDisabled onClick={() => setGroupStatus(false)}/>,
        ]}>
        <AutoHeightTable
          offset={8}
          dataSource={groups}
          columns={columns}
          keySelector={o => o.id}
          onSelectionChanged={async keys => {selectedRowKeys.current = keys}}
          size='small'
          pagination={false}>
        </AutoHeightTable>
      </Card>
    </>
  );
}

export default Groups;