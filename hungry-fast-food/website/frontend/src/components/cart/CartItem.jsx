// E:\hungryHub\hungry-fast-food\website\frontend\src\components\cart\CartItem.jsx

import React from 'react';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';

export default function CartItem({ item }) {
    const { updateQuantity, removeFromCart } = useCart();

    return (
        <div className="flex items-center gap-4 py-4 border-b border-gray-100 last:border-b-0 animate-fade-in">
            {/* Image */}
            <div className="w-16 h-16 md:w-20 md:h-20 bg-gray-50 rounded-lg flex items-center justify-center overflow-hidden flex-shrink-0">
                {item.image_url ? (
                    <img
                        src={item.image_url}
                        alt={item.name}
                        className="w-full h-full object-cover"
                    />
                ) : (
                    <span className="text-3xl select-none">{item.is_deal ? '🎁' : '🍕'}</span>
                )}
            </div>

            {/* Details */}
            <div className="flex-grow min-w-0 space-y-1">
                <div className="flex justify-between items-start gap-2">
                    <h4 className="font-bold text-text-primary text-sm md:text-base leading-snug truncate">
                        {item.name}
                    </h4>
                    <button
                        onClick={() => removeFromCart(item.id)}
                        className="text-text-secondary hover:text-primary p-1 rounded-full hover:bg-orange-50 transition-colors"
                        title="Remove item"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-4 h-4">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                <div className="flex flex-wrap gap-2 items-center">
                    {item.is_deal && (
                        <span className="bg-orange-100 text-primary text-[9px] uppercase font-extrabold px-2 py-0.5 rounded tracking-wider">
                            Combo Deal
                        </span>
                    )}
                    {item.variation_name && (
                        <span className="text-[10px] text-text-secondary font-bold bg-gray-100 px-2 py-0.5 rounded border border-gray-200">
                            {item.variation_name}
                        </span>
                    )}
                </div>

                {/* Adjust quantities & price */}
                <div className="flex items-center justify-between pt-1">
                    {/* Quantity selectors */}
                    <div className="flex items-center border border-gray-200 rounded-lg bg-gray-50 overflow-hidden shadow-sm">
                        <button
                            onClick={() => updateQuantity(item.id, item.quantity - 1)}
                            className="px-2.5 py-1 hover:bg-gray-150 text-text-primary hover:text-primary transition-colors font-bold text-sm"
                        >
                            -
                        </button>
                        <span className="px-3.5 text-xs font-bold text-text-primary">
                            {item.quantity}
                        </span>
                        <button
                            onClick={() => updateQuantity(item.id, item.quantity + 1)}
                            className="px-2.5 py-1 hover:bg-gray-150 text-text-primary hover:text-primary transition-colors font-bold text-sm"
                        >
                            +
                        </button>
                    </div>

                    {/* Total Price */}
                    <span className="font-heading font-extrabold text-sm md:text-base text-primary">
                        {formatPrice(item.price * item.quantity)}
                    </span>
                </div>
            </div>
        </div>
    );
}
