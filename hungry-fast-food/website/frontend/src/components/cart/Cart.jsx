// E:\hungryHub\hungry-fast-food\website\frontend\src\components\cart\Cart.jsx

import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';
import CartItem from './CartItem';
import { formatPrice } from '../../utils/helpers';
import ConfirmationModal from '../common/ConfirmationModal';

export default function Cart() {
    const { cart, cartTotal, clearCart, taxRate } = useCart();
    const { isAuthenticated, googleLogin } = useAuth();
    const navigate = useNavigate();

    const [showClearConfirm, setShowClearConfirm] = useState(false);
    const [showCheckoutOptions, setShowCheckoutOptions] = useState(false);
    const [showGuestWarning, setShowGuestWarning] = useState(false);
    const [isGoogleLoggingIn, setIsGoogleLoggingIn] = useState(false);

    const handleConfirmClear = () => {
        clearCart();
        setShowClearConfirm(false);
    };

    const handleProceedToCheckout = () => {
        if (isAuthenticated) {
            navigate('/checkout');
        } else {
            setShowCheckoutOptions(true);
        }
    };

    const handleGoogleLogin = async () => {
        try {
            setIsGoogleLoggingIn(true);
            const mockGoogleIdToken = 'mock_google_id_token_jwt';
            const res = await googleLogin(mockGoogleIdToken);
            if (res.success) {
                setShowCheckoutOptions(false);
                navigate('/checkout');
            }
        } catch (e) {
            console.error(e);
        } finally {
            setIsGoogleLoggingIn(false);
        }
    };

    const handleGuestCheckout = () => {
        setShowCheckoutOptions(false);
        setShowGuestWarning(true);
    };

    const confirmGuestCheckout = () => {
        localStorage.setItem('guest_checkout', 'true');
        setShowGuestWarning(false);
        navigate('/checkout');
    };

    if (cart.length === 0) {
        return (
            <div className="max-w-md mx-auto text-center py-16 px-6 bg-white rounded-2xl border border-gray-100 shadow-md">
                <span className="text-6xl block mb-6 select-none">🛒</span>
                <h2 className="font-heading font-extrabold text-2xl text-text-primary mb-2">
                    Your Cart is Empty
                </h2>
                <p className="text-text-secondary text-sm mb-8 max-w-xs mx-auto leading-relaxed">
                    Looks like you haven't added anything to your cart yet. Discover our delicious menu now!
                </p>
                <Link to="/menu" className="btn-primary inline-block font-bold px-8 shadow-lg shadow-orange-100/50">
                    Explore Menu
                </Link>
            </div>
        );
    }

    return (
        <>
            {/* Main Content */}
            <div className={`grid grid-cols-1 lg:grid-cols-3 gap-8 transition-all duration-300 ${showCheckoutOptions || showGuestWarning ? 'blur-sm pointer-events-none' : ''}`}>
                {/* Cart Items Column */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="flex items-center justify-between border-b border-gray-100 pb-4">
                        <h1 className="font-heading font-black text-2xl md:text-3xl text-text-primary">
                            Shopping Cart
                        </h1>
                        <button
                            onClick={() => setShowClearConfirm(true)}
                            className="text-text-secondary hover:text-primary font-semibold text-sm transition-colors"
                        >
                            Clear Cart
                        </button>
                    </div>

                    <div className="bg-white rounded-2xl border border-gray-100 p-5 md:p-6 shadow-md divide-y divide-gray-100">
                        {cart.map((item) => (
                            <CartItem key={item.id} item={item} />
                        ))}
                    </div>
                </div>

                {/* Summary Column */}
                <div className="space-y-6">
                    <h2 className="font-heading font-bold text-xl md:text-2xl text-text-primary border-b border-gray-100 pb-4">
                        Order Summary
                    </h2>

                    <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-md space-y-5">
                        <div className="space-y-3.5 border-b border-gray-100 pb-5 text-sm">
                            <div className="flex justify-between text-text-secondary">
                                <span>Subtotal</span>
                                <span className="font-semibold text-text-primary">{formatPrice(cartTotal)}</span>
                            </div>
                            <div className="flex justify-between text-text-secondary">
                                <span>Tax ({taxRate}% GST)</span>
                                <span className="font-semibold text-text-primary">{formatPrice(cartTotal * (taxRate / 100))}</span>
                            </div>
                            <div className="flex justify-between text-text-secondary">
                                <span>Delivery Fee</span>
                                <span className="font-semibold text-teal-600 bg-teal-50 px-2.5 py-0.5 rounded text-xs">
                                    Calculated at Checkout
                                </span>
                            </div>
                            <p className="text-xs text-text-secondary mt-1">
                                💡 Delivery fee varies by distance. Some zones may have minimum order requirements.
                            </p>
                        </div>

                        <div className="flex justify-between items-baseline pt-1">
                            <span className="font-bold text-text-primary">Total Est.</span>
                            <span className="text-2xl font-heading font-black text-primary">
                                {formatPrice(cartTotal + cartTotal * (taxRate / 100))}
                            </span>
                        </div>

                        <div className="pt-2">
                            <button
                                onClick={handleProceedToCheckout}
                                className="btn-primary w-full text-center font-bold shadow-lg shadow-orange-100/50 block py-3.5 hover:shadow-orange-200/50"
                            >
                                Proceed to Checkout
                            </button>
                        </div>

                        <Link
                            to="/menu"
                            className="text-center font-semibold text-xs text-text-secondary hover:text-primary transition-colors block"
                        >
                            &larr; Continue Shopping
                        </Link>
                    </div>
                </div>
            </div>

            {/* Clear Cart Confirmation Modal */}
            <ConfirmationModal
                isOpen={showClearConfirm}
                title="Clear Cart Confirmation"
                message="Are you sure you want to remove all items from your shopping cart? This action cannot be undone."
                confirmText="Clear All"
                cancelText="Cancel"
                onConfirm={handleConfirmClear}
                onCancel={() => setShowClearConfirm(false)}
                type="danger"
            />

            {/* Checkout Options Modal - With Backdrop Blur */}
            {showCheckoutOptions && (
                <>
                    {/* Backdrop with blur */}
                    <div className="fixed inset-0 z-40 bg-white/40 backdrop-blur-sm transition-all duration-300" />
                    
                    {/* Modal */}
                    <div className="fixed inset-0 z-50 overflow-y-auto flex items-center justify-center p-4">
                        <div className="bg-white rounded-3xl max-w-md w-full p-6 md:p-8 space-y-6 shadow-2xl relative animate-slide-up border border-gray-100">
                            <button
                                onClick={() => setShowCheckoutOptions(false)}
                                className="absolute top-4 right-4 text-gray-400 hover:text-text-primary text-xl font-bold p-2 transition-colors"
                            >
                                &times;
                            </button>
                            <div className="text-center space-y-2">
                                <h3 className="font-heading font-black text-2xl text-text-primary">
                                    Checkout Options
                                </h3>
                                <p className="text-text-secondary text-sm">
                                    Choose how you want to place your order:
                                </p>
                            </div>

                            <div className="space-y-3 pt-2">
                                {/* Log In */}
                                <button
                                    onClick={() => {
                                        setShowCheckoutOptions(false);
                                        navigate('/login', { state: { from: { pathname: '/checkout' } } });
                                    }}
                                    className="w-full py-4 bg-primary hover:bg-primary-dark text-white font-bold rounded-xl shadow-lg shadow-orange-100 hover:shadow-orange-200 transition-all flex items-center justify-center gap-2"
                                >
                                    🔐 Log In / Create Account
                                </button>

                                {/* Google Sign In */}
                                <button
                                    onClick={handleGoogleLogin}
                                    disabled={isGoogleLoggingIn}
                                    className="w-full py-4 bg-white border-2 border-gray-200 hover:border-gray-300 text-text-primary font-bold rounded-xl hover:bg-gray-50 transition-all flex items-center justify-center gap-2.5"
                                >
                                    <svg className="w-5 h-5" viewBox="0 0 24 24">
                                        <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
                                        <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
                                        <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z" />
                                        <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z" />
                                    </svg>
                                    Continue with Google
                                </button>

                                <div className="relative flex py-2 items-center">
                                    <div className="flex-grow border-t border-gray-100"></div>
                                    <span className="flex-shrink mx-4 text-xs font-semibold text-text-secondary uppercase">Or</span>
                                    <div className="flex-grow border-t border-gray-100"></div>
                                </div>

                                {/* Guest Checkout */}
                                <button
                                    onClick={handleGuestCheckout}
                                    className="w-full py-3.5 bg-gray-50 hover:bg-gray-100 border border-gray-200 text-text-secondary font-bold rounded-xl transition-all flex items-center justify-center gap-2"
                                >
                                    🛒 Place Order as Guest
                                </button>
                            </div>
                        </div>
                    </div>
                </>
            )}

            {/* Guest Checkout Warning Modal - With Backdrop Blur */}
            {showGuestWarning && (
                <>
                    {/* Backdrop with blur */}
                    <div className="fixed inset-0 z-40 bg-white/40 backdrop-blur-sm transition-all duration-300" />
                    
                    {/* Modal */}
                    <div className="fixed inset-0 z-50 overflow-y-auto flex items-center justify-center p-4">
                        <div className="bg-white rounded-3xl max-w-md w-full p-6 md:p-8 space-y-6 shadow-2xl relative animate-slide-up border border-gray-100">
                            <div className="text-center space-y-3">
                                <span className="text-5xl block select-none">⚠️</span>
                                <h3 className="font-heading font-black text-2xl text-amber-600">
                                    Guest Checkout Warning
                                </h3>
                                <p className="text-text-secondary text-sm leading-relaxed">
                                    Your order will be placed successfully, but <strong>you will not be able to track your order online</strong> from a user dashboard.
                                </p>
                            </div>

                            <div className="flex flex-col gap-2 pt-2">
                                <button
                                    onClick={confirmGuestCheckout}
                                    className="w-full py-3.5 bg-amber-500 hover:bg-amber-600 text-white font-bold rounded-xl shadow-lg shadow-amber-100 transition-all flex items-center justify-center"
                                >
                                    Proceed as Guest Anyway
                                </button>
                                <button
                                    onClick={() => {
                                        setShowGuestWarning(false);
                                        setShowCheckoutOptions(true);
                                    }}
                                    className="w-full py-3 bg-white border border-gray-200 text-text-secondary font-semibold rounded-xl hover:bg-gray-50 transition-all"
                                >
                                    Back to Options
                                </button>
                            </div>
                        </div>
                    </div>
                </>
            )}
        </>
    );
}