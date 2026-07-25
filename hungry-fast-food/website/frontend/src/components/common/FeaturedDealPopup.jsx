// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\FeaturedDealPopup.jsx

import React from 'react';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';

export default function FeaturedDealPopup({ deal, onClose }) {
    const { addToCart } = useCart();

    if (!deal) return null;

    const handleAddDeal = () => {
        // Prepare deal item for cart
        const cartItem = {
            id: `deal-${deal.id}`,
            name: deal.name,
            price: parseFloat(deal.discount_price || deal.total_price),
            image_url: deal.image_url,
            is_deal: true,
            deal_id: deal.id,
            quantity: 1,
            // Include deal items so checkout knows what's inside
            items: deal.items || []
        };
        addToCart(cartItem);
        onClose();
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/15 backdrop-blur-xs animate-fade-in">
            <div className="relative bg-white w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden border border-gray-100 animate-slide-up">
                {/* Close Button */}
                <button
                    onClick={onClose}
                    className="absolute top-4 right-4 z-10 p-2 bg-white rounded-full text-text-primary hover:text-primary shadow-md hover:scale-105 transition-all duration-200"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-5 h-5">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>

                {/* Deal Image */}
                <div className="relative h-56 bg-red-100 flex items-center justify-center overflow-hidden">
                    {deal.image_url ? (
                        <img
                            src={deal.image_url}
                            alt={deal.name}
                            className="w-full h-full object-cover hover:scale-105 transition-transform duration-700"
                        />
                    ) : (
                        <div className="text-8xl">🎁</div>
                    )}
                    <div className="absolute bottom-4 left-4 bg-primary text-white text-xs uppercase font-extrabold px-3 py-1.5 rounded-full tracking-wider shadow-md animate-pulse">
                        Special Offer
                    </div>
                </div>

                {/* Deal Details */}
                <div className="p-6 md:p-8">
                    <h3 className="font-heading font-extrabold text-2xl text-text-primary mb-2">
                        {deal.name}
                    </h3>
                    <p className="text-text-secondary text-sm leading-relaxed mb-4">
                        {deal.description || 'Enjoy this limited-time promotional deal crafted specially for you!'}
                    </p>

                    {/* Deal Items Inside */}
                    {deal.items && deal.items.length > 0 && (
                        <div className="mb-6 bg-red-50 bg-opacity-50 p-4 rounded-xl border border-red-100 border-opacity-50">
                            <h4 className="font-bold text-xs uppercase tracking-wider text-primary mb-2.5">
                                What's Included:
                            </h4>
                            <ul className="flex flex-col gap-2">
                                {deal.items.map((item, idx) => (
                                    <li key={idx} className="flex items-center justify-between text-sm text-text-primary">
                                        <span className="font-medium">
                                            {item.quantity}x {item.product_name}
                                        </span>
                                        {item.variation_name && (
                                            <span className="text-xs text-text-secondary font-semibold bg-white border border-gray-200 px-2 py-0.5 rounded">
                                                {item.variation_name}
                                            </span>
                                        )}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}

                    {/* Price and Add button */}
                    <div className="flex items-center justify-between gap-4 mt-6">
                        <div className="flex flex-col">
                            <span className="text-xs text-text-secondary font-bold line-through">
                                {formatPrice(deal.total_price)}
                            </span>
                            <span className="text-2xl font-heading font-black text-primary">
                                {formatPrice(deal.discount_price || deal.total_price)}
                            </span>
                        </div>
                        <button
                            onClick={handleAddDeal}
                            className="btn-primary flex-grow text-center font-bold shadow-lg shadow-orange-100/50 hover:shadow-orange-200/50"
                        >
                            Claim This Deal
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
