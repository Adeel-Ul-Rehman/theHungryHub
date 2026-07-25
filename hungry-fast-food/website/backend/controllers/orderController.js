// backend/controllers/orderController.js
import { notifyNewOrder } from '../services/pusherService.js';

export const createOrder = async (req, res) => {
    try {
        // ... order create logic ...

        const newOrder = await order.save();

        // 🔔 Push notification send karein
        notifyNewOrder(newOrder);

        res.status(201).json({
            success: true,
            order: newOrder
        });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
};