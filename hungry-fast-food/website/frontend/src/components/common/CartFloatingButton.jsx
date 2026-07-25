// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\CartFloatingButton.jsx

import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { FaShoppingBag } from 'react-icons/fa';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';

export default function CartFloatingButton() {
    const { cartCount, cartTotal } = useCart();
    const location = useLocation();

    // Do not show on cart or checkout pages
    if (cartCount === 0 || location.pathname === '/cart' || location.pathname === '/checkout') {
        return null;
    }

    return (
        <div className="fixed bottom-0 left-0 right-0 z-30 md:hidden animate-slide-up bg-white border-t border-gray-100 p-3.5 shadow-[0_-5px_15px_rgba(0,0,0,0.06)]">
            <Link
                to="/cart"
                className="flex items-center justify-between bg-primary hover:bg-primary-dark text-white px-5 py-4 rounded-xl shadow-lg shadow-orange-100/50 active:scale-[0.98] transition-all duration-150 font-semibold"
            >
                <div className="flex items-center gap-3">
                    <div className="relative">
                        <FaShoppingBag className="w-5 h-5 shrink-0" />
                        <span className="absolute -top-2.5 -right-2.5 bg-yellow-400 text-gray-900 text-[10px] font-black w-5 h-5 flex items-center justify-center rounded-full border-2 border-primary">
                            {cartCount}
                        </span>
                    </div>
                    <span className="text-sm font-black uppercase tracking-wider">View your Cart</span>
                </div>
                
                <div className="flex items-center gap-2">
                    <span className="text-base font-black">{formatPrice(cartTotal)}</span>
                    <span className="text-xl">&rarr;</span>
                </div>
            </Link>
        </div>
    );
}
