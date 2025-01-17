import React, { useState } from 'react';
import type { RadioChangeEvent, TabsProps } from 'antd';
import { Radio, Tabs } from 'antd';
import HomePage from './HomePage';
import PageViewTab from './PageViewTab';

type TargetKey = React.MouseEvent | React.KeyboardEvent | string;

const ViewerTabs: React.FC = () => {
    const [size, setSize] = useState<'small' | 'middle' | 'large'>('small');
    const [activeKey, setActiveKey] = useState('1');
    const [items, setItems] = useState<TabsProps['items']>([
        {
            label: 'Home',
            key: '1',
            closeIcon: false,
            children: <HomePage />,
        },
        {
            label: 'Tab 2',
            key: '2',
            children: <PageViewTab />,
        },
        {
            label: 'Tab 3',
            key: '3',
            children: 'Content of editable tab 3',
        },
    ]);

    const add = () => {
        const newKey = String((items || []).length + 1);
        setItems([
            ...(items || []),
            {
                label: `Tab ${newKey}`,
                key: newKey,
                children: `Content of editable tab ${newKey}`,
            },
        ]);
        setActiveKey(newKey);
    };

    const remove = (targetKey: TargetKey) => {
        if (!items) return;
        const targetIndex = items.findIndex((item) => item.key === targetKey);
        const newItems = items.filter((item) => item.key !== targetKey);

        if (newItems.length && targetKey === activeKey) {
            const newActiveKey =
                newItems[targetIndex === newItems.length ? targetIndex - 1 : targetIndex].key;
            setActiveKey(newActiveKey);
        }

        setItems(newItems);
    };

    const onEdit = (targetKey: TargetKey, action: 'add' | 'remove') => {
        if (action === 'add') {
            add();
        } else {
            remove(targetKey);
        }
    };

    const onChange = (e: RadioChangeEvent) => {
        setSize(e.target.value);
    };

    return (
        <div>
            <Tabs
                type="editable-card"
                size={size}
                activeKey={activeKey}
                onChange={setActiveKey}
                onEdit={onEdit}
                items={items}
                hideAdd={true}
            />
        </div>
    );
};

export default ViewerTabs;