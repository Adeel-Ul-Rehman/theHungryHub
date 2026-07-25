// E:\hungryHub\hungry-fast-food\website\frontend\src\contexts\AuthContext.jsx

import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '../services/authService';

const AuthContext = createContext();

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [requiresVerification, setRequiresVerification] = useState(false);

    useEffect(() => {
        // Check if user is logged in on mount
        const storedUser = localStorage.getItem('user');
        const tokens = authService.getTokens();

        if (storedUser && tokens) {
            setUser(JSON.parse(storedUser));
            setRequiresVerification(false);
        }
        setLoading(false);
    }, []);

    const login = async (email, password) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.login(email, password);

            if (response.success) {
                const { user, tokens } = response.data;
                setUser(user);
                setRequiresVerification(false);
                localStorage.setItem('user', JSON.stringify(user));
                authService.setTokens(tokens);
                return { success: true };
            }

            return { success: false, message: response.message };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const signup = async (userData) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.signup(userData);

            if (response.success) {
                const { user, tokens } = response.data;
                setUser(user);
                setRequiresVerification(user.is_verified === false);
                localStorage.setItem('user', JSON.stringify(user));
                authService.setTokens(tokens);
                return { success: true, requiresVerification: !user.is_verified };
            }

            return { success: false, message: response.message };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const verifyOTP = async (email, otp, purpose) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.verifyOTP(email, otp, purpose);

            if (response.success) {
                if (user) {
                    setRequiresVerification(false);
                    const updatedUser = { ...user, is_verified: true };
                    setUser(updatedUser);
                    localStorage.setItem('user', JSON.stringify(updatedUser));
                }
                return { success: true, data: response.data };
            }

            return { success: false, message: response.message };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const resendOTP = async (email, purpose) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.resendOTP(email, purpose);
            return { success: true, data: response.data };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const googleLogin = async (idToken) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.googleLogin(idToken);

            if (response.success) {
                const { user, tokens } = response.data;
                setUser(user);
                setRequiresVerification(false);
                localStorage.setItem('user', JSON.stringify(user));
                authService.setTokens(tokens);
                return { success: true };
            }

            return { success: false, message: response.message };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const forgotPassword = async (email) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.forgotPassword(email);
            return { success: true, data: response.data };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const resetPassword = async (email, otp, newPassword, resetToken) => {
        try {
            setLoading(true);
            setError(null);

            const response = await authService.resetPassword(email, otp, newPassword, resetToken);
            return { success: true, message: response.message };
        } catch (err) {
            setError(err.message);
            return { success: false, message: err.message };
        } finally {
            setLoading(false);
        }
    };

    const logout = async () => {
        try {
            await authService.logout();
        } catch (err) {
            console.error('Logout error:', err);
        } finally {
            setUser(null);
            setRequiresVerification(false);
            localStorage.removeItem('user');
            authService.clearTokens();
        }
    };

    const value = {
        user,
        loading,
        error,
        requiresVerification,
        login,
        signup,
        verifyOTP,
        resendOTP,
        googleLogin,
        forgotPassword,
        resetPassword,
        logout,
        isAuthenticated: !!user,
        isVerified: user?.is_verified || false,
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};