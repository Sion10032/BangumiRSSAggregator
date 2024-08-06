import { useState } from 'react';
import ResizeObserver from 'rc-resize-observer';
import { Table, TableProps } from 'antd';

type Props = TableProps & {
  offset?: number;
  headerHeight?: number;
};

function AutoHeightTable({ offset, headerHeight, ...props }: Props) {
  const [tableHeight, setTableHeight] = useState(0);
  const OnTableResized = (sizeInfo: { width: number, height: number }) => {
    setTableHeight(sizeInfo.height - (headerHeight ?? 32) - (offset ?? 8));
  };
  const { style, ...otherProps } = props;
  const newStyle = { ...style };
  newStyle.height = '100%';

  return (
    <ResizeObserver onResize={OnTableResized}>
      <Table
        style={newStyle}
        scroll={{ y: tableHeight }}
        {...otherProps}/>
    </ResizeObserver>
  );
}

export default AutoHeightTable;