// E:\hungryHub\hungry-fast-food\website\backend\src\middleware\auth.js

import jwt from 'jsonwebtoken';
import dotenv from 'dotenv';

dotenv.config();

// Verify JWT token
export const verifyToken = (req, res, next) => {
    const authHeader = req.headers.authorization;

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
        return res.status(401).json({
            success: false,
            message: 'No token provided'
        });
    }

    const token = authHeader.split(' ')[1];

    try {
        const decoded = jwt.verify(token, process.env.JWT_SECRET);
        req.user = decoded;
        next();
    } catch (error) {
        if (error.name === 'TokenExpiredError') {
            return res.status(401).json({
                success: false,
                message: 'Token expired',
                code: 'TOKEN_EXPIRED'
            });
        }
        return res.status(401).json({
            success: false,
            message: 'Invalid token'
        });
    }
};

// Verify refresh token
export const verifyRefreshToken = (req, res, next) => {
    const { refreshToken } = req.body;

    if (!refreshToken) {
        return res.status(401).json({
            success: false,
            message: 'No refresh token provided'
        });
    }

    try {
        const decoded = jwt.verify(refreshToken, process.env.JWT_REFRESH_SECRET);
        req.user = decoded;
        next();
    } catch (error) {
        return res.status(401).json({
            success: false,
            message: 'Invalid refresh token'
        });
    }
};

// Optional auth (doesn't fail if no token)
export const optionalAuth = (req, res, next) => {
    const authHeader = req.headers.authorization;

    if (authHeader && authHeader.startsWith('Bearer ')) {
        const token = authHeader.split(' ')[1];
        try {
            const decoded = jwt.verify(token, process.env.JWT_SECRET);
            req.user = decoded;
        } catch (error) {
            // Invalid token, but we don't fail
        }
    }
    next();
};

// Admin auth middleware
export const verifyAdmin = (req, res, next) => {
    // First verify token
    verifyToken(req, res, () => {
        // Check if user is admin (using email domain or specific admin email)
        const adminEmails = (process.env.ADMIN_EMAILS || '').split(',');

        if (!req.user || !req.user.email || !adminEmails.includes(req.user.email)) {
            return res.status(403).json({
                success: false,
                message: 'Admin access required'
            });
        }
        next();
    });
};

// Verify admin API key
export const verifyAdminApiKey = (req, res, next) => {
    const apiKey = req.headers['x-admin-api-key'];

    if (!apiKey || apiKey !== process.env.ADMIN_API_KEY) {
        return res.status(401).json({
            success: false,
            message: 'Invalid admin API key'
        });
    }
    next();
};

// Check if user is verified
export const isVerified = (req, res, next) => {
    if (!req.user || !req.user.is_verified) {
        return res.status(403).json({
            success: false,
            message: 'Email not verified'
        });
    }
    next();
};

// Generate tokens
export const generateTokens = (user) => {
    const payload = {
        id: user.id,
        email: user.email,
        full_name: user.full_name,
        is_verified: user.is_verified
    };

    const accessToken = jwt.sign(payload, process.env.JWT_SECRET, {
        expiresIn: process.env.JWT_ACCESS_EXPIRY || '15m'
    });

    const refreshToken = jwt.sign(payload, process.env.JWT_REFRESH_SECRET, {
        expiresIn: process.env.JWT_REFRESH_EXPIRY || '7d'
    });

    return { accessToken, refreshToken };
};