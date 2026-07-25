// E:\hungryHub\hungry-fast-food\website\backend\src\models\Category.js

import { query } from '../config/database.js';

class Category {
    // Create category
    static async create(categoryData) {
        const { name, slug, display_order, is_active } = categoryData;

        const result = await query(
            `INSERT INTO categories (name, slug, display_order, is_active)
       VALUES ($1, $2, $3, $4)
       RETURNING *`,
            [name, slug, display_order || 0, is_active !== undefined ? is_active : true]
        );

        return result.rows[0];
    }

    // Get all categories
    static async getAll(includeInactive = false) {
        const queryText = includeInactive
            ? 'SELECT * FROM categories ORDER BY created_at ASC'
            : 'SELECT * FROM categories WHERE is_active = true ORDER BY created_at ASC';

        const result = await query(queryText);
        return result.rows;
    }

    // Get category by ID
    static async findById(id) {
        const result = await query(
            `SELECT c.*,
        COALESCE(
          json_agg(
            json_build_object(
              'id', p.id,
              'name', p.name,
              'slug', p.slug,
              'base_price', p.base_price,
              'discount_price', p.discount_price,
              'image_url', p.image_url,
              'is_active', p.is_active
            ) ORDER BY p.display_order ASC
          ) FILTER (WHERE p.id IS NOT NULL), '[]'
        ) as products
       FROM categories c
       LEFT JOIN products p ON c.id = p.category_id AND p.is_active = true
       WHERE c.id = $1
       GROUP BY c.id`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Get category by slug
    static async findBySlug(slug) {
        const result = await query(
            'SELECT * FROM categories WHERE slug = $1',
            [slug]
        );
        return result.rows[0] || null;
    }

    // Get category by exact name (case-insensitive)
    static async findByName(name, excludeId = null) {
        let queryText = 'SELECT * FROM categories WHERE LOWER(name) = LOWER($1)';
        let params = [name];

        if (excludeId) {
            queryText += ' AND id != $2';
            params.push(excludeId);
        }

        const result = await query(queryText, params);
        return result.rows[0] || null;
    }

    // Count all products in a category
    static async countProducts(categoryId) {
        const result = await query(
            'SELECT COUNT(*) as count FROM products WHERE category_id = $1',
            [categoryId]
        );
        return parseInt(result.rows[0].count, 10);
    }

    // Count active products in a category
    static async countActiveProducts(categoryId) {
        const result = await query(
            'SELECT COUNT(*) as count FROM products WHERE category_id = $1 AND is_active = true',
            [categoryId]
        );
        return parseInt(result.rows[0].count, 10);
    }

    // Update category
    static async update(id, updates) {
        const fields = [];
        const values = [];
        let index = 1;

        if (updates.name !== undefined) {
            fields.push(`name = $${index++}`);
            values.push(updates.name);
        }
        if (updates.slug !== undefined) {
            fields.push(`slug = $${index++}`);
            values.push(updates.slug);
        }
        if (updates.display_order !== undefined) {
            fields.push(`display_order = $${index++}`);
            values.push(updates.display_order);
        }
        if (updates.is_active !== undefined) {
            fields.push(`is_active = $${index++}`);
            values.push(updates.is_active);
        }

        if (fields.length === 0) return null;

        values.push(id);
        const result = await query(
            `UPDATE categories 
       SET ${fields.join(', ')}, updated_at = CURRENT_TIMESTAMP
       WHERE id = $${index}
       RETURNING *`,
            values
        );

        return result.rows[0] || null;
    }

    // Delete category (soft delete)
    static async delete(id) {
        const result = await query(
            `UPDATE categories 
       SET is_active = false, updated_at = CURRENT_TIMESTAMP
       WHERE id = $1
       RETURNING *`,
            [id]
        );
        return result.rows[0] || null;
    }
}

export default Category;