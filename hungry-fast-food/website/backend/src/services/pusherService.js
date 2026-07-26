// E:\hungryHub\hungry-fast-food\website\backend\src\services\pusherService.js
// Pusher server-side SDK for broadcasting real-time events to the POS Admin Panel.

import Pusher from 'pusher';
import dotenv from 'dotenv';

dotenv.config();

const pusher = new Pusher({
    appId: process.env.PUSHER_APP_ID,
    key: process.env.PUSHER_APP_KEY,
    secret: process.env.PUSHER_APP_SECRET,
    cluster: process.env.PUSHER_APP_CLUSTER || 'ap2',
    useTLS: true
});

// ─── Channel & Event Constants ────────────────────────────────────────────────
export const CHANNELS = {
    ORDERS: 'orders-channel',
    ADMIN: 'admin-channel'
};

export const EVENTS = {
    NEW_ORDER: 'new-order',
    ORDER_STATUS: 'order-status-update',
    MENU_UPDATED: 'menu-updated'
};

// ─── Trigger new order notification ──────────────────────────────────────────
export const notifyNewOrder = async (orderData) => {
    try {
        await pusher.trigger(CHANNELS.ORDERS, EVENTS.NEW_ORDER, {
            orderId: orderData.id,
            orderNumber: orderData.order_number,
            customerName: orderData.customer_name,
            total: orderData.total,
            type: orderData.order_type,
            status: orderData.status || 'pending',
            timestamp: new Date().toISOString()
        });
        console.log('✅ Pusher: New order notification sent →', orderData.order_number);
    } catch (error) {
        // Non-fatal — log and continue, order is already saved
        console.error('⚠️  Pusher notification failed (non-fatal):', error.message);
    }
};

// ─── Trigger order status update ─────────────────────────────────────────────
export const notifyOrderStatus = async (orderId, status) => {
    try {
        await pusher.trigger(CHANNELS.ORDERS, EVENTS.ORDER_STATUS, {
            orderId,
            status,
            timestamp: new Date().toISOString()
        });
        console.log('✅ Pusher: Order status update sent →', orderId, status);
    } catch (error) {
        console.error('⚠️  Pusher status update failed (non-fatal):', error.message);
    }
};

export default pusher;
