// E:\hungryHub\hungry-fast-food\website\backend\src\services\otpService.js

import crypto from 'crypto';
import OTP from '../models/OTP.js';
import { sendOTPEmail } from './emailService.js';

// Generate 6-digit OTP (Cryptographically secure)
export const generateOTP = () => {
    return crypto.randomInt(100000, 999999).toString();
};

// Create and send OTP with proper error handling
export const createAndSendOTP = async (email, purpose) => {
    try {
        console.log(`📧 [OTP] Creating OTP for ${email} (${purpose})`);

        // Validate email
        if (!email || !email.includes('@')) {
            throw new Error('Invalid email address');
        }

        // Check if OTP already exists and is valid
        const exists = await OTP.exists(email, purpose);
        if (exists) {
            throw new Error('OTP already sent. Please wait for it to expire.');
        }

        // Check rate limit (max 3 attempts per 15 minutes)
        const attempts = await OTP.getAttemptsCount(email, purpose);
        if (attempts >= 3) {
            throw new Error('Too many OTP requests. Please try after 15 minutes.');
        }

        // Generate 6-digit OTP
        const otpCode = generateOTP();
        console.log(`🔑 [OTP] Generated OTP for ${email}: ${otpCode}`);

        // Save OTP to database
        await OTP.create({
            email,
            otp_code: otpCode,
            purpose
        });

        // Send OTP via email
        try {
            await sendOTPEmail(email, otpCode, purpose);
            console.log(`📧 [OTP] Email sent to ${email}`);
        } catch (emailError) {
            console.error(`❌ [OTP] Email send failed: ${emailError.message}`);
            // Don't throw - we still have OTP in database and console
        }

        return otpCode;
    } catch (error) {
        console.error('❌ [OTP] Creation error:', error.message);
        throw error;
    }
};

// Verify OTP with detailed error messages
export const verifyOTP = async (email, otpCode, purpose) => {
    try {
        console.log(`🔍 [OTP] Verifying for ${email}: ${otpCode} (${purpose})`);

        if (!email || !otpCode || !purpose) {
            return {
                success: false,
                message: 'Email, OTP and purpose are required'
            };
        }

        const verification = await OTP.verify(email, otpCode, purpose);

        if (!verification.valid) {
            console.log(`❌ [OTP] Verification failed: ${verification.message}`);
            return { success: false, message: verification.message };
        }

        console.log(`✅ [OTP] Verified successfully for ${email}`);
        return { success: true, otp: verification.otp };
    } catch (error) {
        console.error('❌ [OTP] Verification error:', error.message);
        return { success: false, message: 'OTP verification failed' };
    }
};

// Resend OTP with rate limiting
export const resendOTP = async (email, purpose) => {
    try {
        console.log(`📧 [OTP] Resend request for ${email} (${purpose})`);

        // Check if user exists (for forgot password)
        if (purpose === 'forgot_password') {
            const User = (await import('../models/User.js')).default;
            const user = await User.findByEmail(email);
            if (!user) {
                throw new Error('User not found with this email');
            }
        }

        // Check rate limit
        const attempts = await OTP.getAttemptsCount(email, purpose);
        if (attempts >= 3) {
            throw new Error('Too many OTP requests. Please try after 15 minutes.');
        }

        // Delete old OTPs for this purpose
        await OTP.deleteOld(email, purpose);

        // Generate new OTP
        const otpCode = generateOTP();
        console.log(`🔑 [OTP] New OTP for ${email}: ${otpCode}`);

        await OTP.create({
            email,
            otp_code: otpCode,
            purpose
        });

        await sendOTPEmail(email, otpCode, purpose);
        console.log(`📧 [OTP] Resent to ${email}`);

        return otpCode;
    } catch (error) {
        console.error('❌ [OTP] Resend error:', error.message);
        throw error;
    }
};