import React, { useEffect } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { FaCheckCircle, FaTimesCircle, FaReceipt, FaShoppingBag, FaArrowRight } from 'react-icons/fa';

export function OrderSuccess() {
    const [searchParams] = useSearchParams();
    const { clearCart } = useCart();
    const orderId = searchParams.get('id');
    const navigate = useNavigate();

    // Clear cart upon successful payment checkout
    useEffect(() => {
        clearCart();
    }, [clearCart]);

    return (
        <div className="max-w-md mx-auto my-12 p-8 bg-white rounded-2xl shadow-xl border border-gray-100 text-center animate-slide-up">
            <div className="flex justify-center mb-6">
                <FaCheckCircle className="text-green-500 w-20 h-20 animate-bounce-slow" />
            </div>
            
            <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary mb-3">
                Payment Successful!
            </h2>
            
            <p className="text-text-secondary text-sm md:text-base leading-relaxed mb-6">
                Thank you for your order! Your payment via JazzCash was received successfully. We are now preparing your delicious food.
            </p>

            {orderId && (
                <div className="bg-gray-50 border border-gray-100 rounded-xl p-4 mb-8 flex items-center justify-between text-left">
                    <div className="flex items-center gap-3">
                        <FaReceipt className="text-primary w-5 h-5" />
                        <div>
                            <span className="text-xs text-text-secondary font-semibold uppercase block">Order Reference</span>
                            <span className="text-sm font-bold text-text-primary">#{orderId}</span>
                        </div>
                    </div>
                    <Link
                        to={`/orders`}
                        className="text-primary text-xs font-bold flex items-center gap-1 hover:underline"
                    >
                        Track Order <FaArrowRight className="w-3 h-3" />
                    </Link>
                </div>
            )}

            <div className="flex flex-col gap-3">
                <Link
                    to="/orders"
                    className="w-full py-3.5 bg-primary hover:bg-primary-dark text-white font-bold rounded-xl shadow-lg shadow-orange-100 hover:shadow-orange-200 transition-all flex items-center justify-center gap-2"
                >
                    <FaShoppingBag className="w-4 h-4" /> Go to Order History
                </Link>
                
                <Link
                    to="/menu"
                    className="w-full py-3.5 bg-gray-50 hover:bg-gray-100 text-text-primary font-bold rounded-xl transition-all block text-center"
                >
                    Back to Menu
                </Link>
            </div>
        </div>
    );
}

export function OrderFailed() {
    const [searchParams] = useSearchParams();
    const reason = searchParams.get('reason') || 'Transaction cancelled or declined.';
    const orderId = searchParams.get('id');

    return (
        <div className="max-w-md mx-auto my-12 p-8 bg-white rounded-2xl shadow-xl border border-gray-100 text-center animate-slide-up">
            <div className="flex justify-center mb-6">
                <FaTimesCircle className="text-red-500 w-20 h-20 animate-pulse" />
            </div>
            
            <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary mb-3">
                Payment Failed
            </h2>
            
            <p className="text-text-secondary text-sm md:text-base leading-relaxed mb-6">
                We're sorry, but your transaction could not be processed.
            </p>

            <div className="bg-red-50 border border-red-100 rounded-xl p-4 mb-8 text-left">
                <span className="text-xs text-red-500 font-bold uppercase block mb-1">Reason</span>
                <span className="text-sm text-text-primary font-medium">{reason}</span>
            </div>

            <div className="flex flex-col gap-3">
                <Link
                    to="/cart"
                    className="w-full py-3.5 bg-primary hover:bg-primary-dark text-white font-bold rounded-xl shadow-lg shadow-orange-100 hover:shadow-orange-200 transition-all block text-center"
                >
                    Retry Checkout
                </Link>
                
                <Link
                    to="/menu"
                    className="w-full py-3.5 bg-gray-50 hover:bg-gray-100 text-text-primary font-bold rounded-xl transition-all block text-center"
                >
                    Back to Menu
                </Link>
            </div>
        </div>
    );
}
