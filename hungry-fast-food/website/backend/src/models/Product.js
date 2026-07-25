// E:\hungryHub\hungry-fast-food\website\backend\src\models\Product.js

import { query } from '../config/database.js';

class Product {
    // Create product
    static async create(productData) {
        const {
            category_id, name, slug, description, base_price, discount_price,
            has_variations, is_active, is_deal, image_url, display_order
        } = productData;

        const result = await query(
            `INSERT INTO products (
        category_id, name, slug, description, base_price, discount_price,
        has_variations, is_active, is_deal, image_url, display_order
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)
      RETURNING *`,
            [
                category_id, name, slug, description, base_price, discount_price,
                has_variations || false, is_active !== undefined ? is_active : true,
                is_deal || false, image_url, display_order || 0
            ]
        );

        return result.rows[0];
    }

    // Get product by ID
    static async findById(id) {
        const result = await query(
            `SELECT p.*, c.name as category_name,
        COALESCE(
          json_agg(
            json_build_object(
              'id', pv.id,
              'variation_type', pv.variation_type,
              'variation_name', pv.variation_name,
              'price_adjustment', pv.price_adjustment,
              'is_default', pv.is_default
            ) ORDER BY pv.id
          ) FILTER (WHERE pv.id IS NOT NULL), '[]'
        ) as variations
       FROM products p
       LEFT JOIN categories c ON p.category_id = c.id
       LEFT JOIN product_variations pv ON p.id = pv.product_id
       WHERE p.id = $1
       GROUP BY p.id, c.name`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Get all products with filters
    static async getProducts(filters = {}) {
        const conditions = [];
        const values = [];
        let index = 1;

        if (filters.category_id) {
            conditions.push(`p.category_id = $${index++}`);
            values.push(filters.category_id);
        }
        if (filters.is_active !== undefined) {
            conditions.push(`p.is_active = $${index++}`);
            values.push(filters.is_active);
        }
        if (filters.is_deal !== undefined) {
            conditions.push(`p.is_deal = $${index++}`);
            values.push(filters.is_deal);
        }
        if (filters.search) {
            conditions.push(`(p.name ILIKE $${index++} OR p.description ILIKE $${index++})`);
            values.push(`%${filters.search}%`, `%${filters.search}%`);
        }

        const whereClause = conditions.length > 0
            ? `WHERE ${conditions.join(' AND ')}`
            : '';

        const limit = filters.limit || 50;
        const offset = filters.offset || 0;

        const result = await query(
            `SELECT p.*, c.name as category_name
       FROM products p
       LEFT JOIN categories c ON p.category_id = c.id
       ${whereClause}
       ORDER BY p.display_order ASC, p.name ASC
       LIMIT $${index++} OFFSET $${index++}`,
            [...values, limit, offset]
        );

        return result.rows;
    }

    // Update product
    static async update(id, updates) {
        const fields = [];
        const values = [];
        let index = 1;

        const allowedFields = [
            'category_id', 'name', 'slug', 'description', 'base_price',
            'discount_price', 'has_variations', 'is_active', 'is_deal',
            'image_url', 'display_order'
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
            `UPDATE products 
       SET ${fields.join(', ')}, updated_at = CURRENT_TIMESTAMP
       WHERE id = $${index}
       RETURNING *`,
            values
        );

        return result.rows[0] || null;
    }

    // Delete product (soft delete - set inactive)
    static async delete(id) {
        const result = await query(
            `UPDATE products 
       SET is_active = false, updated_at = CURRENT_TIMESTAMP
       WHERE id = $1
       RETURNING *`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Add variation to product
    static async addVariation(productId, variationData) {
        const { variation_type, variation_name, price_adjustment, is_default } = variationData;

        const result = await query(
            `INSERT INTO product_variations (
        product_id, variation_type, variation_name, price_adjustment, is_default
      ) VALUES ($1, $2, $3, $4, $5)
      RETURNING *`,
            [productId, variation_type, variation_name, price_adjustment || 0, is_default || false]
        );

        return result.rows[0];
    }

    // Remove variation
    static async removeVariation(variationId) {
        const result = await query(
            `DELETE FROM product_variations WHERE id = $1 RETURNING id`,
            [variationId]
        );
        return result.rows[0] || null;
    }

    // Get product by slug
    static async findBySlug(slug) {
        const result = await query(
            'SELECT * FROM products WHERE slug = $1',
            [slug]
        );
        return result.rows[0] || null;
    }

    // Get product by exact name (case-insensitive)
    static async findByName(name, excludeId = null) {
        let queryText = 'SELECT * FROM products WHERE LOWER(name) = LOWER($1)';
        let params = [name];

        if (excludeId) {
            queryText += ' AND id != $2';
            params.push(excludeId);
        }

        const result = await query(queryText, params);
        return result.rows[0] || null;
    }

    // Get variations for a product
    static async getVariations(productId) {
        const result = await query(
            `SELECT * FROM product_variations WHERE product_id = $1 ORDER BY is_default DESC, id`,
            [productId]
        );
        return result.rows;
    }
}

export default Product;