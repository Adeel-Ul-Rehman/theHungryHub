// E:\hungryHub\hungry-fast-food\website\frontend\src\components\auth\OTPVerification.jsx

import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import ToastNotification from '../common/ToastNotification';

export default function OTPVerification() {
    const { verifyOTP, resendOTP, loading } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();

    const [email, setEmail] = useState('');
    const [purpose, setPurpose] = useState('signup');
    const [otp, setOtp] = useState('');
    const [timer, setTimer] = useState(60);
    const [toast, setToast] = useState(null);

    useEffect(() => {
        if (location.state?.email) {
            setEmail(location.state.email);
            setPurpose(location.state.purpose || 'signup');
        } else {
            // Default fallback if reached directly
            navigate('/login');
        }
    }, [location, navigate]);

    // Resend countdown timer
    useEffect(() => {
        if (timer > 0) {
            const interval = setInterval(() => {
                setTimer(prev => prev - 1);
            }, 1000);
            return () => clearInterval(interval);
        }
    }, [timer]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (otp.length !== 6) {
            setToast({ type: 'error', message: 'Please enter a 6-digit OTP code' });
            return;
        }

        const res = await verifyOTP(email, otp, purpose);
        if (res.success) {
            setToast({ type: 'success', message: 'Verification successful!' });
            setTimeout(() => {
                if (purpose === 'forgot_password') {
                    navigate('/reset-password', {
                        state: {
                            email,
                            resetToken: res.data?.resetToken
                        }
                    });
                } else {
                    navigate('/');
                }
            }, 1500);
        } else {
            setToast({ type: 'error', message: res.message });
        }
    };

    const handleResend = async () => {
        if (timer > 0) return;
        const res = await resendOTP(email, purpose);
        if (res.success) {
            setToast({ type: 'success', message: 'A new OTP code has been sent to your email.' });
            setTimer(60);
        } else {
            setToast({ type: 'error', message: res.message });
        }
    };

    return (
        <div className="max-w-md mx-auto my-12 p-6 md:p-8 bg-white rounded-2xl shadow-xl border border-gray-100 animate-slide-up">
            <div className="text-center space-y-2 mb-8">
                <span className="text-4xl">✉️</span>
                <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary">
                    Verify Your Email
                </h2>
                <p className="text-text-secondary text-sm">
                    Enter the 6-digit verification code sent to <br />
                    <strong className="text-text-primary">{email}</strong>
                </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
                <div className="space-y-1.5">
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider block text-center mb-2">
                        Verification Code
                    </label>
                    <input
                        type="text"
                        maxLength="6"
                        placeholder="000000"
                        value={otp}
                        onChange={(e) => setOtp(e.target.value.replace(/\D/g, ''))}
                        className="input-field tracking-[0.3em] text-center font-black text-2xl py-4 bg-gray-50 focus:bg-white"
                        required
                        autoFocus
                    />
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="btn-primary w-full font-bold shadow-lg shadow-orange-100/50 flex items-center justify-center gap-2 pt-3"
                >
                    {loading ? (
                        <>
                            <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                            </svg>
                            Verifying...
                        </>
                    ) : (
                        'Verify Code'
                    )}
                </button>
            </form>

            <div className="mt-8 text-center space-y-4">
                <p className="text-sm text-text-secondary font-semibold">
                    Didn't receive the email?
                </p>
                <button
                    onClick={handleResend}
                    disabled={timer > 0 || loading}
                    className={`font-bold text-sm transition-colors ${
                        timer > 0
                            ? 'text-gray-400 cursor-not-allowed'
                            : 'text-primary hover:text-primary-dark underline'
                    }`}
                >
                    {timer > 0 ? `Resend Code in ${timer}s` : 'Resend Code'}
                </button>
            </div>

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
