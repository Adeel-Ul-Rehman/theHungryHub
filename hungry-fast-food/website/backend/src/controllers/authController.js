// E:\hungryHub\hungry-fast-food\website\backend\src\controllers\authController.js

import bcrypt from 'bcrypt';
import { OAuth2Client } from 'google-auth-library';
import dotenv from 'dotenv';
import User from '../models/User.js';
import OTP from '../models/OTP.js';
import { generateTokens, verifyRefreshToken } from '../middleware/auth.js';
import { sendOTPEmail, sendWelcomeEmail, sendPasswordResetEmail } from '../services/emailService.js';
import { createAndSendOTP, verifyOTP as verifyOTPService } from '../services/otpService.js';
import { hashPassword, comparePassword } from '../utils/validators.js';
import jwt from 'jsonwebtoken';

dotenv.config();

const googleClient = new OAuth2Client(process.env.GOOGLE_CLIENT_ID);

// ============================================
// REGISTER - With Mandatory Email Verification
// ============================================
export const register = async (req, res) => {
    try {
        const { email, password, full_name, phone } = req.body;

        console.log(`📝 [REGISTER] Registering user: ${email}`);

        // Check if user exists
        const existingUser = await User.findByEmail(email);
        if (existingUser) {
            return res.status(409).json({
                success: false,
                message: 'User already exists with this email. Please login.'
            });
        }

        // Hash password
        const hashedPassword = await hashPassword(password);

        // Create user (unverified)
        const user = await User.create({
            email,
            password_hash: hashedPassword,
            full_name,
            phone,
            is_guest: false,
            is_verified: false  // ← IMPORTANT: User is unverified
        });

        console.log(`✅ [REGISTER] User created: ${user.id}`);

        // Generate and send OTP
        try {
            const otpCode = await createAndSendOTP(email, 'signup');
            console.log(`🔑 [REGISTER] OTP for ${email}: ${otpCode}`);
        } catch (otpError) {
            console.error('❌ [REGISTER] OTP error:', otpError.message);
            // User created but OTP failed - still return success but with warning
            return res.status(201).json({
                success: true,
                message: 'User registered. But OTP sending failed. Please use resend-otp endpoint.',
                data: {
                    user: {
                        id: user.id,
                        email: user.email,
                        full_name: user.full_name,
                        phone: user.phone,
                        is_verified: false
                    },
                    requiresVerification: true
                }
            });
        }

        // Generate temporary tokens (limited access until verified)
        const tokens = generateTokens(user);

        res.status(201).json({
            success: true,
            message: 'User registered successfully. Please verify your email with OTP.',
            data: {
                user: {
                    id: user.id,
                    email: user.email,
                    full_name: user.full_name,
                    phone: user.phone,
                    is_verified: false
                },
                tokens,
                requiresVerification: true
            }
        });
    } catch (error) {
        console.error('❌ [REGISTER] Error:', error);
        res.status(500).json({
            success: false,
            message: 'Registration failed',
            error: error.message
        });
    }
};

// ============================================
// VERIFY OTP - Mandatory for account activation
// ============================================
export const verifyOTP = async (req, res) => {
    try {
        const { email, otp, purpose } = req.body;

        console.log(`🔍 [VERIFY] Verifying OTP for ${email} (${purpose})`);

        if (!email || !otp || !purpose) {
            return res.status(400).json({
                success: false,
                message: 'Email, OTP and purpose are required'
            });
        }

        // Verify OTP
        const verification = await verifyOTPService(email, otp, purpose);

        if (!verification.success) {
            return res.status(400).json({
                success: false,
                message: verification.message
            });
        }

        // Update user verification status
        if (purpose === 'signup' || purpose === 'login') {
            const user = await User.findByEmail(email);
            if (user && !user.is_verified) {
                await User.verify(user.id);
                console.log(`✅ [VERIFY] User ${email} verified successfully`);

                // Send welcome email
                await sendWelcomeEmail(email, user.full_name || 'User');
            }
        }

        const responseData = { isVerified: true };
        if (purpose === 'forgot_password') {
            const resetToken = jwt.sign(
                { email, purpose: 'reset_password' },
                process.env.JWT_SECRET,
                { expiresIn: '15m' }
            );
            responseData.resetToken = resetToken;
        }

        res.status(200).json({
            success: true,
            message: 'OTP verified successfully.',
            data: responseData
        });
    } catch (error) {
        console.error('❌ [VERIFY] Error:', error);
        res.status(500).json({
            success: false,
            message: 'OTP verification failed',
            error: error.message
        });
    }
};

// ============================================
// RESEND OTP
// ============================================
export const resendOTP = async (req, res) => {
    try {
        const { email, purpose } = req.body;

        console.log(`📧 [RESEND] Resend OTP for ${email} (${purpose})`);

        if (!email || !purpose) {
            return res.status(400).json({
                success: false,
                message: 'Email and purpose are required'
            });
        }

        // For signup, check if user exists and is not verified
        if (purpose === 'signup') {
            const user = await User.findByEmail(email);
            if (!user) {
                return res.status(404).json({
                    success: false,
                    message: 'User not found. Please register first.'
                });
            }
            if (user.is_verified) {
                return res.status(400).json({
                    success: false,
                    message: 'User is already verified. Please login.'
                });
            }
        }

        // For forgot password, check if user exists
        if (purpose === 'forgot_password') {
            const user = await User.findByEmail(email);
            if (!user) {
                return res.status(404).json({
                    success: false,
                    message: 'User not found with this email'
                });
            }
        }

        // Delete old OTPs
        await OTP.deleteOld(email, purpose);

        // Generate and send new OTP
        const otpCode = await createAndSendOTP(email, purpose);
        console.log(`🔑 [RESEND] New OTP for ${email}: ${otpCode}`);

        res.status(200).json({
            success: true,
            message: 'OTP sent successfully. Please check your email.',
            data: {
                email,
                purpose,
                expiryMinutes: parseInt(process.env.OTP_EXPIRY_MINUTES || 10)
            }
        });
    } catch (error) {
        console.error('❌ [RESEND] Error:', error);
        res.status(500).json({
            success: false,
            message: error.message || 'Failed to resend OTP'
        });
    }
};

// ============================================
// LOGIN - Check verification status
// ============================================
export const login = async (req, res) => {
    try {
        const { email, password } = req.body;

        console.log(`🔐 [LOGIN] Login attempt: ${email}`);

        // Find user
        const user = await User.findByEmail(email);
        if (!user) {
            return res.status(401).json({
                success: false,
                message: 'Invalid credentials'
            });
        }

        // Check if user has password (not Google login only)
        if (!user.password_hash && user.google_id) {
            return res.status(400).json({
                success: false,
                message: 'This account uses Google login. Please use "Continue with Google".'
            });
        }

        // Check password
        const isPasswordValid = await comparePassword(password, user.password_hash);
        if (!isPasswordValid) {
            return res.status(401).json({
                success: false,
                message: 'Invalid credentials'
            });
        }

        // Check if email is verified
        if (!user.is_verified) {
            // Send new OTP
            const otpCode = await createAndSendOTP(email, 'login');
            console.log(`🔑 [LOGIN] Verification OTP for ${email}: ${otpCode}`);

            return res.status(403).json({
                success: false,
                message: 'Email not verified. OTP sent to your email.',
                requiresVerification: true,
                data: {
                    email,
                    purpose: 'login'
                }
            });
        }

        // Generate tokens
        const tokens = generateTokens(user);

        res.status(200).json({
            success: true,
            message: 'Login successful',
            data: {
                user: {
                    id: user.id,
                    email: user.email,
                    full_name: user.full_name,
                    phone: user.phone,
                    is_verified: user.is_verified
                },
                tokens
            }
        });
    } catch (error) {
        console.error('❌ [LOGIN] Error:', error);
        res.status(500).json({
            success: false,
            message: 'Login failed',
            error: error.message
        });
    }
};

// ============================================
// GOOGLE LOGIN - Auto verified (Google handles verification)
// ============================================
export const googleLogin = async (req, res) => {
    try {
        const { idToken } = req.body;

        if (!idToken) {
            return res.status(400).json({
                success: false,
                message: 'ID token is required'
            });
        }

        console.log(`🔐 [GOOGLE] Google login attempt`);

        // Verify Google token
        const ticket = await googleClient.verifyIdToken({
            idToken,
            audience: process.env.GOOGLE_CLIENT_ID
        });

        const payload = ticket.getPayload();
        const { email, name, sub: googleId, email_verified } = payload;

        console.log(`🔐 [GOOGLE] User: ${email}, Verified: ${email_verified}`);

        // Check if user exists
        let user = await User.findByEmail(email);

        if (user) {
            // User exists, update google_id if not set
            if (!user.google_id) {
                await User.update(user.id, { google_id: googleId });
                user = await User.findById(user.id);
            }
            // Mark as verified if Google says verified
            if (email_verified && !user.is_verified) {
                await User.verify(user.id);
                user = await User.findById(user.id);
            }
        } else {
            // Create new user (verified by Google)
            user = await User.create({
                email,
                full_name: name || email.split('@')[0],
                google_id: googleId,
                is_verified: email_verified || false,
                is_guest: false
            });

            // If Google verified, mark as verified
            if (email_verified) {
                await User.verify(user.id);
                user = await User.findById(user.id);
            }
        }

        // Generate tokens
        const tokens = generateTokens(user);

        res.status(200).json({
            success: true,
            message: 'Google login successful',
            data: {
                user: {
                    id: user.id,
                    email: user.email,
                    full_name: user.full_name,
                    phone: user.phone,
                    is_verified: user.is_verified
                },
                tokens
            }
        });
    } catch (error) {
        console.error('❌ [GOOGLE] Error:', error);
        res.status(401).json({
            success: false,
            message: 'Google authentication failed',
            error: error.message
        });
    }
};

// ============================================
// FORGOT PASSWORD - Send OTP
// ============================================
export const forgotPassword = async (req, res) => {
    try {
        const { email } = req.body;

        console.log(`📧 [FORGOT] Password reset request: ${email}`);

        if (!email) {
            return res.status(400).json({
                success: false,
                message: 'Email is required'
            });
        }

        // Check if user exists
        const user = await User.findByEmail(email);
        if (!user) {
            return res.status(404).json({
                success: false,
                message: 'User not found with this email'
            });
        }

        // Check if user has password (not Google-only)
        if (!user.password_hash && user.google_id) {
            return res.status(400).json({
                success: false,
                message: 'This account uses Google login. Please use "Continue with Google".'
            });
        }

        // Delete old OTPs
        await OTP.deleteOld(email, 'forgot_password');

        // Generate and send OTP
        const otpCode = await createAndSendOTP(email, 'forgot_password');
        console.log(`🔑 [FORGOT] Password reset OTP for ${email}: ${otpCode}`);

        res.status(200).json({
            success: true,
            message: 'Password reset OTP sent to your email.',
            data: {
                email,
                purpose: 'forgot_password',
                expiryMinutes: parseInt(process.env.OTP_EXPIRY_MINUTES || 10)
            }
        });
    } catch (error) {
        console.error('❌ [FORGOT] Error:', error);
        res.status(500).json({
            success: false,
            message: error.message || 'Failed to send reset email'
        });
    }
};

// ============================================
// RESET PASSWORD - With OTP verification
// ============================================
export const resetPassword = async (req, res) => {
    try {
        const { email, otp, resetToken, newPassword } = req.body;

        console.log(`🔐 [RESET] Password reset for ${email}`);

        if (!email || (!otp && !resetToken) || !newPassword) {
            return res.status(400).json({
                success: false,
                message: 'Email, OTP/ResetToken and new password are required'
            });
        }

        // Verify Reset Token or OTP
        if (resetToken) {
            try {
                const decoded = jwt.verify(resetToken, process.env.JWT_SECRET);
                if (decoded.email !== email || decoded.purpose !== 'reset_password') {
                    return res.status(400).json({
                        success: false,
                        message: 'Invalid or expired reset token'
                    });
                }
            } catch (err) {
                return res.status(400).json({
                    success: false,
                    message: 'Invalid or expired reset token'
                });
            }
        } else {
            // Verify OTP
            const verification = await verifyOTPService(email, otp, 'forgot_password');
            if (!verification.success) {
                return res.status(400).json({
                    success: false,
                    message: verification.message
                });
            }
        }

        // Hash new password
        const hashedPassword = await hashPassword(newPassword);

        // Update user
        const user = await User.findByEmail(email);
        if (!user) {
            return res.status(404).json({
                success: false,
                message: 'User not found'
            });
        }

        await User.update(user.id, { password_hash: hashedPassword });

        res.status(200).json({
            success: true,
            message: 'Password reset successfully. Please login with your new password.'
        });
    } catch (error) {
        console.error('❌ [RESET] Error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to reset password',
            error: error.message
        });
    }
};

// ============================================
// GET CURRENT USER (With verification check)
// ============================================
export const getCurrentUser = async (req, res) => {
    try {
        const user = await User.findById(req.user.id);
        if (!user) {
            return res.status(404).json({
                success: false,
                message: 'User not found'
            });
        }

        // Check if email is verified
        if (!user.is_verified && !user.google_id) {
            return res.status(403).json({
                success: false,
                message: 'Email not verified. Please verify your email.',
                requiresVerification: true,
                data: {
                    user: {
                        id: user.id,
                        email: user.email,
                        full_name: user.full_name,
                        is_verified: false
                    }
                }
            });
        }

        res.status(200).json({
            success: true,
            data: user
        });
    } catch (error) {
        console.error('❌ [CURRENT] Error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get user',
            error: error.message
        });
    }
};

// ============================================
// REFRESH TOKEN
// ============================================
export const refreshToken = async (req, res) => {
    try {
        const { refreshToken } = req.body;

        if (!refreshToken) {
            return res.status(400).json({
                success: false,
                message: 'Refresh token required'
            });
        }

        // Verify refresh token
        let decoded;
        try {
            decoded = jwt.verify(refreshToken, process.env.JWT_REFRESH_SECRET);
        } catch (error) {
            return res.status(401).json({
                success: false,
                message: 'Invalid or expired refresh token'
            });
        }

        // Get user
        const user = await User.findById(decoded.id);
        if (!user) {
            return res.status(401).json({
                success: false,
                message: 'User not found'
            });
        }

        // Check if user is verified (unless Google user)
        if (!user.is_verified && !user.google_id) {
            return res.status(403).json({
                success: false,
                message: 'Email not verified. Please verify your email.'
            });
        }

        // Generate new tokens
        const tokens = generateTokens(user);

        res.status(200).json({
            success: true,
            data: tokens
        });
    } catch (error) {
        console.error('❌ [REFRESH] Error:', error);
        res.status(401).json({
            success: false,
            message: 'Invalid refresh token',
            error: error.message
        });
    }
};

// ============================================
// LOGOUT
// ============================================
export const logout = async (req, res) => {
    res.status(200).json({
        success: true,
        message: 'Logged out successfully'
    });
};