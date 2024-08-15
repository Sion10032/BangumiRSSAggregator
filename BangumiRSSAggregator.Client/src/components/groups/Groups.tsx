import { useEffect, useRef, useState } from 'react';
import { Button, Card, message, Space } from 'antd';
import { MdPlayArrow, MdPlayDisabled } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { BangumiGroup } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import { getRestClient } from '@/shared/rest-client';
import { UpdateGroupStatusRequest } from '@/shared/types/requests';

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
      width: 100,
      render: (v : boolean) => v ? "✔" : "",
    }
  ];

  const setGroupStatus = async (status : boolean) => {
    if (selectedRowKeys.current.length <= 0) {
      messageApi.info("Please select as least one group.");
      return;
    }

    const reqParam: UpdateGroupStatusRequest = { groupIds: selectedRowKeys.current as number[] };
    console.log("update group status:", status, reqParam)

    // 同时启用/禁用多个group
    const result = await restClient.client
      .post(status ? "enable" : "disable", { json: reqParam });
    if (result.ok) {
      messageApi.info(`Update groups success.`)
    }
    else {
      messageApi.warning(`Update groups failed.`)
    }
    setRefreshData(!refreshData);
  };

  return (
    <>
      {contextHolder}
      <Card
        title="Groups"
        className="flexible-card"
        hoverable={true}>
        <AutoHeightTable
          offset={8}
          dataSource={groups}
          columns={columns}
          selectionType='multiple'
          keySelector={o => o.id}
          onSelectionChanged={async keys => {selectedRowKeys.current = keys}}
          size='small'
          pagination={false}>
        </AutoHeightTable>
      </Card>
      <Space.Compact className='horizontal-action-space-compact'>
        <Button icon={<MdPlayArrow/>} onClick={() => setGroupStatus(true)}/>
        <Button icon={<MdPlayDisabled/>} onClick={() => setGroupStatus(false)}/>
      </Space.Compact>
    </>
  );
}

export default Groups;