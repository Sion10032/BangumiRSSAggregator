import React, { useState } from 'react';
import ResizeObserver from 'rc-resize-observer';
import { Table, TableProps } from 'antd';
import { cloneAndUpdateProperty } from '@/shared/utils';

import "./AutoHeightTable.css";

// todo 优化selection相关代码，抽象一个类型 undefined | { type: 'single', ... } | { type: 'multiple', ... }

type Props<T = any> = Omit<TableProps<T>, "rowKey" | "rowSelection" | "style"> & {
  offset?: number;
  headerHeight?: number;
  style?: React.CSSProperties;
  selectionType?: "single" | "multiple";
  keySelector: (obj : T) => React.Key;
  onSelectionChanged? : (keys : React.Key[]) => Promise<void>;
};

type TableRowSelection<T> = TableProps<T>['rowSelection'];

function AutoHeightTable<T = any>({ offset, headerHeight, style, selectionType, keySelector, onSelectionChanged, ...props }: Props) {
  const [ tableHeight, setTableHeight] = useState(0);
  const OnTableResized = (sizeInfo: { width: number, height: number }) => {
    console.log(sizeInfo);
    setTableHeight(sizeInfo.height - (headerHeight ?? 32) - (offset ?? 8));
  };

  const [ selectedRowKeys, setSelectedRowKeys ] = useState<React.Key[]>([]);
  const updateSelectedRowKeys = (keys : React.Key[]) => {
    setSelectedRowKeys(keys);
    console.log('selectedRowKeys changed: ', keys);
    onSelectionChanged?.(keys);
  };
  const rowSelection: TableRowSelection<T> = selectionType === undefined
    ? undefined
    : {
      type: selectionType === "single" ? "radio" : "checkbox",
      selectedRowKeys,
      onChange: updateSelectedRowKeys,
    };
  const onRow = selectionType === undefined
    ? undefined
    : (record : T) => ({
      onClick: () => {
        const newSelectedRowKeys = [...selectedRowKeys];
        var recordKey = keySelector(record);
        if (selectedRowKeys.indexOf(recordKey) >= 0) {
          newSelectedRowKeys.splice(newSelectedRowKeys.indexOf(recordKey), 1);
        } else {
          newSelectedRowKeys.push(recordKey);
        }
        updateSelectedRowKeys(newSelectedRowKeys);
      }
    });

  return (
    <ResizeObserver onResize={OnTableResized}>
      <Table
        style={cloneAndUpdateProperty(style ?? {}, "height", "100%")}
        scroll={{ y: tableHeight }}
        rowKey={keySelector}
        rowSelection={rowSelection}
        onRow={onRow}
        {...props}/>
    </ResizeObserver>
  );
}

export default AutoHeightTable;