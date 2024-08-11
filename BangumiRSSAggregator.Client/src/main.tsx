import React from 'react';
import ReactDOM from 'react-dom/client';
import { ConfigProvider, ThemeConfig } from 'antd';

import App from './App.tsx';
import './index.css';

const theme : ThemeConfig = {
  components: {
  },
};

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ConfigProvider theme={theme}>
      <App />
    </ConfigProvider>
  </React.StrictMode>,
);
