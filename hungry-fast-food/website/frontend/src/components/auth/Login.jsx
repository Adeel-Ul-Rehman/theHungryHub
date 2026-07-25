// E:\hungryHub\hungry-fast-food\website\frontend\src\components\auth\Login.jsx

import React, { useState } from 'react';
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

    const handleGoogleLogin = async () => {
        // Here you would integrate Google Identity Services SDK
        // For demonstration, we trigger a prompt or mock token
        setToast({ type: 'success', message: 'Google Authentication Triggered' });
        // Under normal circumstances, you'd load the SDK, get the credential, and pass it to googleLogin(credential.credential)
        // Here we simulate successful token retrieval
        const mockGoogleIdToken = 'mock_google_id_token_jwt';
        const res = await googleLogin(mockGoogleIdToken);
        if (res.success) {
            navigate(from, { replace: true });
        } else {
            setToast({ type: 'error', message: res.message });
        }
    };

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
            <button
                onClick={handleGoogleLogin}
                className="w-full flex items-center justify-center gap-3 px-4 py-3 border border-gray-300 rounded-lg font-bold text-text-primary hover:bg-gray-50 transition-all duration-300 shadow-sm"
            >
                <svg className="w-5 h-5" viewBox="0 0 24 24">
                    <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
                    <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
                    <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z" />
                    <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z" />
                </svg>
                Sign In with Google
            </button>

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
