//import { useState } from 'react';
import { Divider, Layout, Typography } from "antd";
import { Col, Row } from 'antd';
import { Card } from 'antd';
import 'antd/dist/reset.css';

//import reactLogo from './assets/react.svg';
//import viteLogo from '/vite.svg';
import '@/App.css';
import Feeds from "@/components/feeds/Feeds";
import Rules from "./components/rules/Rules";


const { Title } = Typography;
const { Header, /*Footer,*/ Content } = Layout;

function App() {
  return (
    <Layout>
      <Header style={{ display: 'flex', alignItems: 'center' }}>
        <Title level={2} style={{ color: "white" }}>Bangumi RSS Aggregator</Title>
      </Header>
      <Content>
        <Row 
          gutter={[8, 8]} 
          justify="space-around" 
          style={{ padding: '8px' }}>
          <Col xs={24} sm={24} md={24} lg={8} xl={8} xxl={8}>
            <Feeds/>
          </Col>
          <Col xs={24} sm={24} md={24} lg={8} xl={8} xxl={8}>
            <Rules/>
          </Col>
          <Col xs={24} sm={24} md={24} lg={8} xl={8} xxl={8}>
            <Card title="Groups" className="flexible-card">
            </Card>
          </Col>
        </Row>
        <Divider/>
        <Row gutter={[8, 8]} style={{ padding: '8px' }}>
          <Col span={24}>
            <Card title="Animes" className="flexible-card">
            </Card>
          </Col>
        </Row>
      </Content>
      {/*<Footer>Footer</Footer>*/}
    </Layout>
  )
}

export default App
