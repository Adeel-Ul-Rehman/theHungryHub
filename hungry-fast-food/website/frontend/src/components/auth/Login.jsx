// E:\hungryHub\hungry-fast-food\website\frontend\src\components\auth\Login.jsx

import React, { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import ToastNotification from '../common/ToastNotification';

export default function Login() {
    const { login, googleLogin, loading } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [toast, setToast] = useState(null);
    const navigate = useNavigate();
    const location = useLocation();

    const from = location.state?.from?.pathname || '/';

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!email || !password) {
            setToast({ type: 'error', message: 'Please fill in all fields' });
            return;
        }

        const res = await login(email, password);
        if (res.success) {
            navigate(from, { replace: true });
        } else {
            // Check if OTP verification is required
            if (res.message && res.message.includes('not verified')) {
                setToast({ type: 'error', message: 'Redirecting to verify your email...' });
                setTimeout(() => {
                    navigate('/verify-otp', { state: { email, purpose: 'login' } });
                }, 1500);
            } else {
                setToast({ type: 'error', message: res.message });
            }
        }
    };

    const handleGoogleCredentialResponse = async (response) => {
        try {
            const res = await googleLogin(response.credential);
            if (res.success) {
                navigate(from, { replace: true });
            } else {
                setToast({ type: 'error', message: res.message });
            }
        } catch (error) {
            console.error('Google Sign-In Error:', error);
            setToast({ type: 'error', message: 'Failed to authenticate with Google' });
        }
    };

    useEffect(() => {
        const initGoogleSignIn = () => {
            if (window.google && window.google.accounts) {
                window.google.accounts.id.initialize({
                    client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID || '375239912076-placeholder.apps.googleusercontent.com',
                    callback: handleGoogleCredentialResponse,
                });
                window.google.accounts.id.renderButton(
                    document.getElementById('google-signin-button'),
                    { theme: 'outline', size: 'large', width: '320' }
                );
            }
        };

        initGoogleSignIn();
        const timer = setTimeout(initGoogleSignIn, 1000);
        return () => clearTimeout(timer);
    }, []);

    return (
        <div className="max-w-md mx-auto my-12 p-6 md:p-8 bg-white rounded-2xl shadow-xl border border-gray-100 animate-slide-up">
            <div className="text-center space-y-2 mb-8">
                <img src="/logo.png" alt="Logo" className="h-12 w-auto mx-auto object-contain" />
                <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary">
                    Welcome Back
                </h2>
                <p className="text-text-secondary text-sm">Sign in to your HungryHub account</p>
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

                <div className="space-y-1.5">
                    <div className="flex justify-between items-center">
                        <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Password</label>
                        <Link to="/forgot-password" className="text-xs text-primary font-bold hover:underline">
                            Forgot?
                        </Link>
                    </div>
                    <input
                        type="password"
                        placeholder="••••••••"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
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
                            Signing In...
                        </>
                    ) : (
                        'Sign In'
                    )}
                </button>
            </form>

            <div className="relative my-6 text-center">
                <hr className="border-gray-200" />
                <span className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-white px-3 text-xs text-text-secondary font-bold uppercase tracking-wider">
                    Or Continue With
                </span>
            </div>

            {/* Google Login Button */}
            <div className="w-full flex justify-center">
                <div id="google-signin-button"></div>
            </div>

            <p className="mt-8 text-center text-sm text-text-secondary">
                Don't have an account?{' '}
                <Link to="/signup" className="text-primary font-bold hover:underline">
                    Register here
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
