// E:\hungryHub\hungry-fast-food\website\frontend\src\components\auth\ForgotPassword.jsx

import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import ToastNotification from '../common/ToastNotification';

export default function ForgotPassword() {
    const { forgotPassword, loading } = useAuth();
    const [email, setEmail] = useState('');
    const [toast, setToast] = useState(null);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!email) {
            setToast({ type: 'error', message: 'Please enter your email' });
            return;
        }

        const res = await forgotPassword(email);
        if (res.success) {
            setToast({ type: 'success', message: 'OTP sent! Redirecting to verification...' });
            setTimeout(() => {
                navigate('/verify-otp', { state: { email, purpose: 'forgot_password' } });
            }, 1500);
        } else {
            setToast({ type: 'error', message: res.message });
        }
    };

    return (
        <div className="max-w-md mx-auto my-12 p-6 md:p-8 bg-white rounded-2xl shadow-xl border border-gray-100 animate-slide-up">
            <div className="text-center space-y-2 mb-8">
                <span className="text-4xl">🔑</span>
                <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary">
                    Forgot Password
                </h2>
                <p className="text-text-secondary text-sm">
                    Enter your email to receive a 6-digit OTP code to reset your password.
                </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5">
                <div className="space-y-1.5">
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Email Address</label>
                    <input
                        type="email"
                        placeholder="you@example.com"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="input-field"
                        required
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
                            Sending OTP...
                        </>
                    ) : (
                        'Request OTP'
                    )}
                </button>
            </form>

            <p className="mt-8 text-center text-sm text-text-secondary">
                Back to{' '}
                <Link to="/login" className="text-primary font-bold hover:underline">
                    Sign In
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
