// backend/services/pusherService.js
import Pusher from 'pusher';
import dotenv from 'dotenv';

dotenv.config();

const pusher = new Pusher({
  appId: process.env.PUSHER_APP_ID,
  key: process.env.PUSHER_APP_KEY,
  secret: process.env.PUSHER_APP_SECRET,
  cluster: process.env.PUSHER_APP_CLUSTER,
  useTLS: true
});

// Channel names
const CHANNELS = {
  ORDERS: 'orders-channel',
  ADMIN: 'admin-channel'
};

// Events
const EVENTS = {
  NEW_ORDER: 'new-order',
  ORDER_STATUS: 'order-status-update'
};

// Function to trigger new order notification
export const notifyNewOrder = (orderData) => {
  try {
    pusher.trigger(CHANNELS.ORDERS, EVENTS.NEW_ORDER, {
      orderId: orderData.id,
      orderNumber: orderData.order_number,
      customerName: orderData.customer_name,
      total: orderData.total,
      type: orderData.order_type,
      status: orderData.status,
      timestamp: new Date().toISOString()
    });
    console.log('✅ New order notification sent:', orderData.order_number);
  } catch (error) {
    console.error('❌ Pusher error:', error.message);
  }
};

// Function to trigger order status update
export const notifyOrderStatus = (orderId, status) => {
  try {
    pusher.trigger(CHANNELS.ORDERS, EVENTS.ORDER_STATUS, {
      orderId: orderId,
      status: status,
      timestamp: new Date().toISOString()
    });
    console.log('✅ Order status notification sent:', orderId, status);
  } catch (error) {
    console.error('❌ Pusher error:', error.message);
  }
};

export default pusher;


