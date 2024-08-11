import { useState } from 'react';
import ResizeObserver from 'rc-resize-observer';
import { Table, TableProps } from 'antd';

type Props<T = any> = Omit<TableProps<T>, "rowKey" | "rowSelection"> & {
  offset?: number;
  headerHeight?: number;
  keySelector: (obj : T) => React.Key;
  onSelectionChanged? : (keys : React.Key[]) => Promise<void>;
};

type TableRowSelection<T> = TableProps<T>['rowSelection'];

function AutoHeightTable<T = any>({ offset, headerHeight, keySelector, onSelectionChanged, ...props }: Props) {
  const [tableHeight, setTableHeight] = useState(0);
  const OnTableResized = (sizeInfo: { width: number, height: number }) => {
    setTableHeight(sizeInfo.height - (headerHeight ?? 32) - (offset ?? 8));
  };
  const { style, ...otherProps } = props;
  const newStyle = { ...style };
  newStyle.height = '100%';

  const [ selectedRowKeys, setSelectedRowKeys ] = useState<React.Key[]>([]);
  const rowSelection: TableRowSelection<T> = {
    type: "radio",
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[]) => {
      setSelectedRowKeys(newSelectedRowKeys);
      console.log('selectedRowKeys changed: ', newSelectedRowKeys);
      onSelectionChanged?.(newSelectedRowKeys);
    },
  };
  const selectRow = (record : T) => {
    const newSelectedRowKeys = [...selectedRowKeys];
    var recordKey = keySelector(record);
    if (selectedRowKeys.indexOf(recordKey) >= 0) {
      newSelectedRowKeys.splice(newSelectedRowKeys.indexOf(recordKey), 1);
    } else {
      newSelectedRowKeys.push(recordKey);
    }
    setSelectedRowKeys(newSelectedRowKeys);
  };

  return (
    <ResizeObserver onResize={OnTableResized}>
      <Table
        style={newStyle}
        scroll={{ y: tableHeight }}
        rowKey={keySelector}
        rowSelection={rowSelection}
        onRow={(record) => ({
          onClick: () => {
            selectRow(record);
          }
        })}
        {...otherProps}/>
    </ResizeObserver>
  );
}

export default AutoHeightTable;