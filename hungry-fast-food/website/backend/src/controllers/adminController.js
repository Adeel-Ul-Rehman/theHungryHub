// E:\hungryHub\hungry-fast-food\website\backend\src\controllers\adminController.js

import User from '../models/User.js';
import Order from '../models/Order.js';
import Product from '../models/Product.js';
import pool, { query } from '../config/database.js';
import { emitSocketEvent } from '../../services/socketService.js';
import { clearDeliveryZoneCache } from '../utils/validators.js';

// ============================================
// DASHBOARD STATS
// ============================================
export const getDashboardStats = async (req, res) => {
    try {
        // Get today's stats
        const today = new Date().toISOString().split('T')[0];
        const startDate = new Date();
        startDate.setDate(startDate.getDate() - 30);

        const [totalUsers, totalOrders, todayOrders, revenueStats] = await Promise.all([
            User.count(),
            Order.getStats(startDate, new Date()),
            Order.getDailyReport(today),
            getRevenueStats()
        ]);

        res.status(200).json({
            success: true,
            data: {
                total_users: totalUsers,
                total_orders: totalOrders?.total_orders || 0,
                total_revenue: totalOrders?.total_revenue || 0,
                today_orders: todayOrders?.total_orders || 0,
                today_revenue: todayOrders?.total_revenue || 0,
                average_order_value: totalOrders?.average_order_value || 0
            }
        });
    } catch (error) {
        console.error('Get dashboard stats error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get dashboard stats',
            error: error.message
        });
    }
};

// ============================================
// VERIFY ADMIN PASSWORD
// ============================================
export const verifyAdminPassword = async (req, res) => {
    try {
        const { email, password } = req.body;
        
        if (!email || !password) {
            return res.status(400).json({ success: false, message: 'Email and password required' });
        }

        const user = await User.findByEmail(email);
        if (!user) {
            return res.status(401).json({ success: false, message: 'Invalid credentials' });
        }

        // We assume verifyAdmin middleware already ran, but we double check if user is admin based on ADMIN_EMAILS
        const adminEmails = (process.env.ADMIN_EMAILS || '').split(',');
        if (!adminEmails.includes(user.email)) {
            return res.status(403).json({ success: false, message: 'Unauthorized: Not an admin account' });
        }

        // Dynamic import of validators to avoid circular dependency
        const { comparePassword } = await import('../utils/validators.js');
        const isMatch = await comparePassword(password, user.password_hash);
        
        if (!isMatch) {
            return res.status(401).json({ success: false, message: 'Invalid password' });
        }

        return res.status(200).json({ success: true, message: 'Password verified' });
    } catch (error) {
        console.error('Verify admin password error:', error);
        res.status(500).json({ success: false, message: 'Failed to verify password' });
    }
};

// ============================================
// GET ADMIN PROFILE
// ============================================
export const getAdminProfile = async (req, res) => {
    try {
        const adminEmail = req.query.email;
        if (!adminEmail) {
            return res.status(400).json({ success: false, message: 'Email required' });
        }
        
        const user = await User.findByEmail(adminEmail);
        if (!user) {
            return res.status(404).json({ success: false, message: 'Admin not found' });
        }

        res.status(200).json({
            success: true,
            data: {
                id: user.id,
                email: user.email,
                full_name: user.full_name,
                phone: user.phone
            }
        });
    } catch (error) {
        console.error('Get admin profile error:', error);
        res.status(500).json({ success: false, message: 'Failed to get profile' });
    }
};

// ============================================
// UPDATE ADMIN PROFILE
// ============================================
export const updateAdminProfile = async (req, res) => {
    try {
        const adminEmail = req.body.email;
        const { full_name, phone } = req.body;
        
        if (!adminEmail) {
            return res.status(400).json({ success: false, message: 'Email required' });
        }
        
        const user = await User.findByEmail(adminEmail);
        if (!user) {
            return res.status(404).json({ success: false, message: 'Admin not found' });
        }

        const updatedUser = await User.update(user.id, { full_name, phone });
        
        res.status(200).json({
            success: true,
            message: 'Profile updated successfully',
            data: {
                email: updatedUser.email,
                full_name: updatedUser.full_name,
                phone: updatedUser.phone
            }
        });
    } catch (error) {
        console.error('Update admin profile error:', error);
        res.status(500).json({ success: false, message: 'Failed to update profile' });
    }
};

// ============================================
// CHANGE ADMIN PASSWORD
// ============================================
export const changeAdminPassword = async (req, res) => {
    try {
        const { email, oldPassword, newPassword } = req.body;
        
        if (!email || !oldPassword || !newPassword) {
            return res.status(400).json({ success: false, message: 'Missing required fields' });
        }
        
        const user = await User.findByEmail(email);
        if (!user) {
            return res.status(404).json({ success: false, message: 'Admin not found' });
        }

        const { comparePassword, hashPassword } = await import('../utils/validators.js');
        
        // Verify old password
        const isMatch = await comparePassword(oldPassword, user.password_hash);
        if (!isMatch) {
            return res.status(401).json({ success: false, message: 'Incorrect current password' });
        }
        
        // Hash and save new password
        const hashedPassword = await hashPassword(newPassword);
        await User.update(user.id, { password_hash: hashedPassword });
        
        res.status(200).json({ success: true, message: 'Password changed successfully' });
    } catch (error) {
        console.error('Change admin password error:', error);
        res.status(500).json({ success: false, message: 'Failed to change password' });
    }
};

// ============================================
// GET ALL USERS (ADMIN)
// ============================================
export const getUsers = async (req, res) => {
    try {
        const { limit = 50, offset = 0 } = req.query;
        const users = await User.getAll(parseInt(limit), parseInt(offset));

        res.status(200).json({
            success: true,
            data: users,
            pagination: {
                limit: parseInt(limit),
                offset: parseInt(offset)
            }
        });
    } catch (error) {
        console.error('Get users error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get users',
            error: error.message
        });
    }
};

// ============================================
// SYSTEM SETTINGS
// ============================================

// Get all settings
export const getSettings = async (req, res) => {
    try {
        const result = await query('SELECT * FROM system_settings ORDER BY setting_key');

        const settings = {};
        result.rows.forEach(row => {
            settings[row.setting_key] = row.setting_value;
        });

        res.status(200).json({
            success: true,
            data: settings
        });
    } catch (error) {
        console.error('Get settings error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get settings',
            error: error.message
        });
    }
};

// Update setting
export const updateSetting = async (req, res) => {
    try {
        const { setting_key, setting_value } = req.body;

        await query(
            `INSERT INTO system_settings (setting_key, setting_value, updated_at)
       VALUES ($1, $2, CURRENT_TIMESTAMP)
       ON CONFLICT (setting_key) 
       DO UPDATE SET setting_value = $2, updated_at = CURRENT_TIMESTAMP`,
            [setting_key, setting_value]
        );

        res.status(200).json({
            success: true,
            message: 'Setting updated successfully'
        });
    } catch (error) {
        console.error('Update setting error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update setting',
            error: error.message
        });
    }
};

// ============================================
// DELIVERY ZONES
// ============================================

// Get delivery zones
export const getDeliveryZones = async (req, res) => {
    try {
        const result = await query(
            `SELECT setting_value FROM system_settings WHERE setting_key = 'delivery_zones'`
        );

        const zones = result.rows[0]?.setting_value || '[]';
        res.status(200).json({
            success: true,
            data: JSON.parse(zones)
        });
    } catch (error) {
        console.error('Get delivery zones error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get delivery zones',
            error: error.message
        });
    }
};

// Update delivery zones
export const updateDeliveryZones = async (req, res) => {
    try {
        const { zones } = req.body;

        await query(
            `INSERT INTO system_settings (setting_key, setting_value, updated_at)
       VALUES ('delivery_zones', $1, CURRENT_TIMESTAMP)
       ON CONFLICT (setting_key) 
       DO UPDATE SET setting_value = $1, updated_at = CURRENT_TIMESTAMP`,
            [JSON.stringify(zones)]
        );

        // Clear the delivery zone cache so new zones are loaded immediately
        clearDeliveryZoneCache();

        res.status(200).json({
            success: true,
            message: 'Delivery zones updated successfully'
        });
    } catch (error) {
        console.error('Update delivery zones error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update delivery zones',
            error: error.message
        });
    }
};

// ============================================
// SUSPICIOUS ORDERS
// ============================================

// Get suspicious orders
export const getSuspiciousOrders = async (req, res) => {
    try {
        const orders = await Order.getOrders({
            is_suspicious: true,
            limit: parseInt(req.query.limit) || 50,
            offset: parseInt(req.query.offset) || 0
        });

        res.status(200).json({
            success: true,
            data: orders
        });
    } catch (error) {
        console.error('Get suspicious orders error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get suspicious orders',
            error: error.message
        });
    }
};

// ============================================
// ADMIN ACTIVITY LOGS
// ============================================

// Get admin activity logs
export const getAdminLogs = async (req, res) => {
    try {
        const { limit = 50, offset = 0 } = req.query;

        const result = await query(
            `SELECT * FROM admin_activity_logs 
       ORDER BY created_at DESC 
       LIMIT $1 OFFSET $2`,
            [parseInt(limit), parseInt(offset)]
        );

        res.status(200).json({
            success: true,
            data: result.rows,
            pagination: {
                limit: parseInt(limit),
                offset: parseInt(offset)
            }
        });
    } catch (error) {
        console.error('Get admin logs error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get admin logs',
            error: error.message
        });
    }
};

// ============================================
// REVENUE STATS (Helper)
// ============================================
async function getRevenueStats() {
    try {
        const result = await query(
            `SELECT 
        SUM(CASE WHEN DATE(created_at) = CURRENT_DATE THEN total ELSE 0 END) as today_revenue,
        SUM(CASE WHEN DATE(created_at) = CURRENT_DATE - INTERVAL '1 day' THEN total ELSE 0 END) as yesterday_revenue,
        SUM(CASE WHEN DATE_TRUNC('week', created_at) = DATE_TRUNC('week', CURRENT_DATE) THEN total ELSE 0 END) as weekly_revenue,
        SUM(total) as total_revenue
       FROM orders 
       WHERE status != 'cancelled'`
        );
        return result.rows[0];
    } catch (error) {
        console.error('Get revenue stats error:', error);
        return null;
    }
}

// ============================================
// CASE TRANSLATION HELPERS
// ============================================
function pascalToSnake(obj) {
    if (Array.isArray(obj)) {
        return obj.map(pascalToSnake);
    } else if (obj !== null && typeof obj === 'object') {
        const newObj = {};
        for (const key of Object.keys(obj)) {
            let snakeKey = key
                .replace(/([A-Z])/g, "_$1")
                .toLowerCase()
                .replace(/^_/, "");
            
            let finalKey = snakeKey;
            if (key === 'CategoryId') finalKey = 'category_id';
            if (key === 'ProductId') finalKey = 'product_id';
            if (key === 'VariationId') finalKey = 'variation_id';
            if (key === 'DealId') finalKey = 'deal_id';
            if (key === 'OrderId') finalKey = 'order_id';
            if (key === 'UserId') finalKey = 'user_id';
            
            let val = obj[key];
            if (val === '') {
                val = null;
            }
            newObj[finalKey] = pascalToSnake(val);
        }
        return newObj;
    }
    return obj;
}

function snakeToPascal(obj) {
    if (Array.isArray(obj)) {
        return obj.map(snakeToPascal);
    } else if (obj instanceof Date) {
        // Convert JS Date objects to ISO strings so C# can deserialize them as strings
        return obj.toISOString();
    } else if (obj !== null && typeof obj === 'object') {
        const newObj = {};
        for (const key of Object.keys(obj)) {
            let pascalKey = key.split('_').map(word => word.charAt(0).toUpperCase() + word.slice(1)).join('');
            
            if (key === 'category_id') pascalKey = 'CategoryId';
            if (key === 'product_id') pascalKey = 'ProductId';
            if (key === 'variation_id') pascalKey = 'VariationId';
            if (key === 'deal_id') pascalKey = 'DealId';
            if (key === 'order_id') pascalKey = 'OrderId';
            if (key === 'user_id') pascalKey = 'UserId';
            
            newObj[pascalKey] = snakeToPascal(obj[key]);
        }
        return newObj;
    }
    return obj;
}

// ============================================
// ADMIN SYNC CONTROLLERS
// ============================================

// POST /api/admin/categories/sync
export const syncCategory = async (req, res) => {
    try {
        const raw = req.body;
        if (!raw.Name && !raw.name) {
            return res.status(400).json({ success: false, message: 'Category Name is required' });
        }
        const category = pascalToSnake(raw);

        // Remove any conflicting category with the same slug but different ID
        await query('DELETE FROM categories WHERE slug = $1 AND id != $2', [category.slug, category.id]);

        await query(
            `INSERT INTO categories (id, name, slug, display_order, is_active)
             VALUES ($1, $2, $3, $4, $5)
             ON CONFLICT (id)
             DO UPDATE SET name = $2, slug = $3, display_order = $4, is_active = $5`,
            [category.id, category.name, category.slug, category.display_order || 0, category.is_active !== false]
        );

        emitSocketEvent('category_added', {
            id: category.id,
            name: category.name,
            slug: category.slug,
            display_order: category.display_order || 0,
            is_active: category.is_active !== false
        });

        res.status(200).json({ success: true, id: category.id, message: 'Category synced successfully' });
    } catch (error) {
        console.error('syncCategory error:', error);
        res.status(500).json({ success: false, message: 'Failed to sync category', error: error.message });
    }
};

// POST /api/admin/products/sync
export const syncProduct = async (req, res) => {
    const client = await pool.connect();
    try {
        const raw = req.body;
        if (!raw.Name && !raw.name) {
            client.release();
            return res.status(400).json({ success: false, message: 'Product Name is required' });
        }
        const product = pascalToSnake(raw);
        const variations = product.variations || [];
        delete product.variations;
        delete product.category_name;

        const columns = Object.keys(product);
        const values = Object.values(product);
        const placeholders = columns.map((_, idx) => `$${idx + 1}`).join(', ');
        const updateSet = columns
            .filter(col => col !== 'id')
            .map(col => `${col} = EXCLUDED.${col}`)
            .join(', ');

        await client.query('BEGIN');
        
        // Remove any conflicting product with the same slug but different ID
        await client.query('DELETE FROM products WHERE slug = $1 AND id != $2', [product.slug, product.id]);

        await client.query(
            `INSERT INTO products (${columns.join(', ')})
             VALUES (${placeholders})
             ON CONFLICT (id)
             DO UPDATE SET ${updateSet}`,
            values
        );

        // Sync variations
        await client.query('DELETE FROM product_variations WHERE product_id = $1', [product.id]);
        for (const v of variations) {
            const vCol = Object.keys(v);
            const vVal = Object.values(v);
            const vPlaceholders = vCol.map((_, idx) => `$${idx + 1}`).join(', ');
            await client.query(
                `INSERT INTO product_variations (${vCol.join(', ')}) VALUES (${vPlaceholders})`,
                vVal
            );
        }

        await client.query('COMMIT');

        emitSocketEvent('product_added', {
            id: product.id,
            name: product.name,
            category_id: product.category_id,
            price: product.price,
            image_url: product.image_url,
            is_active: product.is_active !== false
        });

        res.status(200).json({ success: true, id: product.id, message: 'Product synced successfully' });
    } catch (error) {
        await client.query('ROLLBACK');
        console.error('syncProduct error:', error);
        res.status(500).json({ success: false, message: 'Failed to sync product', error: error.message });
    } finally {
        client.release();
    }
};

// PATCH /api/admin/products/:id/status
export const updateProductStatus = async (req, res) => {
    try {
        const { id } = req.params;
        const { isActive, is_active } = req.body;
        const val = isActive !== undefined ? isActive : (is_active !== undefined ? is_active : true);

        const result = await query(
            `UPDATE products SET is_active = $1, updated_at = CURRENT_TIMESTAMP WHERE id = $2 RETURNING *`,
            [val !== false, id]
        );

        if (result.rowCount === 0) {
            return res.status(404).json({ success: false, message: 'Product not found' });
        }

        emitSocketEvent('product_added', {
            id: result.rows[0].id,
            name: result.rows[0].name,
            category_id: result.rows[0].category_id,
            price: result.rows[0].price,
            image_url: result.rows[0].image_url,
            is_active: result.rows[0].is_active !== false
        });

        res.status(200).json({ success: true, data: result.rows[0], message: 'Product status updated successfully' });
    } catch (error) {
        console.error('updateProductStatus error:', error);
        res.status(500).json({ success: false, message: 'Failed to update product status', error: error.message });
    }
};

// GET /api/admin/orders/new
export const getNewOrders = async (req, res) => {
    try {
        const { since } = req.query;
        let queryStr = `SELECT * FROM orders WHERE status = 'pending'`;
        const params = [];
        
        if (since) {
            queryStr += ` AND created_at > $1`;
            params.push(since);
        }
        
        queryStr += ` ORDER BY created_at ASC`;
        const ordersQuery = await query(queryStr, params);
        const orders = ordersQuery.rows;
        const resultOrders = [];

        for (const order of orders) {
            const itemsQuery = await query('SELECT * FROM order_items WHERE order_id = $1', [order.id]);
            order.items = itemsQuery.rows;
            resultOrders.push(snakeToPascal(order));
        }

        res.status(200).json(resultOrders);
    } catch (error) {
        console.error('getNewOrders error:', error);
        res.status(500).json({ success: false, message: 'Failed to get new orders', error: error.message });
    }
};

// POST /api/admin/orders/sync
export const syncOrder = async (req, res) => {
    const client = await pool.connect();
    try {
        const raw = req.body;
        const order = pascalToSnake(raw);
        const items = order.items || [];
        delete order.items;

        const columns = Object.keys(order);
        const values = Object.values(order);
        const placeholders = columns.map((_, idx) => `$${idx + 1}`).join(', ');
        const updateSet = columns
            .filter(col => col !== 'id')
            .map(col => `${col} = EXCLUDED.${col}`)
            .join(', ');

        await client.query('BEGIN');
        
        await client.query(
            `INSERT INTO orders (${columns.join(', ')})
             VALUES (${placeholders})
             ON CONFLICT (id)
             DO UPDATE SET ${updateSet}`,
            values
        );

        await client.query('DELETE FROM order_items WHERE order_id = $1', [order.id]);
        for (const oi of items) {
            const oiCol = Object.keys(oi);
            const oiVal = Object.values(oi);
            const oiPlaceholders = oiCol.map((_, idx) => `$${idx + 1}`).join(', ');
            await client.query(
                `INSERT INTO order_items (${oiCol.join(', ')}) VALUES (${oiPlaceholders})`,
                oiVal
            );
        }

        await client.query('COMMIT');
        res.status(200).json({ success: true, message: 'Order synced successfully' });
    } catch (error) {
        await client.query('ROLLBACK');
        console.error('syncOrder error:', error);
        res.status(500).json({ success: false, message: 'Failed to sync order', error: error.message });
    } finally {
        client.release();
    }
};

// DELETE /api/admin/categories/:id
export const deleteCategory = async (req, res) => {
    try {
        const { id } = req.params;
        const result = await query('UPDATE categories SET is_active = false WHERE id = $1 RETURNING id', [id]);
        if (result.rowCount === 0) {
            return res.status(200).json({ success: true, message: 'Category not found (already deleted)' });
        }
        emitSocketEvent('category_deleted', { id: id });
        res.status(200).json({ success: true, message: 'Category soft-deleted successfully' });
    } catch (error) {
        console.error('deleteCategory error:', error);
        res.status(500).json({ success: false, message: 'Failed to delete category', error: error.message });
    }
};

// DELETE /api/admin/products/:id
export const deleteProduct = async (req, res) => {
    try {
        const { id } = req.params;
        const result = await query('UPDATE products SET is_active = false, updated_at = CURRENT_TIMESTAMP WHERE id = $1 RETURNING id', [id]);
        if (result.rowCount === 0) {
            return res.status(200).json({ success: true, message: 'Product not found (already deleted)' });
        }
        emitSocketEvent('product_deleted', { id: id });
        res.status(200).json({ success: true, message: 'Product soft-deleted successfully' });
    } catch (error) {
        console.error('deleteProduct error:', error);
        res.status(500).json({ success: false, message: 'Failed to delete product', error: error.message });
    }
};