import React from 'react';
import { Flex, Layout } from 'antd';
import HomePage from './HomePage';

const { Header, Footer, Sider, Content } = Layout;

const headerStyle: React.CSSProperties = {
    textAlign: 'center',
    color: '#fff',
    height: 64,
    paddingInline: 48,
    lineHeight: '64px',
    // backgroundColor: '#4096ff',
};

const contentStyle: React.CSSProperties = {
    textAlign: 'center',
    minHeight: 120,
    lineHeight: '120px',
    color: '#fff',
    //   backgroundColor: '#0958d9',
};

const siderStyle: React.CSSProperties = {
    textAlign: 'center',
    lineHeight: '120px',
    color: '#fff',
    //   backgroundColor: '#1677ff',
};

const footerStyle: React.CSSProperties = {
    textAlign: 'center',
    color: '#fff',
    //   backgroundColor: '#4096ff',
};

const layoutStyle = {
    borderRadius: 8,
    height: '100vh',
    overflow: 'hidden',
};

const App: React.FC = () => (
    <Flex gap="middle" wrap>
        <Layout style={layoutStyle}>
            <Header style={headerStyle}> </Header>
            <Layout>
                <Sider width="10%" style={siderStyle}>
                    
                </Sider>
                <Content style={contentStyle}>
                    <HomePage />
                </Content>
            </Layout>
            <Footer style={footerStyle}> </Footer>
        </Layout>
    </Flex>
);

export default App;