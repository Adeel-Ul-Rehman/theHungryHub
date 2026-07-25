// E:\hungryHub\hungry-fast-food\website\frontend\src\components\cart\Cart.jsx

import React, { useState, useEffect } from 'react';
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

    const handleGoogleCredentialResponse = async (response) => {
        try {
            setIsGoogleLoggingIn(true);
            const res = await googleLogin(response.credential);
            if (res.success) {
                setShowCheckoutOptions(false);
                navigate('/checkout');
            }
        } catch (error) {
            console.error('Google Sign-In Error:', error);
        } finally {
            setIsGoogleLoggingIn(false);
        }
    };

    useEffect(() => {
        const initGoogleSignIn = () => {
            if (showCheckoutOptions && window.google && window.google.accounts) {
                window.google.accounts.id.initialize({
                    client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID || '375239912076-placeholder.apps.googleusercontent.com',
                    callback: handleGoogleCredentialResponse,
                });
                window.google.accounts.id.renderButton(
                    document.getElementById('google-signin-button-cart'),
                    { theme: 'outline', size: 'large', width: '320' }
                );
            }
        };

        initGoogleSignIn();
        const timer = setTimeout(initGoogleSignIn, 1000);
        return () => clearTimeout(timer);
    }, [showCheckoutOptions]);

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
                                <div className="w-full flex justify-center">
                                    <div id="google-signin-button-cart"></div>
                                </div>

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