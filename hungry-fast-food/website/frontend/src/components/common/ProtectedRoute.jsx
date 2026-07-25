// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\ProtectedRoute.jsx

import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import LoadingSpinner from './LoadingSpinner';

export default function ProtectedRoute({ children }) {
    const { isAuthenticated, requiresVerification, loading, user } = useAuth();
    const location = useLocation();

    if (loading) {
        return (
            <div className="h-screen flex items-center justify-center">
                <LoadingSpinner />
            </div>
        );
    }

    if (!isAuthenticated) {
        // Redirect to login but save the current location they were trying to access
        const isGuestAllowed = localStorage.getItem('guest_checkout') === 'true';
        if (isGuestAllowed) {
            return children;
        }
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (requiresVerification || (user && !user.is_verified)) {
        // Redirect unverified users to OTP verification
        return <Navigate to="/verify-otp" state={{ email: user?.email, purpose: 'signup' }} replace />;
    }

    return children;
}
