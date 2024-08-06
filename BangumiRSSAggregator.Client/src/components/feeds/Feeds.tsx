import { ReactNode, useState } from 'react';
import { Card } from 'antd';
import { MdAdd, MdDelete, MdEdit } from "react-icons/md";

import type { TableColumnsType } from 'antd';
import type { FeedSource } from '@/shared/types/models';

import AutoHeightTable from '../AutoHeightTable'
import AddFeedFormModal from './AddFeed';

const emptyFeedSource : FeedSource = 
{
  id: -1,
  name: "",
  url: "",
  updateInterval: 15,
};

function Feeds({ feeds }: { feeds: FeedSource[] }) {
  const [ isOpen, setIsOpen ] = useState(false);
  const newFeed = useState<FeedSource>({ ...emptyFeedSource });
  const actions: ReactNode[] = [
    <MdAdd onClick={() => setIsOpen(true) } />,
    <MdEdit />,
    <MdDelete />,
  ];
  const columns: TableColumnsType<FeedSource> = [
    {
      title: 'Name',
      dataIndex: 'name',
    }
  ];
  return (
    <Card
      title="Feeds"
      className="flexible-card"
      hoverable={true}
      actions={actions}>
      <AutoHeightTable
        offset={8}
        dataSource={feeds}
        columns={columns}
        rowKey="id"
        size='small'
        pagination={false}>
      </AutoHeightTable>
      <AddFeedFormModal isOpen={isOpen} feedState={newFeed} />
    </Card>
  );
}

export default Feeds;