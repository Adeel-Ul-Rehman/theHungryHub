// E:\hungryHub\hungry-fast-food\website\backend\src\models\User.js

import { query } from '../config/database.js';

class User {
    // Create user
    static async create(userData) {
        const { email, password_hash, full_name, phone, google_id, is_guest } = userData;

        const result = await query(
            `INSERT INTO users (email, password_hash, full_name, phone, google_id, is_guest, is_verified)
       VALUES ($1, $2, $3, $4, $5, $6, $7)
       RETURNING id, email, full_name, phone, is_guest, is_verified, created_at`,
            [email, password_hash, full_name, phone, google_id, is_guest || false, false]
        );

        return result.rows[0];
    }

    // Find user by email
    static async findByEmail(email) {
        const result = await query(
            'SELECT * FROM users WHERE email = $1',
            [email]
        );
        return result.rows[0] || null;
    }

    // Find user by ID
    static async findById(id) {
        const result = await query(
            'SELECT id, email, full_name, phone, is_guest, is_verified, google_id, created_at FROM users WHERE id = $1',
            [id]
        );
        return result.rows[0] || null;
    }

    // Find user by Google ID
    static async findByGoogleId(googleId) {
        const result = await query(
            'SELECT * FROM users WHERE google_id = $1',
            [googleId]
        );
        return result.rows[0] || null;
    }

    // Update user
    static async update(id, updates) {
        const fields = [];
        const values = [];
        let index = 1;

        if (updates.full_name) {
            fields.push(`full_name = $${index++}`);
            values.push(updates.full_name);
        }
        if (updates.phone) {
            fields.push(`phone = $${index++}`);
            values.push(updates.phone);
        }
        if (updates.is_verified !== undefined) {
            fields.push(`is_verified = $${index++}`);
            values.push(updates.is_verified);
        }
        if (updates.password_hash) {
            fields.push(`password_hash = $${index++}`);
            values.push(updates.password_hash);
        }

        if (fields.length === 0) return null;

        values.push(id);
        const result = await query(
            `UPDATE users SET ${fields.join(', ')}, updated_at = CURRENT_TIMESTAMP
       WHERE id = $${index}
       RETURNING id, email, full_name, phone, is_guest, is_verified, created_at`,
            values
        );

        return result.rows[0] || null;
    }

    // Verify user (email verification)
    static async verify(id) {
        const result = await query(
            `UPDATE users SET is_verified = true, updated_at = CURRENT_TIMESTAMP
       WHERE id = $1
       RETURNING id, email, full_name, is_verified`,
            [id]
        );
        return result.rows[0] || null;
    }

    // Get all users (admin)
    static async getAll(limit = 50, offset = 0) {
        const result = await query(
            `SELECT id, email, full_name, phone, is_guest, is_verified, created_at
       FROM users
       ORDER BY created_at DESC
       LIMIT $1 OFFSET $2`,
            [limit, offset]
        );
        return result.rows;
    }

    // Get user count
    static async count() {
        const result = await query('SELECT COUNT(*) FROM users');
        return parseInt(result.rows[0].count);
    }
}

export default User;