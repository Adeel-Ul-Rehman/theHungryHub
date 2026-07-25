// E:\hungryHub\hungry-fast-food\website\frontend\src\components\pages\DealDetailPage.jsx

import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';
import LoadingSpinner from '../common/LoadingSpinner';

export default function DealDetailPage() {
    const { id } = useParams();
    const { addToCart } = useCart();

    const [deal, setDeal] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [addedToCart, setAddedToCart] = useState(false);

    useEffect(() => {
        const fetchDeal = async () => {
            setLoading(true);
            try {
                const response = await api.get(`/menu/deals/${id}`);
                if (response.success && response.data) {
                    setDeal(response.data);
                } else {
                    setError('Deal not found');
                }
            } catch (err) {
                setError('Failed to load deal details');
            } finally {
                setLoading(false);
            }
        };
        fetchDeal();
    }, [id]);

    const handleAddToCart = () => {
        const cartItem = {
            id: `deal-${deal.id}`,
            name: deal.name,
            price: parseFloat(deal.discount_price || deal.total_price),
            image_url: deal.image_url,
            is_deal: true,
            deal_id: deal.id,
            quantity: 1,
            items: deal.items || [],
        };
        addToCart(cartItem);
        setAddedToCart(true);
        setTimeout(() => setAddedToCart(false), 2000);
    };

    const savings = deal ? parseFloat(deal.total_price) - parseFloat(deal.discount_price || deal.total_price) : 0;
    const savingsPct = deal && deal.total_price > 0 ? Math.round((savings / parseFloat(deal.total_price)) * 100) : 0;

    if (loading) return (
        <div className="py-20">
            <LoadingSpinner />
        </div>
    );

    if (error || !deal) return (
        <div className="text-center py-20 space-y-4">
            <span className="text-6xl">😕</span>
            <h2 className="font-heading font-bold text-2xl text-text-primary">{error || 'Deal not found'}</h2>
            <Link to="/menu" className="btn-primary inline-block">Back to Menu</Link>
        </div>
    );

    return (
        <div className="py-4 max-w-5xl mx-auto">
            {/* Breadcrumb */}
            <nav className="flex items-center gap-2 text-sm text-text-secondary mb-8">
                <Link to="/" className="hover:text-primary transition-colors">Home</Link>
                <span>›</span>
                <Link to="/menu" className="hover:text-primary transition-colors">Menu</Link>
                <span>›</span>
                <Link to="/menu/deals" className="hover:text-primary transition-colors">Deals</Link>
                <span>›</span>
                <span className="text-text-primary font-semibold line-clamp-1">{deal.name}</span>
            </nav>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-10 items-start">
                {/* Image Section */}
                <div className={`relative rounded-2xl overflow-hidden bg-orange-50 shadow-xl aspect-square flex items-center justify-center group ${deal.is_featured ? 'border-2 border-yellow-400 shadow-[0_0_30px_rgba(250,204,21,0.3)]' : ''}`}>
                    {/* Featured corner lights */}
                    {deal.is_featured && (
                        <>
                            <div className="absolute top-0 left-0 w-4 h-4 bg-yellow-400 rounded-br-xl shadow-[0_0_12px_#facc15] animate-pulse z-10"></div>
                            <div className="absolute top-0 right-0 w-4 h-4 bg-yellow-400 rounded-bl-xl shadow-[0_0_12px_#facc15] animate-pulse z-10"></div>
                            <div className="absolute bottom-0 left-0 w-4 h-4 bg-yellow-400 rounded-tr-xl shadow-[0_0_12px_#facc15] animate-pulse z-10"></div>
                            <div className="absolute bottom-0 right-0 w-4 h-4 bg-yellow-400 rounded-tl-xl shadow-[0_0_12px_#facc15] animate-pulse z-10"></div>
                        </>
                    )}
                    {deal.image_url ? (
                        <img
                            src={deal.image_url}
                            alt={deal.name}
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
                        />
                    ) : (
                        <span className="text-[120px] select-none">🎁</span>
                    )}

                    {/* Badges */}
                    <span className="absolute top-4 left-4 bg-primary text-white text-sm font-extrabold px-3 py-1.5 rounded-full shadow-lg">
                        🎁 Combo Deal
                    </span>
                    {deal.is_featured && (
                        <span className="absolute top-4 right-4 bg-yellow-400 text-gray-900 text-xs font-extrabold px-3 py-1 rounded-full shadow-lg animate-pulse">
                            ⭐ Featured
                        </span>
                    )}
                    {savingsPct > 0 && (
                        <span className="absolute bottom-4 right-4 bg-green-600 text-white text-xs font-extrabold px-3 py-1.5 rounded-full shadow-lg">
                            Save {savingsPct}%
                        </span>
                    )}
                </div>

                {/* Details Section */}
                <div className="space-y-6">
                    <div className="flex flex-wrap gap-2">
                        <span className="inline-block bg-orange-100 text-primary text-xs uppercase font-extrabold px-3 py-1 rounded-full tracking-wider">
                            Combo Deal
                        </span>
                        {deal.is_featured && (
                            <span className="inline-block bg-yellow-100 text-yellow-800 text-xs uppercase font-extrabold px-3 py-1 rounded-full tracking-wider">
                                ⭐ Featured
                            </span>
                        )}
                    </div>

                    <h1 className="font-heading font-black text-3xl md:text-4xl text-text-primary leading-tight">
                        {deal.name}
                    </h1>

                    <p className="text-text-secondary text-sm leading-relaxed">
                        {deal.description || 'Enjoy this exclusive bundle pack loaded with extra flavors and sides! Perfect for sharing or solo enjoyment.'}
                    </p>

                    {/* Price */}
                    <div className="flex items-baseline gap-4">
                        {savings > 0 && (
                            <span className="text-lg text-text-secondary line-through font-semibold">
                                {formatPrice(deal.total_price)}
                            </span>
                        )}
                        <span className="text-4xl font-heading font-black text-primary">
                            {formatPrice(deal.discount_price || deal.total_price)}
                        </span>
                        {savings > 0 && (
                            <span className="text-sm font-bold text-green-600 bg-green-50 px-2.5 py-1 rounded-full">
                                Save {formatPrice(savings)}
                            </span>
                        )}
                    </div>

                    {/* What's Included */}
                    {deal.items && deal.items.length > 0 && (
                        <div className="bg-orange-50 border border-orange-100 rounded-2xl p-5 space-y-3">
                            <h3 className="font-bold text-sm uppercase tracking-wider text-primary">
                                🍽️ What's Included
                            </h3>
                            <ul className="space-y-2.5">
                                {deal.items.map((item, idx) => (
                                    <li key={idx} className="flex items-center justify-between gap-3">
                                        <div className="flex items-center gap-3">
                                            <span className="w-7 h-7 bg-primary text-white text-xs font-black rounded-full flex items-center justify-center shrink-0">
                                                {item.quantity}x
                                            </span>
                                            <span className="font-semibold text-text-primary text-sm">
                                                {item.product_name}
                                            </span>
                                        </div>
                                        {item.variation_name && (
                                            <span className="text-xs text-text-secondary font-semibold bg-white border border-gray-200 px-2.5 py-1 rounded-lg shrink-0">
                                                {item.variation_name}
                                            </span>
                                        )}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}

                    {/* Divider */}
                    <div className="border-t border-gray-100"></div>

                    {/* Add to Cart Button */}
                    <div className="flex flex-col sm:flex-row gap-3">
                        <button
                            onClick={handleAddToCart}
                            className={`flex-grow py-4 rounded-xl font-bold text-lg transition-all duration-300 shadow-lg flex items-center justify-center gap-3 ${
                                addedToCart
                                    ? 'bg-green-500 text-white shadow-green-200'
                                    : 'bg-primary hover:bg-primary-dark text-white shadow-orange-200 hover:shadow-orange-300 active:scale-95'
                            }`}
                        >
                            {addedToCart ? (
                                <>
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-6 h-6">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                                    </svg>
                                    Added to Cart!
                                </>
                            ) : (
                                <>
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-6 h-6">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 00-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 00-16.536-1.84M7.5 14.25L5.106 5.272M6 20.25a.75.75 0 11-1.5 0 .75.75 0 011.5 0zm12.75 0a.75.75 0 11-1.5 0 .75.75 0 011.5 0z" />
                                    </svg>
                                    Claim This Deal
                                </>
                            )}
                        </button>
                        <Link
                            to="/menu"
                            className="px-6 py-4 rounded-xl font-bold border-2 border-gray-200 text-text-secondary hover:border-primary hover:text-primary transition-all duration-200 text-center"
                        >
                            ← Back
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}
