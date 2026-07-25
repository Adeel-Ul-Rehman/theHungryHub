// frontend/src/services/pusherService.js
import Pusher from 'pusher-js';

let pusher = null;
let channel = null;

export const initializePusher = () => {
    // Agar real-time disable hai toh return
    if (import.meta.env.VITE_ENABLE_REALTIME !== 'true') {
        console.log('ℹ️ Real-time notifications disabled');
        return null;
    }

    try {
        pusher = new Pusher(import.meta.env.VITE_PUSHER_APP_KEY, {
            cluster: import.meta.env.VITE_PUSHER_APP_CLUSTER,
            forceTLS: true,
            enabledTransports: ['ws', 'wss']
        });

        channel = pusher.subscribe('orders-channel');

        console.log('✅ Pusher connected successfully');
        return { pusher, channel };
    } catch (error) {
        console.error('❌ Pusher connection failed:', error.message);
        return null;
    }
};

export const subscribeToNewOrders = (callback) => {
    if (!channel) {
        console.warn('⚠️ Pusher not initialized');
        return () => { };
    }

    const handler = (data) => {
        console.log('📦 New order received:', data);
        callback(data);
    };

    channel.bind('new-order', handler);

    // Unsubscribe function return karein
    return () => {
        channel.unbind('new-order', handler);
    };
};

export const subscribeToOrderStatus = (callback) => {
    if (!channel) {
        console.warn('⚠️ Pusher not initialized');
        return () => { };
    }

    const handler = (data) => {
        console.log('🔄 Order status updated:', data);
        callback(data);
    };

    channel.bind('order-status-update', handler);

    return () => {
        channel.unbind('order-status-update', handler);
    };
};

export const subscribeToSettingsUpdate = (callback) => {
    if (!channel) {
        return () => { };
    }

    const handler = (data) => {
        console.log('🔄 Settings updated:', data);
        callback(data);
    };

    channel.bind('settings_updated', handler);

    return () => {
        channel.unbind('settings_updated', handler);
    };
};

export const disconnectPusher = () => {
    if (pusher) {
        pusher.disconnect();
        console.log('🔌 Pusher disconnected');
    }
};