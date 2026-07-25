// E:\hungryHub\hungry-fast-food\website\frontend\src\services\authService.js

import { api } from './api';

class AuthService {
    // Register user
    async signup(userData) {
        return api.post('/auth/register', userData);
    }

    // Login user
    async login(email, password) {
        return api.post('/auth/login', { email, password });
    }

    // Google login
    async googleLogin(idToken) {
        return api.post('/auth/google-login', { idToken });
    }

    // Verify OTP
    async verifyOTP(email, otp, purpose) {
        return api.post('/auth/verify-otp', { email, otp, purpose });
    }

    // Resend OTP
    async resendOTP(email, purpose) {
        return api.post('/auth/resend-otp', { email, purpose });
    }

    // Forgot password
    async forgotPassword(email) {
        return api.post('/auth/forgot-password', { email });
    }

    // Reset password
    async resetPassword(email, otp, newPassword, resetToken) {
        return api.post('/auth/reset-password', { email, otp, newPassword, resetToken });
    }

    // Refresh token
    async refreshToken() {
        const tokens = this.getTokens();
        return api.post('/auth/refresh-token', { refreshToken: tokens.refreshToken });
    }

    // Logout
    async logout() {
        const tokens = this.getTokens();
        if (tokens?.accessToken) {
            try {
                await api.post('/auth/logout', {}, { includeAuth: true });
            } catch (error) {
                console.error('Logout error:', error);
            }
        }
        this.clearTokens();
    }

    // Get current user
    async getCurrentUser() {
        return api.get('/auth/me', { includeAuth: true });
    }

    // Token management
    getTokens() {
        const tokens = localStorage.getItem('tokens');
        return tokens ? JSON.parse(tokens) : null;
    }

    setTokens(tokens) {
        localStorage.setItem('tokens', JSON.stringify(tokens));
    }

    clearTokens() {
        localStorage.removeItem('tokens');
        localStorage.removeItem('user');
    }

    // Check if user is authenticated
    isAuthenticated() {
        const tokens = this.getTokens();
        return tokens && tokens.accessToken;
    }
}

export const authService = new AuthService();