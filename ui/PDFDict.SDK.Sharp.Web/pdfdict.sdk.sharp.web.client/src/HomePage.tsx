import React from 'react';
import { Col, Divider, Row } from 'antd';

const style: React.CSSProperties = { background: '#0092ff', padding: '8px 0' };

const rowStyle = {
    width: '60%',
    alignContent: 'center',
    margin: 'auto',
};

const HomePage: React.FC = () => (
    <>
        <Divider orientation="left">Horizontal</Divider>
        <Row gutter={16} style={rowStyle}>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
        </Row>
        <Divider orientation="left">Responsive</Divider>
        <Row gutter={{ xs: 8, sm: 16, md: 24, lg: 32 }} style={rowStyle}>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
            <Col className="gutter-row" span={6}>
                <div style={style}>col-6</div>
            </Col>
        </Row>
    </>
);

export default HomePage;