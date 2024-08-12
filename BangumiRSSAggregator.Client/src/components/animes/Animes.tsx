import { useEffect, useState } from 'react';
import { Card } from 'antd';

import type { TableColumnsType } from 'antd';
import type { BangumiItem } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import client from '@/shared/client';

function Animes() {
  const [ items, setItems ] = useState<BangumiItem[]>([]);
  const [ refreshData, setRefreshData ] = useState<boolean>(false);

  const getItems = () => client.get("bangumi/items").json<BangumiItem[]>();

  const refreshItems = async () => {
    setItems(await getItems());
  };

  useEffect(
    () => {
      refreshItems();
    },
    [ refreshData ]);

  const columns: TableColumnsType<BangumiItem> = [
    {
      title: 'Name',
      dataIndex: 'name',
    },
    {
      title: 'Url',
      dataIndex: 'url',
    }
  ];

  return (
    <>
      <Card
        title="Animes"
        className="flexible-card"
        hoverable={true}>
        <AutoHeightTable
          offset={8}
          dataSource={items}
          columns={columns}
          keySelector={o => o.id}
          size='small'
          pagination={false}>
        </AutoHeightTable>
      </Card>
    </>
  );
}

export default Animes;