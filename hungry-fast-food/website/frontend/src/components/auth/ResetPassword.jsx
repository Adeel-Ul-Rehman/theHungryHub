// E:\hungryHub\hungry-fast-food\website\frontend\src\components\auth\ResetPassword.jsx

import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import ToastNotification from '../common/ToastNotification';

export default function ResetPassword() {
    const { resetPassword, loading } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();

    const [email, setEmail] = useState('');
    const [resetToken, setResetToken] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [toast, setToast] = useState(null);

    useEffect(() => {
        if (location.state?.email && location.state?.resetToken) {
            setEmail(location.state.email);
            setResetToken(location.state.resetToken);
        } else {
            // If no email or resetToken, redirect to forgot password
            navigate('/forgot-password');
        }
    }, [location, navigate]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!newPassword) {
            setToast({ type: 'error', message: 'Please enter a new password' });
            return;
        }

        const res = await resetPassword(email, null, newPassword, resetToken);
        if (res.success) {
            setToast({ type: 'success', message: 'Password reset successfully! Redirecting to login...' });
            setTimeout(() => {
                navigate('/login');
            }, 2000);
        } else {
            setToast({ type: 'error', message: res.message });
        }
    };

    return (
        <div className="max-w-md mx-auto my-12 p-6 md:p-8 bg-white rounded-2xl shadow-xl border border-gray-100 animate-slide-up">
            <div className="text-center space-y-2 mb-8">
                <span className="text-4xl">🔐</span>
                <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary">
                    Set New Password
                </h2>
                <p className="text-text-secondary text-sm">
                    OTP Verified! Please enter a new password for <strong className="text-text-primary">{email}</strong>.
                </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5">
                <div className="space-y-1.5">
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">New Password</label>
                    <input
                        type="password"
                        placeholder="Minimum 6 characters"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        className="input-field"
                        required
                        minLength={6}
                        autoFocus
                    />
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="btn-primary w-full font-bold shadow-lg shadow-orange-100/50 flex items-center justify-center gap-2"
                >
                    {loading ? (
                        <>
                            <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                            </svg>
                            Resetting Password...
                        </>
                    ) : (
                        'Save Password'
                    )}
                </button>
            </form>

            <p className="mt-8 text-center text-sm text-text-secondary font-semibold">
                Remember your password?{' '}
                <Link to="/login" className="text-primary font-bold hover:underline">
                    Login
                </Link>
            </p>

            {toast && (
                <ToastNotification
                    message={toast.message}
                    type={toast.type}
                    onClose={() => setToast(null)}
                />
            )}
        </div>
    );
}
