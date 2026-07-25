// E:\hungryHub\hungry-fast-food\website\backend\src\models\Deal.js

import pool, { query } from '../config/database.js';

class Deal {
    // Create deal
    static async create(dealData, items) {
        const client = await pool.connect();

        try {
            await client.query('BEGIN');

            // Insert deal
            const dealResult = await client.query(
                `INSERT INTO deals (
          name, slug, description, total_price, discount_price,
          is_active, is_featured, image_url
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
        RETURNING *`,
                [
                    dealData.name,
                    dealData.slug,
                    dealData.description || null,
                    dealData.total_price,
                    dealData.discount_price || null,
                    dealData.is_active !== undefined ? dealData.is_active : true,
                    dealData.is_featured || false,
                    dealData.image_url || null
                ]
            );

            const deal = dealResult.rows[0];

            // Insert deal items
            for (const item of items) {
                await client.query(
                    `INSERT INTO deal_items (
            deal_id, product_id, variation_id, quantity, unit_price
          ) VALUES ($1, $2, $3, $4, $5)`,
                    [
                        deal.id,
                        item.product_id,
                        item.variation_id || null,
                        item.quantity || 1,
                        item.unit_price || 0
                    ]
                );
            }

            await client.query('COMMIT');
            return deal;
        } catch (error) {
            await client.query('ROLLBACK');
            throw error;
        } finally {
            client.release();
        }
    }

    // Get deal by ID with items
    static async findById(id) {
        const result = await query(
            `SELECT d.*,
        COALESCE(
          json_agg(
            json_build_object(
              'id', di.id,
              'product_id', di.product_id,
              'product_name', p.name,
              'variation_id', di.variation_id,
              'variation_name', pv.variation_name,
              'quantity', di.quantity,
              'unit_price', di.unit_price
            ) ORDER BY di.id
          ) FILTER (WHERE di.id IS NOT NULL), '[]'
        ) as items
       FROM deals d
       LEFT JOIN deal_items di ON d.id = di.deal_id
       LEFT JOIN products p ON di.product_id = p.id
       LEFT JOIN product_variations pv ON di.variation_id = pv.id
       WHERE d.id = $1
       GROUP BY d.id`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Get featured deal
    static async getFeatured() {
        const result = await query(
            `SELECT d.*,
        COALESCE(
          json_agg(
            json_build_object(
              'id', di.id,
              'product_id', di.product_id,
              'product_name', p.name,
              'variation_id', di.variation_id,
              'variation_name', pv.variation_name,
              'quantity', di.quantity,
              'unit_price', di.unit_price
            ) ORDER BY di.id
          ) FILTER (WHERE di.id IS NOT NULL), '[]'
        ) as items
       FROM deals d
       LEFT JOIN deal_items di ON d.id = di.deal_id
       LEFT JOIN products p ON di.product_id = p.id
       LEFT JOIN product_variations pv ON di.variation_id = pv.id
       WHERE d.is_featured = true AND d.is_active = true
       GROUP BY d.id
       LIMIT 1`,
            []
        );
        return result.rows[0] || null;
    }

    // Get all deals
    static async getDeals(filters = {}) {
        const conditions = [];
        const values = [];
        let index = 1;

        if (filters.is_active !== undefined) {
            conditions.push(`d.is_active = $${index++}`);
            values.push(filters.is_active);
        }
        if (filters.is_featured !== undefined) {
            conditions.push(`d.is_featured = $${index++}`);
            values.push(filters.is_featured);
        }

        const whereClause = conditions.length > 0
            ? `WHERE ${conditions.join(' AND ')}`
            : '';

        const result = await query(
            `SELECT d.*,
        COALESCE(
          json_agg(
            json_build_object(
              'id', di.id,
              'product_id', di.product_id,
              'product_name', p.name,
              'variation_id', di.variation_id,
              'variation_name', pv.variation_name,
              'quantity', di.quantity,
              'unit_price', di.unit_price
            ) ORDER BY di.id
          ) FILTER (WHERE di.id IS NOT NULL), '[]'
        ) as items
       FROM deals d
       LEFT JOIN deal_items di ON d.id = di.deal_id
       LEFT JOIN products p ON di.product_id = p.id
       LEFT JOIN product_variations pv ON di.variation_id = pv.id
       ${whereClause}
       GROUP BY d.id
       ORDER BY d.created_at DESC`,
            values
        );

        return result.rows;
    }

    // Update deal
    static async update(id, updates) {
        const fields = [];
        const values = [];
        let index = 1;

        const allowedFields = [
            'name', 'slug', 'description', 'total_price',
            'discount_price', 'is_active', 'is_featured', 'image_url'
        ];

        for (const field of allowedFields) {
            if (updates[field] !== undefined) {
                fields.push(`${field} = $${index++}`);
                values.push(updates[field]);
            }
        }

        if (fields.length === 0) return null;

        values.push(id);
        const result = await query(
            `UPDATE deals 
       SET ${fields.join(', ')}, updated_at = CURRENT_TIMESTAMP
       WHERE id = $${index}
       RETURNING *`,
            values
        );

        return result.rows[0] || null;
    }

    // Delete deal
    static async delete(id) {
        const result = await query(
            `UPDATE deals 
       SET is_active = false, updated_at = CURRENT_TIMESTAMP
       WHERE id = $1
       RETURNING *`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Update deal items (replace all items)
    static async updateItems(dealId, items) {
        const client = await pool.connect();

        try {
            await client.query('BEGIN');

            // Delete existing items
            await client.query('DELETE FROM deal_items WHERE deal_id = $1', [dealId]);

            // Insert new items
            for (const item of items) {
                await client.query(
                    `INSERT INTO deal_items (
            deal_id, product_id, variation_id, quantity, unit_price
          ) VALUES ($1, $2, $3, $4, $5)`,
                    [dealId, item.product_id, item.variation_id || null, item.quantity || 1, item.unit_price || 0]
                );
            }

            await client.query('COMMIT');
            return await this.findById(dealId);
        } catch (error) {
            await client.query('ROLLBACK');
            throw error;
        } finally {
            client.release();
        }
    }
}

export default Deal;