// E:\hungryHub\hungry-fast-food\website\frontend\src\components\auth\Signup.jsx

import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import ToastNotification from '../common/ToastNotification';

export default function Signup() {
    const { signup, loading } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [fullName, setFullName] = useState('');
    const [phone, setPhone] = useState('');
    const [toast, setToast] = useState(null);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!email || !password || !fullName) {
            setToast({ type: 'error', message: 'Please fill in all required fields' });
            return;
        }

        const res = await signup({
            email,
            password,
            full_name: fullName,
            phone
        });

        if (res.success) {
            setToast({ type: 'success', message: 'Account created! Redirecting to verification...' });
            setTimeout(() => {
                navigate('/verify-otp', { state: { email, purpose: 'signup' } });
            }, 1500);
        } else {
            setToast({ type: 'error', message: res.message });
        }
    };

    return (
        <div className="max-w-md mx-auto my-12 p-6 md:p-8 bg-white rounded-2xl shadow-xl border border-gray-100 animate-slide-up">
            <div className="text-center space-y-2 mb-8">
                <img src="/logo.png" alt="Logo" className="h-12 w-auto mx-auto object-contain" />
                <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary">
                    Create Account
                </h2>
                <p className="text-text-secondary text-sm">Register to order fresh fast food</p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-1.5">
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Full Name *</label>
                    <input
                        type="text"
                        placeholder="John Doe"
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        className="input-field"
                        required
                    />
                </div>

                <div className="space-y-1.5">
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Email Address *</label>
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
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Phone Number</label>
                    <input
                        type="text"
                        placeholder="e.g. 03001234567"
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        className="input-field"
                    />
                </div>

                <div className="space-y-1.5">
                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Password *</label>
                    <input
                        type="password"
                        placeholder="Minimum 6 characters"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="input-field"
                        required
                        minLength={6}
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
                            Creating Account...
                        </>
                    ) : (
                        'Register'
                    )}
                </button>
            </form>

            <p className="mt-8 text-center text-sm text-text-secondary font-semibold">
                Already have an account?{' '}
                <Link to="/login" className="text-primary font-bold hover:underline">
                    Sign In instead
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
