// E:\hungryHub\hungry-fast-food\website\frontend\src\components\layout\HomePage.jsx

import React, { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import FeaturedDealPopup from '../common/FeaturedDealPopup';
import LoadingSpinner from '../common/LoadingSpinner';
import { formatPrice } from '../../utils/helpers';

export default function HomePage() {
    const [allDeals, setAllDeals] = useState([]);
    const [featuredDeal, setFeaturedDeal] = useState(null);
    const [showPopup, setShowPopup] = useState(false);
    const [selectedDeal, setSelectedDeal] = useState(null);
    const [loading, setLoading] = useState(true);
    const scrollRef = useRef(null);
    const { addToCart } = useCart();

    useEffect(() => {
        const fetchDeals = async () => {
            try {
                const response = await api.get('/menu/deals?is_active=true');
                if (response.success && response.data && response.data.length > 0) {
                    setAllDeals(response.data);
                    const featured = response.data.find(d => d.is_featured);
                    if (featured) {
                        setFeaturedDeal(featured);
                        setTimeout(() => setShowPopup(true), 1500);
                    }
                }
            } catch (error) {
                console.error('Failed to fetch deals:', error);
            } finally {
                setLoading(false);
            }
        };
        fetchDeals();
    }, []);

    const scrollLeft = () => {
        if (scrollRef.current) {
            scrollRef.current.scrollBy({ left: -320, behavior: 'smooth' });
        }
    };

    const scrollRight = () => {
        if (scrollRef.current) {
            scrollRef.current.scrollBy({ left: 320, behavior: 'smooth' });
        }
    };

    const handleQuickAdd = (deal, e) => {
        e.preventDefault();
        e.stopPropagation();
        addToCart({
            id: `deal-${deal.id}`,
            name: deal.name,
            price: parseFloat(deal.discount_price || deal.total_price),
            image_url: deal.image_url,
            is_deal: true,
            deal_id: deal.id,
            quantity: 1,
            items: deal.items || [],
        });
    };

    return (
        <div className="space-y-16 py-4">
            {/* Hero Section */}
            <section className="relative rounded-3xl bg-gradient-to-br from-secondary to-gray-900 overflow-hidden shadow-xl text-white py-16 px-6 md:px-12 md:py-24">
                <div className="absolute inset-0 bg-black opacity-10"></div>
                <div className="relative z-10 grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
                    <div className="space-y-6 text-center md:text-left">
                        <span className="inline-flex items-center bg-white text-primary text-[9px] sm:text-xs font-extrabold uppercase px-2.5 sm:px-3.5 py-1 sm:py-1.5 rounded-full tracking-wide shadow-sm">
                            🚀 Super Fast Delivery in Rawalpindi
                        </span>
                        <h1 className="font-heading font-black text-3xl sm:text-4xl md:text-6xl leading-tight">
                            Craving Hot, <br />
                            Tasty <span className="text-secondary font-black">Fast Food?</span>
                        </h1>
                        <p className="text-gray-300 text-sm md:text-base leading-relaxed max-w-md">
                            Experience the best burgers, hand-stretched pizzas, and exclusive value combos delivered fresh to your doorstep in 30 mins!
                        </p>
                        <div className="flex flex-col sm:flex-row items-center justify-center md:justify-start gap-4 pt-2">
                            <Link to="/menu" className="btn-primary w-full sm:w-auto px-8 py-3.5 text-center font-bold shadow-lg shadow-orange-100/30">
                                Order Now
                            </Link>
                            <Link to="/menu" className="border-2 border-white hover:bg-white hover:text-primary transition-colors text-white font-bold w-full sm:w-auto px-8 py-3.5 rounded-lg text-center">
                                View Menu
                            </Link>
                        </div>
                    </div>
                    <div className="flex justify-center select-none animate-bounce-slow">
                        <span className="text-[120px] md:text-[200px] drop-shadow-2xl">🍔</span>
                    </div>
                </div>
            </section>

            {/* All Deals – Horizontal Scrollable Row */}
            <section className="space-y-6">
                <div className="flex items-center justify-between gap-3 border-b border-gray-100 pb-4">
                    <div>
                        <h2 className="font-heading font-extrabold text-xl sm:text-2xl md:text-3xl text-text-primary flex items-center gap-2">
                            🎁 Value Combo Deals
                        </h2>
                        <p className="text-text-secondary text-sm">Scroll through our exclusive promotional bundles</p>
                    </div>
                    <Link to="/menu" className="text-primary font-bold hover:underline text-sm whitespace-nowrap">
                        See all &rarr;
                    </Link>
                </div>

                {loading ? (
                    <LoadingSpinner />
                ) : allDeals.length > 0 ? (
                    <div className="relative group px-1">
                        {/* Left Arrow (Absolute positioned at left-most edge) */}
                        <button
                            onClick={scrollLeft}
                            className="absolute left-1 md:left-[-22px] top-1/2 -translate-y-1/2 z-20 w-8 h-8 md:w-11 md:h-11 rounded-full bg-white hover:bg-primary hover:text-white text-text-primary border border-gray-200 flex items-center justify-center shadow-xl transition-all duration-200 hover:scale-110 active:scale-95 md:opacity-0 group-hover:opacity-100 focus:opacity-100 flex cursor-pointer"
                            aria-label="Scroll left"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={3} stroke="currentColor" className="w-4 h-4 md:w-5 md:h-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
                            </svg>
                        </button>

                        {/* Scroll Container */}
                        <div
                            ref={scrollRef}
                            className="flex gap-3 sm:gap-5 overflow-x-auto pb-4 scroll-smooth"
                            style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
                        >
                            {allDeals.map(deal => (
                                <div
                                    key={deal.id}
                                    className={`relative flex-shrink-0 w-[calc(50%-6px)] sm:w-72 rounded-2xl overflow-hidden bg-white group transition-all duration-300 hover:shadow-xl cursor-pointer flex flex-col ${
                                        deal.is_featured
                                            ? 'border-2 border-yellow-400 shadow-[0_0_18px_rgba(250,204,21,0.35)]'
                                            : 'border border-gray-100 shadow-md'
                                    }`}
                                >
                                    {/* Featured corner lights */}
                                    {deal.is_featured && (
                                        <>
                                            <div className="absolute top-0 left-0 w-3 h-3 bg-yellow-400 rounded-br-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                            <div className="absolute top-0 right-0 w-3 h-3 bg-yellow-400 rounded-bl-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                            <div className="absolute bottom-0 left-0 w-3 h-3 bg-yellow-400 rounded-tr-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                            <div className="absolute bottom-0 right-0 w-3 h-3 bg-yellow-400 rounded-tl-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                        </>
                                    )}

                                    {/* Image */}
                                    <Link to={`/deal/${deal.id}`} className="block">
                                        <div className="relative h-28 sm:h-44 bg-orange-50 flex items-center justify-center overflow-hidden">
                                            {deal.image_url ? (
                                                <img
                                                    src={deal.image_url}
                                                    alt={deal.name}
                                                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                                                />
                                            ) : (
                                                <span className="text-4xl sm:text-6xl select-none">🎁</span>
                                            )}
                                            <span className="absolute top-2 left-2 sm:top-3 sm:left-3 bg-primary text-white text-[9px] sm:text-xs font-extrabold px-2 py-0.5 sm:px-2.5 sm:py-1 rounded-full shadow-md">
                                                Deal
                                            </span>
                                            {deal.is_featured && (
                                                <span className="absolute top-2 right-2 sm:top-3 sm:right-3 bg-yellow-400 text-gray-900 text-[8px] sm:text-[10px] font-extrabold px-1.5 py-0.5 sm:px-2 sm:py-0.5 rounded-full shadow animate-pulse">
                                                    ⭐ Featured
                                                </span>
                                            )}
                                            {deal.discount_price && parseFloat(deal.discount_price) < parseFloat(deal.total_price) && (
                                                <span className="absolute bottom-2 right-2 sm:bottom-3 sm:right-3 bg-green-600 text-white text-[8px] sm:text-[10px] font-extrabold px-1.5 py-0.5 sm:px-2 sm:py-0.5 rounded-full shadow">
                                                    Save {Math.round(((parseFloat(deal.total_price) - parseFloat(deal.discount_price)) / parseFloat(deal.total_price)) * 100)}%
                                                </span>
                                            )}
                                        </div>
                                    </Link>

                                    {/* Content */}
                                    <div className="p-3 sm:p-4 flex-grow flex flex-col justify-between gap-2.5 sm:gap-3">
                                        <div>
                                            <Link to={`/deal/${deal.id}`}>
                                                <h3 className="font-bold text-text-primary text-xs sm:text-base leading-snug group-hover:text-primary transition-colors line-clamp-1">
                                                    {deal.name}
                                                </h3>
                                            </Link>
                                            <p className="text-text-secondary text-[10px] sm:text-xs leading-relaxed line-clamp-2 mt-0.5 sm:mt-1">
                                                {deal.description || 'Exclusive value combo packed with flavor.'}
                                            </p>
                                        </div>

                                        <div className="flex items-center justify-between gap-2 mt-auto pt-2 border-t border-gray-50">
                                            <div className="flex flex-col">
                                                {deal.discount_price && parseFloat(deal.discount_price) < parseFloat(deal.total_price) && (
                                                    <span className="text-[9px] sm:text-[11px] text-text-secondary line-through">
                                                        {formatPrice(deal.total_price)}
                                                    </span>
                                                )}
                                                <span className="text-sm sm:text-lg font-heading font-black text-primary">
                                                    {formatPrice(deal.discount_price || deal.total_price)}
                                                </span>
                                            </div>
                                            <button
                                                onClick={(e) => handleQuickAdd(deal, e)}
                                                className="bg-primary hover:bg-primary-dark text-white p-2 sm:p-2.5 rounded-lg sm:rounded-xl shadow hover:shadow-lg transition-all active:scale-95 group-hover:scale-105"
                                                title="Add to Cart"
                                            >
                                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-4 h-4">
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                                                </svg>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Right Arrow (Absolute positioned at right-most edge) */}
                        <button
                            onClick={scrollRight}
                            className="absolute right-1 md:right-[-22px] top-1/2 -translate-y-1/2 z-20 w-8 h-8 md:w-11 md:h-11 rounded-full bg-white hover:bg-primary hover:text-white text-text-primary border border-gray-200 flex items-center justify-center shadow-xl transition-all duration-200 hover:scale-110 active:scale-95 md:opacity-0 group-hover:opacity-100 focus:opacity-100 flex cursor-pointer"
                            aria-label="Scroll right"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={3} stroke="currentColor" className="w-4 h-4 md:w-5 md:h-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
                            </svg>
                        </button>
                    </div>
                ) : (
                    <div className="text-center py-10 bg-white rounded-2xl border border-gray-100">
                        <span className="text-4xl block mb-3">🎁</span>
                        <p className="text-text-secondary text-sm">No deals available right now. Check back soon!</p>
                    </div>
                )}
            </section>

            {/* Why Choose Us */}
            <section className="space-y-8">
                <div className="text-center space-y-2">
                    <h2 className="font-heading font-extrabold text-2xl md:text-3xl text-text-primary">
                        Why Choose HungryHub?
                    </h2>
                    <p className="text-text-secondary text-sm max-w-md mx-auto">
                        We are committed to delivering restaurant-quality fast food with exceptional services.
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-16 h-16 bg-orange-50 text-3xl flex items-center justify-center rounded-full mx-auto">
                            ⚡
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Super-fast Delivery</h3>
                        <p className="text-text-secondary text-sm leading-relaxed">
                            Our hot delivery bags ensure your meals arrive piping hot and fresh within 30 minutes.
                        </p>
                    </div>

                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-16 h-16 bg-orange-50 text-3xl flex items-center justify-center rounded-full mx-auto">
                            👑
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Quality Ingredients</h3>
                        <p className="text-text-secondary text-sm leading-relaxed">
                            We use 100% fresh meat patties, locally sourced veggies, and premium melted mozzarella cheese.
                        </p>
                    </div>

                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-16 h-16 bg-teal-50 text-3xl flex items-center justify-center rounded-full mx-auto">
                            🏷️
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Unbeatable Pricing</h3>
                        <p className="text-text-secondary text-sm leading-relaxed">
                            Get access to daily promo codes, discount combos, and custom budget deals for the whole family.
                        </p>
                    </div>
                </div>
            </section>

            {/* Featured Deal Popup */}
            {showPopup && featuredDeal && (
                <FeaturedDealPopup
                    deal={featuredDeal}
                    onClose={() => setShowPopup(false)}
                />
            )}
        </div>
    );
}
