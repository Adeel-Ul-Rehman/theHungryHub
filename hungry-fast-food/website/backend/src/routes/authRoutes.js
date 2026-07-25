// E:\hungryHub\hungry-fast-food\website\backend\src\routes\authRoutes.js

import express from 'express';
import {
    register,
    login,
    googleLogin,
    verifyOTP,
    resendOTP,
    forgotPassword,
    resetPassword,
    refreshToken,
    logout,
    getCurrentUser
} from '../controllers/authController.js';
import { verifyToken } from '../middleware/auth.js';
import { validate } from '../middleware/validation.js';
import { schemas } from '../middleware/validation.js';
import { authLimiter, otpLimiter } from '../middleware/rateLimiter.js';

const router = express.Router();

// Public routes
router.post('/register', authLimiter, validate(schemas.signup), register);
router.post('/verify-otp', otpLimiter, validate(schemas.verifyOTP), verifyOTP);
router.post('/resend-otp', otpLimiter, validate(schemas.resendOTP), resendOTP);
router.post('/login', authLimiter, validate(schemas.login), login);
router.post('/google-login', authLimiter, validate(schemas.googleLogin), googleLogin);
router.post('/forgot-password', authLimiter, validate(schemas.forgotPassword), forgotPassword);
router.post('/reset-password', authLimiter, validate(schemas.resetPassword), resetPassword);
router.post('/refresh-token', refreshToken);

// Protected routes
router.post('/logout', verifyToken, logout);
router.get('/me', verifyToken, getCurrentUser);

export default router;