// E:\hungryHub\hungry-fast-food\website\backend\src\controllers\syncController.js

import pool from '../config/database.js';
import { sendOrderStatusEmail } from '../services/emailService.js';
import { emitSocketEvent } from '../../services/socketService.js';

// PascalCase to snake_case translator
function pascalToSnake(obj) {
    if (Array.isArray(obj)) {
        return obj.map(pascalToSnake);
    } else if (obj !== null && typeof obj === 'object') {
        const newObj = {};
        for (const key of Object.keys(obj)) {
            let snakeKey = key
                .replace(/([A-Z])/g, "_$1")
                .toLowerCase()
                .replace(/^_/, ""); // strip leading underscore
            
            // Map custom overrides to match Postgres schema columns exactly
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

// snake_case to PascalCase translator for C# models
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
            
            // Map custom overrides to match C# models exactly
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

export const pushSyncItems = async (req, res) => {
    const { items } = req.body;
    if (!items || !Array.isArray(items)) {
        return res.status(400).json({ success: false, message: 'Invalid sync payload' });
    }

    const client = await pool.connect();
    try {
        await client.query('BEGIN');

        for (const item of items) {
            const { OperationType, TableName, RecordId, Payload } = item;
            
            // CLOUDINARY_UPLOAD is handled client-side; skip if it reaches the server
            if (OperationType === 'CLOUDINARY_UPLOAD') continue;
            
            // Map C# SQLite table name to lowercased PostgreSQL table name
            const postgresTable = TableName.toLowerCase();
            
            if (OperationType === 'DELETE') {
                if (postgresTable === 'systemsettings' || postgresTable === 'system_settings') {
                    await client.query(
                        'DELETE FROM system_settings WHERE setting_key = $1',
                        [RecordId]
                    );
                } else if (postgresTable === 'products') {
                    await client.query('DELETE FROM product_variations WHERE product_id = $1', [RecordId]);
                    await client.query('DELETE FROM products WHERE id = $1', [RecordId]);
                } else if (postgresTable === 'deals') {
                    await client.query('DELETE FROM deal_items WHERE deal_id = $1', [RecordId]);
                    await client.query('DELETE FROM deals WHERE id = $1', [RecordId]);
                } else {
                    await client.query(
                        `DELETE FROM ${postgresTable} WHERE id = $1`,
                        [RecordId]
                    );
                }
                continue;
            }

            if (OperationType === 'INSERT' || OperationType === 'UPDATE') {
                const rawRecord = JSON.parse(Payload);
                const record = pascalToSnake(rawRecord);

                if (postgresTable === 'systemsettings' || postgresTable === 'system_settings') {
                    // SystemSettings table uses setting_key UNIQUE constraint
                    await client.query(
                        `INSERT INTO system_settings (setting_key, setting_value, updated_at)
                         VALUES ($1, $2, CURRENT_TIMESTAMP)
                         ON CONFLICT (setting_key)
                         DO UPDATE SET setting_value = $2, updated_at = CURRENT_TIMESTAMP`,
                        [record.setting_key, record.setting_value]
                    );
                    emitSocketEvent('settings_updated', { key: record.setting_key, value: record.setting_value });
                } else if (postgresTable === 'products') {
                    const variations = record.variations || [];
                    delete record.variations; // strip to match columns
                    delete record.category_name;

                    const columns = Object.keys(record);
                    const values = Object.values(record);
                    const placeholders = columns.map((_, idx) => `$${idx + 1}`).join(', ');
                    const updateSet = columns
                        .filter(col => col !== 'id')
                        .map(col => `${col} = EXCLUDED.${col}`)
                        .join(', ');

                    const sql = `
                        INSERT INTO products (${columns.join(', ')})
                        VALUES (${placeholders})
                        ON CONFLICT (id)
                        DO UPDATE SET ${updateSet}
                    `;
                    await client.query(sql, values);

                    // Sync product variations
                    await client.query('DELETE FROM product_variations WHERE product_id = $1', [RecordId]);
                    for (const v of variations) {
                        const vCol = Object.keys(v);
                        const vVal = Object.values(v);
                        const vPlaceholders = vCol.map((_, idx) => `$${idx + 1}`).join(', ');
                        await client.query(
                            `INSERT INTO product_variations (${vCol.join(', ')}) VALUES (${vPlaceholders})`,
                            vVal
                        );
                    }
                } else if (postgresTable === 'deals') {
                    const itemsList = record.items || [];
                    delete record.items; // strip to match columns

                    const columns = Object.keys(record);
                    const values = Object.values(record);
                    const placeholders = columns.map((_, idx) => `$${idx + 1}`).join(', ');
                    const updateSet = columns
                        .filter(col => col !== 'id')
                        .map(col => `${col} = EXCLUDED.${col}`)
                        .join(', ');

                    const sql = `
                        INSERT INTO deals (${columns.join(', ')})
                        VALUES (${placeholders})
                        ON CONFLICT (id)
                        DO UPDATE SET ${updateSet}
                    `;
                    await client.query(sql, values);

                    // Sync deal items
                    await client.query('DELETE FROM deal_items WHERE deal_id = $1', [RecordId]);
                    for (const di of itemsList) {
                        delete di.product_name;
                        delete di.variation_name;
                        const diCol = Object.keys(di);
                        const diVal = Object.values(di);
                        const diPlaceholders = diCol.map((_, idx) => `$${idx + 1}`).join(', ');
                        await client.query(
                            `INSERT INTO deal_items (${diCol.join(', ')}) VALUES (${diPlaceholders})`,
                            diVal
                        );
                    }
                } else if (postgresTable === 'orders') {
                    const orderItemsList = record.items || [];
                    delete record.items; // strip to match columns

                    const columns = Object.keys(record);
                    const values = Object.values(record);
                    const placeholders = columns.map((_, idx) => `$${idx + 1}`).join(', ');
                    const updateSet = columns
                        .filter(col => col !== 'id')
                        .map(col => `${col} = EXCLUDED.${col}`)
                        .join(', ');

                    const sql = `
                        INSERT INTO orders (${columns.join(', ')})
                        VALUES (${placeholders})
                        ON CONFLICT (id)
                        DO UPDATE SET ${updateSet}
                    `;
                    await client.query(sql, values);

                    // Sync order items
                    await client.query('DELETE FROM order_items WHERE order_id = $1', [RecordId]);
                    for (const oi of orderItemsList) {
                        const oiCol = Object.keys(oi);
                        const oiVal = Object.values(oi);
                        const oiPlaceholders = oiCol.map((_, idx) => `$${idx + 1}`).join(', ');
                        await client.query(
                            `INSERT INTO order_items (${oiCol.join(', ')}) VALUES (${oiPlaceholders})`,
                            oiVal
                        );
                    }
                } else {
                    const columns = Object.keys(record);
                    const values = Object.values(record);
                    const placeholders = columns.map((_, idx) => `$${idx + 1}`).join(', ');
                    const updateSet = columns
                        .filter(col => col !== 'id')
                        .map(col => `${col} = EXCLUDED.${col}`)
                        .join(', ');

                    const sql = `
                        INSERT INTO ${postgresTable} (${columns.join(', ')})
                        VALUES (${placeholders})
                        ON CONFLICT (id)
                        DO UPDATE SET ${updateSet}
                    `;
                    await client.query(sql, values);
                }
            }
        }

        await client.query('COMMIT');
        res.status(200).json({ success: true, message: 'Sync push successful' });
    } catch (error) {
        await client.query('ROLLBACK');
        console.error('❌ Sync Push Error:', error);
        res.status(500).json({ success: false, message: 'Sync push failed', error: error.message });
    } finally {
        client.release();
    }
};

export const pullSyncOrders = async (req, res) => {
    try {
        // Fetch all online orders that aren't synced to local yet
        const ordersQuery = await pool.query(
            `SELECT * FROM orders 
             WHERE order_type IN ('delivery', 'takeaway') 
               AND (is_synced = false OR is_synced IS NULL)
             ORDER BY created_at ASC`
        );

        const orders = ordersQuery.rows;
        const resultOrders = [];

        for (const order of orders) {
            // Fetch items for this order
            const itemsQuery = await pool.query(
                'SELECT * FROM order_items WHERE order_id = $1',
                [order.id]
            );
            
            order.items = itemsQuery.rows;
            resultOrders.push(snakeToPascal(order));

            // Mark this order as synced
            await pool.query(
                `UPDATE orders SET is_synced = true, synced_at = CURRENT_TIMESTAMP WHERE id = $1`,
                [order.id]
            );
        }

        res.status(200).json(resultOrders);
    } catch (error) {
        console.error('❌ Sync Pull Error:', error);
        res.status(500).json({ success: false, message: 'Sync pull failed', error: error.message });
    }
};

export const syncCategory = async (req, res) => {
    try {
        const raw = req.body;
        const category = pascalToSnake(raw);
        await pool.query(
            `INSERT INTO categories (id, name, slug, display_order, is_active)
             VALUES ($1, $2, $3, $4, $5)
             ON CONFLICT (id)
             DO UPDATE SET name = $2, slug = $3, display_order = $4, is_active = $5`,
            [category.id, category.name, category.slug, category.display_order || 0, category.is_active !== false]
        );
        res.status(200).json({ success: true, message: 'Category synced successfully' });
    } catch (error) {
        console.error('syncCategory error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const updateCategorySync = async (req, res) => {
    try {
        const { id } = req.params;
        const raw = req.body;
        const category = pascalToSnake(raw);
        await pool.query(
            `UPDATE categories SET name = $1, slug = $2, display_order = $3, is_active = $4 WHERE id = $5`,
            [category.name, category.slug, category.display_order || 0, category.is_active !== false, id]
        );
        res.status(200).json({ success: true, message: 'Category updated successfully' });
    } catch (error) {
        console.error('updateCategorySync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const deleteCategorySync = async (req, res) => {
    try {
        const { id } = req.params;
        await pool.query('UPDATE categories SET is_active = false WHERE id = $1', [id]);
        res.status(200).json({ success: true, message: 'Category soft-deleted successfully' });
    } catch (error) {
        console.error('deleteCategorySync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const syncProduct = async (req, res) => {
    const client = await pool.connect();
    try {
        await client.query('BEGIN');
        const raw = req.body;
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
        res.status(200).json({ success: true, message: 'Product synced successfully' });
    } catch (error) {
        await client.query('ROLLBACK');
        console.error('syncProduct error:', error);
        res.status(500).json({ success: false, message: error.message });
    } finally {
        client.release();
    }
};

export const updateProductSync = async (req, res) => {
    return syncProduct(req, res);
};

export const deleteProductSync = async (req, res) => {
    try {
        const { id } = req.params;
        await pool.query('UPDATE products SET is_active = false WHERE id = $1', [id]);
        res.status(200).json({ success: true, message: 'Product soft-deleted successfully' });
    } catch (error) {
        console.error('deleteProductSync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const updateProductAvailabilitySync = async (req, res) => {
    try {
        const { id } = req.params;
        const { is_active } = req.body;
        await pool.query('UPDATE products SET is_active = $1 WHERE id = $2', [is_active !== false, id]);
        res.status(200).json({ success: true, message: 'Product availability updated successfully' });
    } catch (error) {
        console.error('updateProductAvailabilitySync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const getNewOrdersSync = async (req, res) => {
    try {
        const { since } = req.query;
        let queryStr = `SELECT * FROM orders WHERE order_type IN ('delivery', 'takeaway')`;
        const params = [];
        
        if (since) {
            queryStr += ` AND updated_at > $1`;
            params.push(since);
        } else {
            queryStr += ` AND (is_synced = false OR is_synced IS NULL)`;
        }
        
        queryStr += ` ORDER BY created_at ASC`;
        const ordersQuery = await pool.query(queryStr, params);
        const orders = ordersQuery.rows;
        const resultOrders = [];

        for (const order of orders) {
            const itemsQuery = await pool.query('SELECT * FROM order_items WHERE order_id = $1', [order.id]);
            order.items = itemsQuery.rows;
            resultOrders.push(snakeToPascal(order));
            
            if (!since) {
                await pool.query('UPDATE orders SET is_synced = true, synced_at = CURRENT_TIMESTAMP WHERE id = $1', [order.id]);
            }
        }
        res.status(200).json(resultOrders);
    } catch (error) {
        console.error('getNewOrdersSync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const updateOrderStatusSync = async (req, res) => {
    try {
        const { id } = req.params;
        const { status } = req.body;

        // 1. Update status
        await pool.query('UPDATE orders SET status = $1, updated_at = CURRENT_TIMESTAMP WHERE id = $2', [status, id]);

        // 2. Fetch order details to trigger email and socket updates
        const orderResult = await pool.query('SELECT order_number, customer_name, customer_email, updated_at FROM orders WHERE id = $1', [id]);
        if (orderResult.rows.length > 0) {
            const order = orderResult.rows[0];

            // 3. Send email update if email exists
            if (order.customer_email) {
                try {
                    await sendOrderStatusEmail(
                        order.customer_email,
                        order.order_number,
                        status,
                        order.customer_name
                    );
                } catch (emailErr) {
                    console.error('Failed to send status update email during sync:', emailErr);
                }
            }

            // 4. Emit socket event for real-time tracking page updates
            emitSocketEvent('order_status_updated', {
                id: id,
                status,
                order_number: order.order_number,
                updated_at: order.updated_at || new Date().toISOString()
            });
        }

        res.status(200).json({ success: true, message: 'Order status updated successfully' });
    } catch (error) {
        console.error('updateOrderStatusSync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};


export const syncOrder = async (req, res) => {
    const client = await pool.connect();
    try {
        await client.query('BEGIN');
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
        res.status(500).json({ success: false, message: error.message });
    } finally {
        client.release();
    }
};

import { uploadImage } from '../services/cloudinaryService.js';
export const uploadImageSync = async (req, res) => {
    try {
        const { image, fileName } = req.body;
        if (!image) {
            return res.status(400).json({ success: false, message: 'Image base64 data required' });
        }
        const buffer = Buffer.from(image, 'base64');
        const secureUrl = await uploadImage(buffer, fileName || 'upload.jpg');
        res.status(200).json({ success: true, url: secureUrl });
    } catch (error) {
        console.error('uploadImageSync error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
};

export const fullSync = async (req, res) => {
    const client = await pool.connect();
    try {
        const { categories, products, deals, settings } = req.body;
        
        await client.query('BEGIN');

        // 1. Upsert Categories
        if (categories && Array.isArray(categories)) {
            const currentCatRes = await client.query('SELECT id FROM categories');
            const currentCatIds = currentCatRes.rows.map(r => r.id);
            const incomingCatIds = new Set();
            
            for (const cat of categories) {
                incomingCatIds.add(cat.id);
                await client.query(
                    `INSERT INTO categories (id, name, slug, display_order, is_active)
                     VALUES ($1, $2, $3, $4, $5)
                     ON CONFLICT (id) DO UPDATE 
                     SET name = $2, slug = $3, display_order = $4, is_active = $5`,
                    [cat.id, cat.name, cat.slug, cat.display_order, cat.is_active]
                );
            }
            // Mark missing as inactive
            const missingCatIds = currentCatIds.filter(id => !incomingCatIds.has(id));
            if (missingCatIds.length > 0) {
                await client.query('UPDATE categories SET is_active = false WHERE id = ANY($1)', [missingCatIds]);
            }
        }

        // 2. Upsert Products
        if (products && Array.isArray(products)) {
            const currentProdRes = await client.query('SELECT id FROM products');
            const currentProdIds = currentProdRes.rows.map(r => r.id);
            const incomingProdIds = new Set();
            
            for (const prod of products) {
                incomingProdIds.add(prod.id);
                await client.query(
                    `INSERT INTO products (id, category_id, name, slug, description, base_price, discount_price, has_variations, is_active, is_deal, image_url, display_order)
                     VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12)
                     ON CONFLICT (id) DO UPDATE 
                     SET category_id = $2, name = $3, slug = $4, description = $5, base_price = $6, discount_price = $7, has_variations = $8, is_active = $9, is_deal = $10, image_url = $11, display_order = $12`,
                    [prod.id, prod.category_id, prod.name, prod.slug, prod.description, prod.base_price, prod.discount_price, prod.has_variations, prod.is_active, prod.is_deal, prod.image_url, prod.display_order]
                );
                
                // Variations
                await client.query('DELETE FROM product_variations WHERE product_id = $1', [prod.id]);
                if (prod.variations && prod.variations.length > 0) {
                    for (const v of prod.variations) {
                        await client.query(
                            `INSERT INTO product_variations (id, product_id, variation_type, variation_name, price_adjustment, is_default)
                             VALUES ($1, $2, $3, $4, $5, $6)`,
                            [v.id, v.product_id, v.variation_type, v.variation_name, v.price_adjustment, v.is_default]
                        );
                    }
                }
            }
            // Mark missing as inactive
            const missingProdIds = currentProdIds.filter(id => !incomingProdIds.has(id));
            if (missingProdIds.length > 0) {
                await client.query('UPDATE products SET is_active = false WHERE id = ANY($1)', [missingProdIds]);
            }
        }

        // 3. Upsert Deals
        if (deals && Array.isArray(deals)) {
            const currentDealRes = await client.query('SELECT id FROM deals');
            const currentDealIds = currentDealRes.rows.map(r => r.id);
            const incomingDealIds = new Set();
            
            for (const deal of deals) {
                incomingDealIds.add(deal.id);
                await client.query(
                    `INSERT INTO deals (id, name, slug, description, total_price, image_url, is_active)
                     VALUES ($1, $2, $3, $4, $5, $6, $7)
                     ON CONFLICT (id) DO UPDATE 
                     SET name = $2, slug = $3, description = $4, total_price = $5, image_url = $6, is_active = $7`,
                    [deal.id, deal.name, deal.slug, deal.description, deal.total_price, deal.image_url, deal.is_active]
                );
                
                // Items
                await client.query('DELETE FROM deal_items WHERE deal_id = $1', [deal.id]);
                if (deal.items && deal.items.length > 0) {
                    for (const item of deal.items) {
                        await client.query(
                            `INSERT INTO deal_items (id, deal_id, product_id, variation_id, quantity)
                             VALUES ($1, $2, $3, $4, $5)`,
                            [item.id, item.deal_id, item.product_id, item.variation_id, item.quantity]
                        );
                    }
                }
            }
            // Mark missing as inactive
            const missingDealIds = currentDealIds.filter(id => !incomingDealIds.has(id));
            if (missingDealIds.length > 0) {
                await client.query('UPDATE deals SET is_active = false WHERE id = ANY($1)', [missingDealIds]);
            }
        }

        // 4. Upsert Settings
        if (settings && Array.isArray(settings)) {
            let settingsUpdated = false;
            for (const setting of settings) {
                const settingValStr = setting.setting_value != null ? setting.setting_value.toString() : "";
                await client.query(
                    `INSERT INTO system_settings (setting_key, setting_value, updated_at)
                     VALUES ($1, $2, CURRENT_TIMESTAMP)
                     ON CONFLICT (setting_key)
                     DO UPDATE SET setting_value = $2, updated_at = CURRENT_TIMESTAMP`,
                    [setting.setting_key, settingValStr]
                );
                settingsUpdated = true;
                emitSocketEvent('settings_updated', { key: setting.setting_key, value: settingValStr });
            }
        }
        
        await client.query('COMMIT');
        res.status(200).json({ success: true, message: 'Full sync completed successfully.' });
    } catch (error) {
        await client.query('ROLLBACK');
        console.error('Full sync error:', error);
        res.status(500).json({ success: false, message: 'Full sync failed', error: error.message });
    } finally {
        client.release();
    }
};

export const syncStatus = async (req, res) => {
    try {
        const result = await query(
            "SELECT setting_value FROM system_settings WHERE setting_key = 'last_menu_update'"
        );
        if (result.rows.length > 0) {
            res.status(200).json({ success: true, last_menu_update: result.rows[0].setting_value });
        } else {
            res.status(200).json({ success: true, last_menu_update: "" });
        }
    } catch (error) {
        console.error('syncStatus error:', error);
        res.status(500).json({ success: false, message: 'Failed to fetch sync status', error: error.message });
    }
};
