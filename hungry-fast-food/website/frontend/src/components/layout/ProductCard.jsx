// E:\hungryHub\hungry-fast-food\website\frontend\src\components\layout\ProductCard.jsx

import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';

export default function ProductCard({ product }) {
    const { addToCart } = useCart();
    const [selectedVariation, setSelectedVariation] = useState(null);
    const [price, setPrice] = useState(product.base_price);

    useEffect(() => {
        if (product.variations && product.variations.length > 0) {
            const defaultVar = product.variations.find(v => v.is_default) || product.variations[0];
            setSelectedVariation(defaultVar);
            updatePrice(defaultVar);
        } else {
            setPrice(product.discount_price || product.base_price);
        }
    }, [product]);

    const updatePrice = (variation) => {
        const base = parseFloat(product.discount_price || product.base_price);
        const adjustment = parseFloat(variation?.price_adjustment || 0);
        setPrice(base + adjustment);
    };

    const handleVariationChange = (e) => {
        const varId = e.target.value;
        const variation = product.variations.find(v => v.id.toString() === varId);
        setSelectedVariation(variation);
        updatePrice(variation);
    };

    const handleAddToCart = (e) => {
        e.preventDefault();
        e.stopPropagation();
        const cartItem = {
            id: selectedVariation ? `${product.id}-${selectedVariation.id}` : product.id,
            product_id: product.id,
            name: product.name,
            price: parseFloat(price),
            image_url: product.image_url,
            is_deal: false,
            quantity: 1,
            variation_id: selectedVariation ? selectedVariation.id : null,
            variation_name: selectedVariation ? selectedVariation.variation_name : null,
        };

        addToCart(cartItem);
    };

    return (
        <div className="card bg-white border border-gray-100 flex flex-col justify-between group hover:shadow-xl transition-all duration-300">
            {/* Image - clickable */}
            <Link to={`/product/${product.id}`} className="block">
                <div className="relative h-32 sm:h-48 bg-gray-50 flex items-center justify-center overflow-hidden">
                    {product.image_url ? (
                        <img
                            src={product.image_url}
                            alt={product.name}
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                        />
                    ) : (
                        <span className="text-4xl sm:text-6xl select-none">🍔</span>
                    )}
                    {product.discount_price && (
                        <span className="absolute top-2 left-2 sm:top-3 sm:left-3 bg-primary text-white text-[10px] sm:text-xs font-bold px-2 py-0.5 sm:px-2.5 sm:py-1 rounded-full shadow-md animate-pulse">
                            Sale
                        </span>
                    )}
                </div>
            </Link>

            {/* Content */}
            <div className="p-3 sm:p-5 flex-grow flex flex-col justify-between gap-3 sm:gap-4">
                <div className="space-y-1">
                    <Link to={`/product/${product.id}`}>
                        <h3 className="font-bold text-text-primary text-sm sm:text-lg leading-snug group-hover:text-primary transition-colors line-clamp-1 sm:line-clamp-none">
                            {product.name}
                        </h3>
                    </Link>
                    <p className="text-text-secondary text-[10px] sm:text-xs leading-relaxed line-clamp-2">
                        {product.description || 'Deliciously fresh and hot, prepared with high quality ingredients.'}
                    </p>
                </div>

                <div className="space-y-3 mt-auto">
                    {/* Variations dropdown if available */}
                    {product.variations && product.variations.length > 0 && (
                        <div className="space-y-1">
                            <label className="text-[10px] font-bold text-text-secondary uppercase tracking-wider">
                                Choose Size/Option
                            </label>
                            <select
                                onChange={handleVariationChange}
                                value={selectedVariation?.id || ''}
                                className="w-full text-xs font-semibold px-2 py-1.5 border border-gray-200 rounded bg-white text-text-primary focus:outline-none focus:border-primary"
                                onClick={(e) => e.stopPropagation()}
                            >
                                {product.variations.map((v) => {
                                    const optionPrice = parseFloat(product.discount_price || product.base_price) + parseFloat(v.price_adjustment || 0);
                                    return (
                                        <option key={v.id} value={v.id}>
                                            {v.variation_name} (PKR {optionPrice.toFixed(0)})
                                        </option>
                                    );
                                })}
                            </select>
                        </div>
                    )}

                    {/* Price and Add button */}
                    <div className="flex items-center justify-between gap-2 pt-1.5">
                        <div className="flex flex-col">
                            {product.discount_price && (
                                <span className="text-xs text-text-secondary line-through">
                                    {formatPrice(parseFloat(product.base_price) + parseFloat(selectedVariation?.price_adjustment || 0))}
                                </span>
                            )}
                            <span className="text-lg font-heading font-black text-primary">
                                {formatPrice(price)}
                            </span>
                        </div>
                        <button
                            onClick={handleAddToCart}
                            className="bg-primary hover:bg-primary-dark text-white p-2 sm:p-2.5 rounded-lg shadow hover:shadow-lg transition-all active:scale-95 flex items-center justify-center gap-1 group-hover:scale-105"
                            title="Add to Cart"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-5 h-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                            </svg>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

