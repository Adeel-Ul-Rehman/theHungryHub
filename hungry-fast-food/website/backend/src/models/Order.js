// E:\hungryHub\hungry-fast-food\website\backend\src\models\Order.js

import pool, { query } from '../config/database.js';

class Order {
    // Create order
    static async create(orderData, items) {
        const client = await pool.connect();

        try {
            await client.query('BEGIN');

            // Insert order
            const orderResult = await client.query(
                `INSERT INTO orders (
          order_number, order_type, user_id, customer_name, customer_phone,
          customer_email, delivery_address, delivery_latitude, delivery_longitude,
          status, subtotal, delivery_charge, tax, total, payment_method,
          payment_status, admin_notes, maps_url
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18)
        RETURNING *`,
                [
                    orderData.order_number,
                    orderData.order_type,
                    orderData.user_id || null,
                    orderData.customer_name,
                    orderData.customer_phone,
                    orderData.customer_email,
                    orderData.delivery_address || null,
                    orderData.delivery_latitude || null,
                    orderData.delivery_longitude || null,
                    'pending',
                    orderData.subtotal,
                    orderData.delivery_charge || 0,
                    orderData.tax || 0,
                    orderData.total,
                    orderData.payment_method || 'cash',
                    orderData.payment_status || 'pending',
                    orderData.admin_notes || null,
                    orderData.maps_url || null
                ]
            );

            const order = orderResult.rows[0];

            // Insert order items
            for (const item of items) {
                await client.query(
                    `INSERT INTO order_items (
            order_id, product_name, variation_name, quantity,
            unit_price, total_price, is_from_deal, deal_id
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)`,
                    [
                        order.id,
                        item.product_name,
                        item.variation_name || null,
                        item.quantity,
                        item.unit_price,
                        item.total_price,
                        item.is_from_deal || false,
                        item.deal_id || null
                    ]
                );
            }

            await client.query('COMMIT');
            return order;
        } catch (error) {
            await client.query('ROLLBACK');
            throw error;
        } finally {
            client.release();
        }
    }

    // Get order by ID
    static async findById(id) {
        const result = await query(
            `SELECT o.*, 
        COALESCE(
          json_agg(
            json_build_object(
              'id', oi.id,
              'product_name', oi.product_name,
              'variation_name', oi.variation_name,
              'quantity', oi.quantity,
              'unit_price', oi.unit_price,
              'total_price', oi.total_price,
              'is_from_deal', oi.is_from_deal
            ) ORDER BY oi.id
          ) FILTER (WHERE oi.id IS NOT NULL), '[]'
        ) as items
       FROM orders o
       LEFT JOIN order_items oi ON o.id = oi.order_id
       WHERE o.id = $1
       GROUP BY o.id`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Get order by order number
    static async findByOrderNumber(orderNumber) {
        const result = await query(
            `SELECT o.*, 
        COALESCE(
          json_agg(
            json_build_object(
              'id', oi.id,
              'product_name', oi.product_name,
              'variation_name', oi.variation_name,
              'quantity', oi.quantity,
              'unit_price', oi.unit_price,
              'total_price', oi.total_price,
              'is_from_deal', oi.is_from_deal
            ) ORDER BY oi.id
          ) FILTER (WHERE oi.id IS NOT NULL), '[]'
        ) as items
       FROM orders o
       LEFT JOIN order_items oi ON o.id = oi.order_id
       WHERE o.order_number = $1
       GROUP BY o.id`,
            [orderNumber]
        );
        return result.rows[0] || null;
    }

    // Get orders with filters
    static async getOrders(filters = {}) {
        const conditions = [];
        const values = [];
        let index = 1;

        if (filters.status) {
            conditions.push(`status = $${index++}`);
            values.push(filters.status);
        }
        if (filters.order_type) {
            conditions.push(`order_type = $${index++}`);
            values.push(filters.order_type);
        }
        if (filters.start_date) {
            conditions.push(`created_at >= $${index++}`);
            values.push(filters.start_date);
        }
        if (filters.end_date) {
            conditions.push(`created_at <= $${index++}`);
            values.push(filters.end_date);
        }
        if (filters.user_id) {
            conditions.push(`user_id = $${index++}`);
            values.push(filters.user_id);
        }
        if (filters.is_suspicious !== undefined) {
            conditions.push(`is_suspicious = $${index++}`);
            values.push(filters.is_suspicious);
        }

        const whereClause = conditions.length > 0
            ? `WHERE ${conditions.join(' AND ')}`
            : '';

        const limit = filters.limit || 50;
        const offset = filters.offset || 0;

        const result = await query(
            `SELECT o.*, 
        COALESCE(
          json_agg(
            json_build_object(
              'id', oi.id,
              'product_name', oi.product_name,
              'variation_name', oi.variation_name,
              'quantity', oi.quantity,
              'unit_price', oi.unit_price,
              'total_price', oi.total_price,
              'is_from_deal', oi.is_from_deal
            ) ORDER BY oi.id
          ) FILTER (WHERE oi.id IS NOT NULL), '[]'
        ) as items
       FROM orders o
       LEFT JOIN order_items oi ON o.id = oi.order_id
       ${whereClause}
       GROUP BY o.id
       ORDER BY o.created_at DESC
       LIMIT $${index++} OFFSET $${index++}`,
            [...values, limit, offset]
        );

        return result.rows;
    }

    // Update order status
    static async updateStatus(id, status, adminEmail = null) {
        const result = await query(
            `UPDATE orders 
       SET status = $1, updated_at = CURRENT_TIMESTAMP
       WHERE id = $2
       RETURNING *`,
            [status, id]
        );

        // Log admin activity if adminEmail provided
        if (adminEmail && result.rows[0]) {
            await query(
                `INSERT INTO admin_activity_logs (admin_email, action, details)
         VALUES ($1, $2, $3)`,
                [adminEmail, 'order_status_update', { order_id: id, new_status: status }]
            );
        }

        return result.rows[0] || null;
    }

    // Mark order as suspicious
    static async markSuspicious(id, adminEmail) {
        const result = await query(
            `UPDATE orders 
       SET is_suspicious = true, updated_at = CURRENT_TIMESTAMP
       WHERE id = $1
       RETURNING *`,
            [id]
        );

        if (result.rows[0]) {
            await query(
                `INSERT INTO admin_activity_logs (admin_email, action, details)
         VALUES ($1, $2, $3)`,
                [adminEmail, 'mark_suspicious', { order_id: id }]
            );
        }

        return result.rows[0] || null;
    }

    // Get order statistics
    static async getStats(startDate, endDate) {
        const result = await query(
            `SELECT 
        COUNT(*) as total_orders,
        SUM(CASE WHEN order_type = 'delivery' THEN 1 ELSE 0 END) as delivery_orders,
        SUM(CASE WHEN order_type = 'dining' THEN 1 ELSE 0 END) as dining_orders,
        SUM(CASE WHEN order_type = 'takeaway' THEN 1 ELSE 0 END) as takeaway_orders,
        SUM(total) as total_revenue,
        AVG(total) as average_order_value
       FROM orders
       WHERE created_at BETWEEN $1 AND $2
       AND status != 'cancelled'`,
            [startDate, endDate]
        );
        return result.rows[0];
    }

    // Get daily sales report
    static async getDailyReport(date) {
        const result = await query(
            `SELECT 
        DATE(created_at) as order_date,
        COUNT(*) as total_orders,
        SUM(total) as total_revenue,
        SUM(CASE WHEN payment_method = 'jazzcash' THEN total ELSE 0 END) as online_payment,
        SUM(CASE WHEN payment_method = 'cod' THEN total ELSE 0 END) as cod_payment,
        SUM(CASE WHEN payment_method = 'cash' THEN total ELSE 0 END) as cash_payment
       FROM orders
       WHERE DATE(created_at) = $1
       AND status != 'cancelled'
       GROUP BY DATE(created_at)`,
            [date]
        );
        return result.rows[0] || null;
    }
}

export default Order;