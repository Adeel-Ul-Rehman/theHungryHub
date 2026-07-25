// E:\hungryHub\hungry-fast-food\website\backend\src\models\OTP.js

import { query } from '../config/database.js';

class OTP {
    // Create OTP
    static async create(otpData) {
        const { email, otp_code, purpose } = otpData;
        const expiresAt = new Date();
        expiresAt.setMinutes(expiresAt.getMinutes() + parseInt(process.env.OTP_EXPIRY_MINUTES || 10));

        const result = await query(
            `INSERT INTO otp_logs (email, otp_code, purpose, expires_at)
       VALUES ($1, $2, $3, $4)
       RETURNING *`,
            [email, otp_code, purpose, expiresAt]
        );

        console.log(`✅ [OTP] Saved to DB: ${email}, ${otp_code}, expires: ${expiresAt}`);
        return result.rows[0];
    }

    // Verify OTP with detailed checks
    static async verify(email, otp_code, purpose) {
        console.log(`🔍 [OTP] Verifying: ${email}, ${otp_code}, ${purpose}`);

        // Find valid OTP
        const result = await query(
            `SELECT * FROM otp_logs 
       WHERE email = $1 
         AND otp_code = $2 
         AND purpose = $3 
         AND is_used = false 
         AND expires_at > CURRENT_TIMESTAMP
       ORDER BY created_at DESC
       LIMIT 1`,
            [email, otp_code, purpose]
        );

        console.log(`📊 [OTP] Query result rows: ${result.rows.length}`);

        const otp = result.rows[0];

        if (!otp) {
            // Check if OTP exists but expired
            const expiredResult = await query(
                `SELECT * FROM otp_logs 
         WHERE email = $1 
           AND otp_code = $2 
           AND purpose = $3 
           AND is_used = false 
           AND expires_at <= CURRENT_TIMESTAMP
         ORDER BY created_at DESC
         LIMIT 1`,
                [email, otp_code, purpose]
            );

            if (expiredResult.rows.length > 0) {
                return { valid: false, message: 'OTP has expired. Please request a new one.' };
            }

            // Check if OTP already used
            const usedResult = await query(
                `SELECT * FROM otp_logs 
         WHERE email = $1 
           AND otp_code = $2 
           AND purpose = $3 
           AND is_used = true
         ORDER BY created_at DESC
         LIMIT 1`,
                [email, otp_code, purpose]
            );

            if (usedResult.rows.length > 0) {
                return { valid: false, message: 'OTP has already been used. Please request a new one.' };
            }

            // Check if any OTP exists for this email (for debugging)
            const anyResult = await query(
                `SELECT * FROM otp_logs 
         WHERE email = $1 
           AND purpose = $2
         ORDER BY created_at DESC
         LIMIT 1`,
                [email, purpose]
            );

            if (anyResult.rows.length > 0) {
                const lastOtp = anyResult.rows[0];
                console.log(`🔍 [OTP] Last OTP for ${email}: ${lastOtp.otp_code}, used: ${lastOtp.is_used}, expires: ${lastOtp.expires_at}`);
                return {
                    valid: false,
                    message: `Invalid OTP. Last OTP was ${lastOtp.otp_code} (expired: ${new Date(lastOtp.expires_at) < new Date()})`
                };
            }

            return { valid: false, message: 'No OTP found. Please request a new one.' };
        }

        // Mark as used
        await query(
            `UPDATE otp_logs SET is_used = true WHERE id = $1`,
            [otp.id]
        );

        return { valid: true, otp };
    }

    // Check if OTP exists and is valid
    static async exists(email, purpose) {
        const result = await query(
            `SELECT COUNT(*) FROM otp_logs 
       WHERE email = $1 
         AND purpose = $2 
         AND is_used = false 
         AND expires_at > CURRENT_TIMESTAMP`,
            [email, purpose]
        );
        return parseInt(result.rows[0].count) > 0;
    }

    // Get recent OTP attempts count
    static async getAttemptsCount(email, purpose, windowMinutes = 15) {
        const cutoff = new Date();
        cutoff.setMinutes(cutoff.getMinutes() - windowMinutes);

        const result = await query(
            `SELECT COUNT(*) FROM otp_logs 
       WHERE email = $1 
         AND purpose = $2 
         AND created_at > $3`,
            [email, purpose, cutoff]
        );
        return parseInt(result.rows[0].count);
    }

    // Delete old OTPs for a purpose (cleanup)
    static async deleteOld(email, purpose) {
        const result = await query(
            `DELETE FROM otp_logs 
       WHERE email = $1 
         AND purpose = $2 
         AND (is_used = true OR expires_at < CURRENT_TIMESTAMP)`,
            [email, purpose]
        );
        console.log(`🧹 [OTP] Deleted ${result.rowCount} old OTPs for ${email}`);
        return result.rowCount;
    }

    // Clean expired OTPs (scheduled job)
    static async cleanExpired() {
        const result = await query(
            `DELETE FROM otp_logs WHERE expires_at < CURRENT_TIMESTAMP`
        );
        if (result.rowCount > 0) {
            console.log(`🧹 [OTP] Cleaned ${result.rowCount} expired OTPs`);
        }
        return result.rowCount || 0;
    }
}

export default OTP;