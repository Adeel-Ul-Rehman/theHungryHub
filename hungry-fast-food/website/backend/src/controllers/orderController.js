// E:\hungryHub\hungry-fast-food\website\backend\src\controllers\orderController.js

import Order from '../models/Order.js';
import User from '../models/User.js';
import { sendOrderConfirmationEmail, sendOrderStatusEmail } from '../services/emailService.js';
import { generateOrderNumber, calculateDistance, checkDeliveryZone, generateGoogleMapsLink } from '../utils/validators.js';
import { emitSocketEvent } from '../../services/socketService.js';
import { notifyNewOrder } from '../services/pusherService.js';
import { query } from '../config/database.js';

// ============================================
// CREATE ORDER
// ============================================
export const createOrder = async (req, res) => {
    try {
        let {
            order_type,
            customer_name,
            customer_phone,
            customer_email,
            delivery_address,
            delivery_latitude,
            delivery_longitude,
            items,
            payment_method,
            admin_notes
        } = req.body;

        // --- Order Taking Verification ---
        try {
            const settingsRes = await query("SELECT setting_key, setting_value FROM system_settings WHERE setting_key IN ('accept_website_orders', 'use_auto_timing', 'opening_time', 'closing_time')");
            const settings = {};
            settingsRes.rows.forEach(row => { settings[row.setting_key] = row.setting_value; });
            
            const isManualOpen = settings.accept_website_orders !== 'False'; // default true
            const useAutoTiming = settings.use_auto_timing === 'True';
            const openingTime = settings.opening_time || '10:00';
            const closingTime = settings.closing_time || '23:00';
            
            let is_currently_open = true;
            if (!useAutoTiming) {
                is_currently_open = isManualOpen;
            } else {
                const tzDate = new Date(new Date().toLocaleString('en-US', { timeZone: 'Asia/Karachi' }));
                const currentTotalMins = tzDate.getHours() * 60 + tzDate.getMinutes();
                const [openH, openM] = openingTime.split(':').map(Number);
                const [closeH, closeM] = closingTime.split(':').map(Number);
                const openTotalMins = openH * 60 + openM;
                const closeTotalMins = closeH * 60 + closeM;
                if (closeTotalMins > openTotalMins) {
                    is_currently_open = currentTotalMins >= openTotalMins && currentTotalMins <= closeTotalMins;
                } else {
                    is_currently_open = currentTotalMins >= openTotalMins || currentTotalMins <= closeTotalMins;
                }
            }
            
            if (!is_currently_open) {
                return res.status(400).json({
                    success: false,
                    message: `The restaurant is closed right now. You can place your order after ${openingTime}`
                });
            }
        } catch (dbErr) {
            console.warn('Could not read store open status:', dbErr.message);
        }
        // ----------------------------------

        // For logged-in users, get info from user
        let userId = null;
        let email = customer_email;

        if (req.user) {
            userId = req.user.id;
            const user = await User.findById(userId);
            if (user) {
                if (!customer_name) customer_name = user.full_name;
                if (!email) email = user.email;
                if (!customer_phone) customer_phone = user.phone;
            }
        }

        // Calculate items and subtotal first (needed for minOrder validation)
        let subtotal = 0;
        const orderItems = items.map(item => {
            const total = item.unit_price * item.quantity;
            subtotal += total;
            return {
                ...item,
                total_price: total
            };
        });

        // Check delivery zone + enforce admin-defined rules for ALL delivery orders.
        let deliveryCharge = 0;
        if (order_type === 'delivery') {
            if (!delivery_address) {
                return res.status(400).json({
                    success: false,
                    message: 'Delivery orders require a delivery address.'
                });
            }

            // We enforce coordinates only if possible, but allow order placement if refused as last resort
            if (delivery_latitude && delivery_longitude) {
                const distance = calculateDistance(
                    parseFloat(process.env.RESTAURANT_LATITUDE || '33.5651'),
                    parseFloat(process.env.RESTAURANT_LONGITUDE || '73.0169'),
                    delivery_latitude,
                    delivery_longitude
                );

                const zoneCheck = await checkDeliveryZone(distance);
                if (!zoneCheck.allowed) {
                    return res.status(400).json({
                        success: false,
                        message: zoneCheck.message
                    });
                }
                deliveryCharge = zoneCheck.charge || 0;

                // Effective minimum order = max(zone-specific minOrder, global min_order setting)
                let globalMinOrder = 0;
                try {
                    const settingRes = await query(
                        `SELECT setting_value FROM system_settings WHERE setting_key = 'min_order'`
                    );
                    if (settingRes.rows.length > 0) {
                        const parsed = parseFloat(settingRes.rows[0].setting_value);
                        if (!isNaN(parsed)) globalMinOrder = parsed;
                    }
                } catch (dbErr) {
                    console.warn('⚠️ Could not read global min_order setting:', dbErr.message);
                }

                const effectiveMinOrder = Math.max(zoneCheck.minOrder || 0, globalMinOrder);

                // Validate minimum order against admin-defined rules
                if (effectiveMinOrder > 0 && subtotal < effectiveMinOrder) {
                    const ruleSource = zoneCheck.minOrder > 0 ? `your ${zoneCheck.zoneName}` : 'the restaurant';
                    return res.status(400).json({
                        success: false,
                        message: `Minimum order for ${ruleSource} is ${effectiveMinOrder} PKR. Your order subtotal is ${subtotal} PKR. Please add more items.`,
                        minOrder: effectiveMinOrder,
                        currentSubtotal: subtotal
                    });
                }
            } else {
                // Last resort fallback (user refused location sharing)
                // Determine delivery fee from dynamic zones cache fallback
                try {
                    const zones = await getDeliveryZones();
                    // Find first zone with a charge > 0
                    const chargedZone = zones.find(z => parseFloat(z.charge || z.Charge || 0) > 0);
                    deliveryCharge = chargedZone ? parseFloat(chargedZone.charge || chargedZone.Charge) : 150;
                } catch {
                    deliveryCharge = 150;
                }

                // Enforce global minimum order setting if any
                let globalMinOrder = 0;
                try {
                    const settingRes = await query(
                        `SELECT setting_value FROM system_settings WHERE setting_key = 'min_order'`
                    );
                    if (settingRes.rows.length > 0) {
                        const parsed = parseFloat(settingRes.rows[0].setting_value);
                        if (!isNaN(parsed)) globalMinOrder = parsed;
                    }
                } catch (dbErr) {
                    console.warn('⚠️ Could not read global min_order setting:', dbErr.message);
                }

                if (globalMinOrder > 0 && subtotal < globalMinOrder) {
                    return res.status(400).json({
                        success: false,
                        message: `Minimum order for the restaurant is ${globalMinOrder} PKR. Your order subtotal is ${subtotal} PKR. Please add more items.`,
                        minOrder: globalMinOrder,
                        currentSubtotal: subtotal
                    });
                }
            }
        }

        // Calculate tax dynamically from settings
        let taxRate = 0; // fallback
        try {
            const taxSettingRes = await query(`SELECT setting_value FROM system_settings WHERE setting_key = 'tax_rate'`);
            if (taxSettingRes.rows.length > 0) {
                const parsedRate = parseFloat(taxSettingRes.rows[0].setting_value);
                if (!isNaN(parsedRate)) {
                    taxRate = parsedRate / 100;
                }
            }
        } catch (dbErr) {
            console.warn('⚠️ Could not read tax_rate setting:', dbErr.message);
        }
        
        const tax = subtotal * taxRate;
        const total = subtotal + tax + deliveryCharge;

        // Generate order number
        const orderNumber = generateOrderNumber(order_type);

        // Generate Google Maps link for delivery orders
        let mapsUrl = null;
        if (order_type === 'delivery') {
            mapsUrl = generateGoogleMapsLink(delivery_address, delivery_latitude, delivery_longitude);
        }

        // Create order
        const orderData = {
            order_number: orderNumber,
            order_type,
            user_id: userId,
            customer_name,
            customer_phone,
            customer_email: email,
            delivery_address,
            delivery_latitude,
            delivery_longitude,
            subtotal,
            delivery_charge: deliveryCharge,
            tax,
            total,
            payment_method,
            payment_status: payment_method === 'jazzcash' ? 'pending' : 'pending',
            admin_notes,
            maps_url: mapsUrl
        };

        const order = await Order.create(orderData, orderItems);

        // Send confirmation email if email provided
        if (email) {
            await sendOrderConfirmationEmail(email, orderNumber, customer_name, total);
        }

        emitSocketEvent('order_placed', {
            id: order.id,
            order_number: orderNumber,
            customer_name,
            total,
            order_type,
            status: order.status || 'pending',
            created_at: order.created_at || new Date().toISOString()
        });

        // Pusher: zero-latency real-time push to POS Admin Panel
        notifyNewOrder({
            id: order.id,
            order_number: orderNumber,
            customer_name,
            total,
            order_type,
            status: order.status || 'pending'
        });

        res.status(201).json({
            success: true,
            message: 'Order created successfully',
            data: {
                order: order,
                orderNumber: orderNumber
            }
        });
    } catch (error) {
        console.error('Create order error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to create order',
            error: error.message
        });
    }
};

// ============================================
// GET ORDER BY ID
// ============================================
export const getOrderById = async (req, res) => {
    try {
        const { id } = req.params;

        const order = await Order.findById(id);
        if (!order) {
            return res.status(404).json({
                success: false,
                message: 'Order not found'
            });
        }

        // Check if user is authorized (admin or order owner)
        if (req.user) {
            const adminEmails = (process.env.ADMIN_EMAILS || '').split(',');
            const isAdmin = req.user.email && adminEmails.includes(req.user.email);
            
            if (!isAdmin && order.user_id && order.user_id !== req.user.id) {
                return res.status(403).json({
                    success: false,
                    message: 'Unauthorized to view this order'
                });
            }
        } else if (order.user_id) {
            return res.status(401).json({
                success: false,
                message: 'Authentication required to view this order'
            });
        }

        res.status(200).json({
            success: true,
            data: order
        });
    } catch (error) {
        console.error('Get order error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get order',
            error: error.message
        });
    }
};

// ============================================
// GET ORDER BY NUMBER
// ============================================
export const getOrderByNumber = async (req, res) => {
    try {
        const { orderNumber } = req.params;

        const order = await Order.findByOrderNumber(orderNumber);
        if (!order) {
            return res.status(404).json({
                success: false,
                message: 'Order not found'
            });
        }

        res.status(200).json({
            success: true,
            data: order
        });
    } catch (error) {
        console.error('Get order by number error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get order',
            error: error.message
        });
    }
};

// ============================================
// GET USER ORDERS
// ============================================
export const getUserOrders = async (req, res) => {
    try {
        const userId = req.user.id;
        const { status, limit = 20, offset = 0 } = req.query;

        const orders = await Order.getOrders({
            user_id: userId,
            status,
            limit: parseInt(limit),
            offset: parseInt(offset)
        });

        res.status(200).json({
            success: true,
            data: orders,
            pagination: {
                limit: parseInt(limit),
                offset: parseInt(offset)
            }
        });
    } catch (error) {
        console.error('Get user orders error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get orders',
            error: error.message
        });
    }
};

// ============================================
// GET ALL ORDERS (ADMIN)
// ============================================
export const getOrders = async (req, res) => {
    try {
        const {
            status,
            order_type,
            start_date,
            end_date,
            is_suspicious,
            limit = 50,
            offset = 0
        } = req.query;

        const orders = await Order.getOrders({
            status,
            order_type,
            start_date,
            end_date,
            is_suspicious: is_suspicious === 'true',
            limit: parseInt(limit),
            offset: parseInt(offset)
        });

        res.status(200).json({
            success: true,
            data: orders,
            pagination: {
                limit: parseInt(limit),
                offset: parseInt(offset)
            }
        });
    } catch (error) {
        console.error('Get orders error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get orders',
            error: error.message
        });
    }
};

// ============================================
// UPDATE ORDER STATUS (ADMIN)
// ============================================
export const updateOrderStatus = async (req, res) => {
    try {
        const { id } = req.params;
        const { status } = req.body;
        const adminEmail = req.user ? req.user.email : 'admin';

        // Check if order exists
        const order = await Order.findById(id);
        if (!order) {
            return res.status(404).json({
                success: false,
                message: 'Order not found'
            });
        }

        // Prevent status changes for cancelled/completed orders
        if (order.status === 'cancelled') {
            return res.status(400).json({
                success: false,
                message: 'Cannot update cancelled order'
            });
        }

        // Update status
        const updatedOrder = await Order.updateStatus(id, status, adminEmail);

        // Send email notification if customer provided email
        if (order.customer_email) {
            await sendOrderStatusEmail(
                order.customer_email,
                order.order_number,
                status,
                order.customer_name
            );
        }

        emitSocketEvent('order_status_updated', {
            id: updatedOrder.id,
            status,
            order_number: updatedOrder.order_number,
            updated_at: updatedOrder.updated_at || new Date().toISOString()
        });

        res.status(200).json({
            success: true,
            message: 'Order status updated',
            data: updatedOrder
        });
    } catch (error) {
        console.error('Update order status error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update order status',
            error: error.message
        });
    }
};

// ============================================
// CANCEL ORDER
// ============================================
export const cancelOrder = async (req, res) => {
    try {
        const { id } = req.params;
        const { reason, admin_email } = req.body;
        const adminEmail = admin_email || (req.user ? req.user.email : 'admin');

        // Check if order exists
        const order = await Order.findById(id);
        if (!order) {
            return res.status(404).json({
                success: false,
                message: 'Order not found'
            });
        }

        // Check if order can be cancelled
        if (order.status === 'completed' || order.status === 'cancelled') {
            return res.status(400).json({
                success: false,
                message: `Cannot cancel order with status: ${order.status}`
            });
        }

        // If order is completed, mark as suspicious
        if (order.status === 'completed') {
            await Order.markSuspicious(id, adminEmail);
        }

        // Update status to cancelled
        const updatedOrder = await Order.updateStatus(id, 'cancelled', adminEmail);

        // Send email notification
        if (order.customer_email) {
            await sendOrderStatusEmail(
                order.customer_email,
                order.order_number,
                'cancelled',
                order.customer_name,
                reason
            );
        }

        res.status(200).json({
            success: true,
            message: 'Order cancelled successfully',
            data: updatedOrder
        });
    } catch (error) {
        console.error('Cancel order error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to cancel order',
            error: error.message
        });
    }
};

// ============================================
// MARK AS SUSPICIOUS (ADMIN)
// ============================================
export const markSuspicious = async (req, res) => {
    try {
        const { id } = req.params;
        const adminEmail = req.user ? req.user.email : 'admin';

        const order = await Order.markSuspicious(id, adminEmail);
        if (!order) {
            return res.status(404).json({
                success: false,
                message: 'Order not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Order marked as suspicious',
            data: order
        });
    } catch (error) {
        console.error('Mark suspicious error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to mark order as suspicious',
            error: error.message
        });
    }
};

// ============================================
// GET ORDER STATS (ADMIN)
// ============================================
export const getOrderStats = async (req, res) => {
    try {
        const { start_date, end_date } = req.query;

        if (!start_date || !end_date) {
            return res.status(400).json({
                success: false,
                message: 'start_date and end_date are required'
            });
        }

        const stats = await Order.getStats(start_date, end_date);

        res.status(200).json({
            success: true,
            data: stats
        });
    } catch (error) {
        console.error('Get order stats error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get order statistics',
            error: error.message
        });
    }
};

// ============================================
// GET DAILY REPORT (ADMIN)
// ============================================
export const getDailyReport = async (req, res) => {
    try {
        const { date } = req.query;
        const reportDate = date || new Date().toISOString().split('T')[0];

        const report = await Order.getDailyReport(reportDate);

        res.status(200).json({
            success: true,
            data: report || { total_orders: 0, total_revenue: 0 }
        });
    } catch (error) {
        console.error('Get daily report error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get daily report',
            error: error.message
        });
    }
};

// ============================================
// CHECK DELIVERY AVAILABILITY
// ============================================
export const checkDelivery = async (req, res) => {
    try {
        const { lat, lng } = req.query;

        if (!lat || !lng) {
            return res.status(400).json({
                success: false,
                message: 'Latitude and longitude are required'
            });
        }

        const distance = calculateDistance(
            parseFloat(process.env.RESTAURANT_LATITUDE),
            parseFloat(process.env.RESTAURANT_LONGITUDE),
            parseFloat(lat),
            parseFloat(lng)
        );

        const zoneCheck = await checkDeliveryZone(distance);

        // Include the global min_order setting in the effective minimum so the
        // frontend warning stays in sync with backend enforcement.
        let globalMinOrder = 0;
        try {
            const settingRes = await query(
                `SELECT setting_value FROM system_settings WHERE setting_key = 'min_order'`
            );
            if (settingRes.rows.length > 0) {
                const parsed = parseFloat(settingRes.rows[0].setting_value);
                if (!isNaN(parsed)) globalMinOrder = parsed;
            }
        } catch (dbErr) {
            console.warn('⚠️ Could not read global min_order setting:', dbErr.message);
        }

        const effectiveMinOrder = Math.max(zoneCheck.minOrder || 0, globalMinOrder);

        res.status(200).json({
            success: true,
            data: {
                distance: distance,
                allowed: zoneCheck.allowed,
                charge: zoneCheck.charge || 0,
                minOrder: effectiveMinOrder,
                zoneMinOrder: zoneCheck.minOrder || 0,
                globalMinOrder,
                zoneName: zoneCheck.zoneName || null,
                isPaidZone: (zoneCheck.charge || 0) > 0,
                message: zoneCheck.message
            }
        });
    } catch (error) {
        console.error('Check delivery error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to check delivery availability',
            error: error.message
        });
    }
};
