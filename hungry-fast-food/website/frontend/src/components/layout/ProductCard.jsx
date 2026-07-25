// E:\hungryHub\hungry-fast-food\website\frontend\src\components\layout\ProductCard.jsx

import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';

export default function ProductCard({ product }) {
    const { addToCart } = useCart();
    const [selectedVariation, setSelectedVariation] = useState(null);
    const [price, setPrice] = useState(product.base_price);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modalQuantity, setModalQuantity] = useState(1);

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

    const handleQuickAddClick = (e) => {
        e.preventDefault();
        e.stopPropagation();
        if (product.variations && product.variations.length > 0) {
            setIsModalOpen(true);
        } else {
            handleAddToCart(e);
        }
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
                        {product.variations && product.variations.length > 0 ? (
                            <button
                                onClick={handleQuickAddClick}
                                className="bg-primary hover:bg-primary-dark text-white px-3 py-1.5 rounded-lg text-xs font-bold shadow hover:shadow-lg transition-all active:scale-95 flex items-center gap-1.5"
                            >
                                <span>Choose Size</span>
                            </button>
                        ) : (
                            <button
                                onClick={handleQuickAddClick}
                                className="bg-primary hover:bg-primary-dark text-white p-2 sm:p-2.5 rounded-lg shadow hover:shadow-lg transition-all active:scale-95 flex items-center justify-center gap-1 group-hover:scale-105"
                                title="Add to Cart"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-5 h-5">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                                </svg>
                            </button>
                        )}
                    </div>
                </div>
            </div>

            {/* Size Selection Modal Overlay */}
            {isModalOpen && (
                <div 
                    className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4 transition-all duration-300"
                    onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        setIsModalOpen(false);
                    }}
                >
                    <div 
                        className="bg-white rounded-3xl max-w-sm w-full p-6 space-y-6 shadow-2xl relative border border-gray-100 animate-slide-up text-left"
                        onClick={(e) => e.stopPropagation()}
                    >
                        {/* Close Button */}
                        <button
                            onClick={(e) => {
                                e.preventDefault();
                                e.stopPropagation();
                                setIsModalOpen(false);
                            }}
                            className="absolute top-4 right-4 text-gray-400 hover:text-text-primary text-xl font-bold p-2 transition-colors cursor-pointer"
                        >
                            &times;
                        </button>

                        {/* Product info */}
                        <div className="space-y-2">
                            <span className="text-[10px] uppercase font-extrabold tracking-wider bg-orange-50 text-primary px-2.5 py-1 rounded-full">
                                Size Selection
                            </span>
                            <h3 className="font-heading font-black text-xl text-text-primary mt-2">
                                {product.name}
                            </h3>
                            <p className="text-text-secondary text-xs leading-relaxed">
                                {product.description || 'Choose your preferred size/option below to add to your order.'}
                            </p>
                        </div>

                        {/* Variations list */}
                        <div className="space-y-2.5">
                            <label className="text-[10px] font-bold text-text-secondary uppercase tracking-wider block">
                                Available Sizes
                            </label>
                            <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
                                {product.variations.map((v) => {
                                    const optionPrice = parseFloat(product.discount_price || product.base_price) + parseFloat(v.price_adjustment || 0);
                                    const isSelected = selectedVariation?.id === v.id;
                                    return (
                                        <button
                                            key={v.id}
                                            onClick={(e) => {
                                                e.preventDefault();
                                                e.stopPropagation();
                                                setSelectedVariation(v);
                                                updatePrice(v);
                                            }}
                                            className={`w-full px-4 py-3 rounded-2xl border-2 flex items-center justify-between text-sm transition-all duration-200 ${
                                                isSelected
                                                    ? 'border-primary bg-orange-50/40 text-primary font-bold shadow-sm shadow-orange-100'
                                                    : 'border-gray-200 bg-white text-text-primary font-semibold hover:border-primary/50'
                                            }`}
                                        >
                                            <span>{v.variation_name}</span>
                                            <span className={isSelected ? 'font-black' : 'text-text-secondary font-medium'}>
                                                PKR {optionPrice.toFixed(0)}
                                            </span>
                                        </button>
                                    );
                                })}
                            </div>
                        </div>

                        {/* Quantity Selector */}
                        <div className="flex items-center justify-between pt-2 border-t border-gray-150">
                            <span className="text-xs font-bold text-text-primary uppercase tracking-wider">
                                Quantity
                            </span>
                            <div className="flex items-center border border-gray-200 rounded-xl bg-gray-50 overflow-hidden shadow-sm">
                                <button
                                    onClick={(e) => {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        if (modalQuantity > 1) setModalQuantity(modalQuantity - 1);
                                    }}
                                    className="px-3 py-1.5 hover:bg-gray-150 text-text-primary hover:text-primary transition-colors font-bold text-sm"
                                >
                                    -
                                </button>
                                <span className="px-4 text-xs font-bold text-text-primary">
                                    {modalQuantity}
                                </span>
                                <button
                                    onClick={(e) => {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        setModalQuantity(modalQuantity + 1);
                                    }}
                                    className="px-3 py-1.5 hover:bg-gray-150 text-text-primary hover:text-primary transition-colors font-bold text-sm"
                                >
                                    +
                                </button>
                            </div>
                        </div>

                        {/* Action CTA */}
                        <button
                            onClick={(e) => {
                                e.preventDefault();
                                e.stopPropagation();
                                const cartItem = {
                                    id: `${product.id}-${selectedVariation.id}`,
                                    product_id: product.id,
                                    name: product.name,
                                    price: parseFloat(price),
                                    image_url: product.image_url,
                                    is_deal: false,
                                    quantity: modalQuantity,
                                    variation_id: selectedVariation.id,
                                    variation_name: selectedVariation.variation_name,
                                };
                                addToCart(cartItem);
                                setIsModalOpen(false);
                                setModalQuantity(1); // Reset quantity
                            }}
                            className="w-full btn-primary py-4 text-center font-bold text-sm tracking-wide rounded-2xl shadow-lg shadow-orange-100/50 block"
                        >
                            Add to Cart (PKR {(price * modalQuantity).toFixed(0)})
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

