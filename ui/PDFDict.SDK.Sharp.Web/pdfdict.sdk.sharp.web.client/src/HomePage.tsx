import React from 'react';
import { Card, Flex } from 'antd';
import FileUploader from './FileUploader';

const cardStyle: React.CSSProperties = {
    display: 'inline-flex',
    justifyContent: 'center',
    alignItems: 'center',
    width: '50%',
};

const HomePage: React.FC = () => (
    <Card hoverable style={cardStyle} styles={{ body: { padding: 2, overflow: 'hidden' } }}>
        <Flex justify="space-between">
            <Flex>
                <FileUploader />
            </Flex>
        </Flex>
    </Card>
);

export default HomePage;