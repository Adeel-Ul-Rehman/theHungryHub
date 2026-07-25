// E:\hungryHub\hungry-fast-food\website\frontend\src\components\layout\MenuPage.jsx

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { api } from '../../services/api';
import { useMenu } from '../../hooks/useMenu';
import CategoryFilter from './CategoryFilter';
import ProductCard from './ProductCard';
import LoadingSpinner from '../common/LoadingSpinner';
import FeaturedDealPopup from '../common/FeaturedDealPopup';
import { formatPrice } from '../../utils/helpers';
import { useCart } from '../../contexts/CartContext';

export default function MenuPage() {
    const { category: urlCategorySlug } = useParams();
    const navigate = useNavigate();
    const { addToCart } = useCart();

    const { categories, products, deals, isLoading } = useMenu();
    const [selectedCategoryId, setSelectedCategoryId] = useState(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedDeal, setSelectedDeal] = useState(null);

    useEffect(() => {
        // If slug present in URL, set category
        if (urlCategorySlug && categories.length > 0) {
            const matchedCat = categories.find(c => c.slug === urlCategorySlug);
            if (matchedCat) {
                setSelectedCategoryId(matchedCat.id);
            }
        }
    }, [urlCategorySlug, categories]);

    const handleSelectCategory = (categoryId) => {
        setSelectedCategoryId(categoryId);
        if (categoryId) {
            const cat = categories.find(c => c.id === categoryId);
            if (cat) {
                navigate(`/menu/${cat.slug}`);
            }
        } else {
            navigate('/menu');
        }
    };

    // Filter products
    const filteredProducts = products.filter(product => {
        const matchesCategory = selectedCategoryId ? product.category_id === selectedCategoryId : true;
        const matchesSearch = searchQuery
            ? product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
              product.description?.toLowerCase().includes(searchQuery.toLowerCase())
            : true;
        return matchesCategory && matchesSearch;
    });

    const isDealsCategorySelected = () => {
        if (!selectedCategoryId) return false;
        const currentCat = categories.find(c => c.id === selectedCategoryId);
        return currentCat && (currentCat.name.toLowerCase().includes("deal") || currentCat.slug.toLowerCase().includes("deal"));
    };

    const handleClaimDeal = (deal) => {
        setSelectedDeal(deal);
    };

    const handleQuickAddDeal = (deal) => {
        const cartItem = {
            id: `deal-${deal.id}`,
            name: deal.name,
            price: parseFloat(deal.discount_price || deal.total_price),
            image_url: deal.image_url,
            is_deal: true,
            deal_id: deal.id,
            quantity: 1,
            items: deal.items || []
        };
        addToCart(cartItem);
    };

    return (
        <div className="space-y-10 py-4">
            {/* Search and Title */}
            <div className="flex flex-col md:flex-row items-center justify-between gap-6 border-b border-gray-100 pb-6">
                <div>
                    <h1 className="font-heading font-black text-3xl md:text-4xl text-text-primary text-center md:text-left">
                        Our Delicious Menu
                    </h1>
                    <p className="text-text-secondary text-sm text-center md:text-left">
                        Explore our wide range of premium fast food items and promotional bundles.
                    </p>
                </div>
                {/* Search Bar */}
                <div className="relative w-full md:max-w-md">
                    <input
                        type="text"
                        placeholder="Search for burgers, pizzas, sides..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="input-field pl-11 shadow-sm"
                    />
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5 absolute left-4 top-3.5 text-text-secondary">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.602 10.602z" />
                    </svg>
                </div>
            </div>

            {isLoading ? (
                <LoadingSpinner />
            ) : (
                <>
                    {/* Category Filter + Product/Deal Grid */}
                    <div className="space-y-6">
                        <CategoryFilter
                            categories={categories}
                            activeCategory={selectedCategoryId}
                            onSelectCategory={handleSelectCategory}
                        />

                        {/* Product Grid / Deals Grid */}
                        {isDealsCategorySelected() ? (
                            deals.length > 0 ? (
                                <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 sm:gap-6">
                                    {deals.map(deal => (
                                        <div
                                            key={deal.id}
                                            className={`card bg-white flex flex-col justify-between group cursor-pointer relative overflow-hidden transition-all duration-300 hover:shadow-xl ${deal.is_featured ? 'border-2 border-yellow-400 shadow-[0_0_16px_rgba(250,204,21,0.35)]' : 'border border-gray-100'}`}
                                        >
                                            {/* Featured glow corners */}
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
                                                <div className="relative h-48 bg-orange-50 flex items-center justify-center overflow-hidden">
                                                    {deal.image_url ? (
                                                        <img
                                                            src={deal.image_url}
                                                            alt={deal.name}
                                                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                                                        />
                                                    ) : (
                                                        <span className="text-6xl select-none">🎁</span>
                                                    )}
                                                    <span className="absolute top-3 left-3 bg-primary text-white text-xs font-bold px-2.5 py-1 rounded-full shadow-md">
                                                        Deal
                                                    </span>
                                                    {deal.is_featured && (
                                                        <span className="absolute top-3 right-3 bg-yellow-400 text-gray-900 text-[10px] font-extrabold px-2 py-0.5 rounded-full shadow animate-pulse">
                                                            ⭐ Featured
                                                        </span>
                                                    )}
                                                </div>
                                            </Link>

                                            {/* Content */}
                                            <div className="p-5 flex-grow flex flex-col justify-between gap-4">
                                                <div className="space-y-1.5">
                                                    <Link to={`/deal/${deal.id}`}>
                                                        <h3 className="font-bold text-text-primary text-lg leading-snug group-hover:text-primary transition-colors">
                                                            {deal.name}
                                                        </h3>
                                                    </Link>
                                                    <p className="text-text-secondary text-xs leading-relaxed line-clamp-2">
                                                        {deal.description || 'Taste the perfect combo pack put together just for you.'}
                                                    </p>
                                                </div>

                                                <div className="flex items-center justify-between gap-2 pt-1.5 mt-auto">
                                                    <div className="flex flex-col">
                                                        {deal.discount_price && deal.discount_price < deal.total_price && (
                                                            <span className="text-xs text-text-secondary line-through">
                                                                {formatPrice(deal.total_price)}
                                                            </span>
                                                        )}
                                                        <span className="text-lg font-heading font-black text-primary">
                                                            {formatPrice(deal.discount_price || deal.total_price)}
                                                        </span>
                                                    </div>
                                                    <button
                                                        onClick={() => handleQuickAddDeal(deal)}
                                                        className="bg-primary hover:bg-primary-dark text-white p-2.5 rounded-lg shadow hover:shadow-lg transition-all active:scale-95 flex items-center justify-center gap-1 group-hover:scale-105"
                                                        title="Add to Cart"
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-5 h-5">
                                                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                                                        </svg>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <div className="text-center py-16 bg-white rounded-2xl border border-gray-100 p-8">
                                    <span className="text-5xl block mb-4">🎁</span>
                                    <h3 className="font-bold text-text-primary text-lg mb-1">No deals found</h3>
                                    <p className="text-text-secondary text-sm">
                                        No promotional combos are currently active. Check back later!
                                    </p>
                                </div>
                            )
                        ) : (
                            <>
                                {/* All products grid */}
                                {filteredProducts.length > 0 ? (
                                    <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 sm:gap-6">
                                        {filteredProducts.map(product => (
                                            <ProductCard key={product.id} product={product} />
                                        ))}
                                    </div>
                                ) : (
                                    !selectedCategoryId && deals.length === 0 && (
                                        <div className="text-center py-16 bg-white rounded-2xl border border-gray-100 p-8">
                                            <span className="text-5xl block mb-4">🔍</span>
                                            <h3 className="font-bold text-text-primary text-lg mb-1">No products found</h3>
                                            <p className="text-text-secondary text-sm">
                                                Try checking another category or refining your search parameters.
                                            </p>
                                        </div>
                                    )
                                )}

                                {/* Show deals in 'All' view */}
                                {!selectedCategoryId && deals.length > 0 && (
                                    <div className="space-y-4 mt-4">
                                        <h2 className="font-heading font-bold text-xl text-text-primary flex items-center gap-2">
                                            🎁 Value Combo Deals
                                        </h2>
                                        <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 sm:gap-6">
                                            {deals.map(deal => (
                                                <div
                                                    key={deal.id}
                                                    className={`card bg-white flex flex-col justify-between group cursor-pointer relative overflow-hidden transition-all duration-300 hover:shadow-xl ${deal.is_featured ? 'border-2 border-yellow-400 shadow-[0_0_16px_rgba(250,204,21,0.35)]' : 'border border-gray-100'}`}
                                                >
                                                    {deal.is_featured && (
                                                        <>
                                                            <div className="absolute top-0 left-0 w-3 h-3 bg-yellow-400 rounded-br-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                                            <div className="absolute top-0 right-0 w-3 h-3 bg-yellow-400 rounded-bl-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                                            <div className="absolute bottom-0 left-0 w-3 h-3 bg-yellow-400 rounded-tr-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                                            <div className="absolute bottom-0 right-0 w-3 h-3 bg-yellow-400 rounded-tl-lg shadow-[0_0_8px_#facc15] animate-pulse z-10"></div>
                                                        </>
                                                    )}
                                                    <Link to={`/deal/${deal.id}`} className="block">
                                                        <div className="relative h-48 bg-orange-50 flex items-center justify-center overflow-hidden">
                                                            {deal.image_url ? (
                                                                <img src={deal.image_url} alt={deal.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                                                            ) : (
                                                                <span className="text-6xl select-none">🎁</span>
                                                            )}
                                                            <span className="absolute top-3 left-3 bg-primary text-white text-xs font-bold px-2.5 py-1 rounded-full shadow-md">Deal</span>
                                                            {deal.is_featured && (
                                                                <span className="absolute top-3 right-3 bg-yellow-400 text-gray-900 text-[10px] font-extrabold px-2 py-0.5 rounded-full shadow animate-pulse">⭐ Featured</span>
                                                            )}
                                                        </div>
                                                    </Link>
                                                    <div className="p-5 flex-grow flex flex-col justify-between gap-4">
                                                        <div className="space-y-1.5">
                                                            <Link to={`/deal/${deal.id}`}>
                                                                <h3 className="font-bold text-text-primary text-lg leading-snug group-hover:text-primary transition-colors">{deal.name}</h3>
                                                            </Link>
                                                            <p className="text-text-secondary text-xs leading-relaxed line-clamp-2">{deal.description || 'Exclusive value combo packed with flavor.'}</p>
                                                        </div>
                                                        <div className="flex items-center justify-between gap-2 pt-1.5 mt-auto">
                                                            <div className="flex flex-col">
                                                                {deal.discount_price && parseFloat(deal.discount_price) < parseFloat(deal.total_price) && (
                                                                    <span className="text-xs text-text-secondary line-through">{formatPrice(deal.total_price)}</span>
                                                                )}
                                                                <span className="text-lg font-heading font-black text-primary">{formatPrice(deal.discount_price || deal.total_price)}</span>
                                                            </div>
                                                            <button
                                                                onClick={() => handleQuickAddDeal(deal)}
                                                                className="bg-primary hover:bg-primary-dark text-white p-2.5 rounded-lg shadow hover:shadow-lg transition-all active:scale-95"
                                                                title="Add to Cart"
                                                            >
                                                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-5 h-5">
                                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                                                                </svg>
                                                            </button>
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </>
                        )}
                    </div>
                </>
            )}

            {/* Deal Info Modal Popup */}
            {selectedDeal && (
                <FeaturedDealPopup
                    deal={selectedDeal}
                    onClose={() => setSelectedDeal(null)}
                />
            )}
        </div>
    );
}
