// E:\hungryHub\hungry-fast-food\website\frontend\src\components\pages\ProductDetailPage.jsx

import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { api } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils/helpers';
import LoadingSpinner from '../common/LoadingSpinner';

export default function ProductDetailPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { addToCart } = useCart();

    const [product, setProduct] = useState(null);
    const [selectedVariation, setSelectedVariation] = useState(null);
    const [price, setPrice] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [addedToCart, setAddedToCart] = useState(false);

    useEffect(() => {
        const fetchProduct = async () => {
            setLoading(true);
            try {
                const response = await api.get(`/menu/products/${id}`);
                if (response.success && response.data) {
                    const p = response.data;
                    setProduct(p);
                    if (p.variations && p.variations.length > 0) {
                        const def = p.variations.find(v => v.is_default) || p.variations[0];
                        setSelectedVariation(def);
                        const base = parseFloat(p.discount_price || p.base_price);
                        setPrice(base + parseFloat(def.price_adjustment || 0));
                    } else {
                        setPrice(parseFloat(p.discount_price || p.base_price));
                    }
                } else {
                    setError('Product not found');
                }
            } catch (err) {
                setError('Failed to load product details');
            } finally {
                setLoading(false);
            }
        };
        fetchProduct();
    }, [id]);

    const handleVariationChange = (variation) => {
        setSelectedVariation(variation);
        const base = parseFloat(product.discount_price || product.base_price);
        setPrice(base + parseFloat(variation.price_adjustment || 0));
    };

    const handleAddToCart = () => {
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
        setAddedToCart(true);
        setTimeout(() => setAddedToCart(false), 2000);
    };

    if (loading) return (
        <div className="py-20">
            <LoadingSpinner />
        </div>
    );

    if (error || !product) return (
        <div className="text-center py-20 space-y-4">
            <span className="text-6xl">😕</span>
            <h2 className="font-heading font-bold text-2xl text-text-primary">{error || 'Product not found'}</h2>
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
                <span className="text-text-primary font-semibold line-clamp-1">{product.name}</span>
            </nav>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-10 items-start">
                {/* Image Section */}
                <div className="relative rounded-2xl overflow-hidden bg-orange-50 shadow-xl aspect-square flex items-center justify-center group">
                    {product.image_url ? (
                        <img
                            src={product.image_url}
                            alt={product.name}
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
                        />
                    ) : (
                        <span className="text-[120px] select-none">🍔</span>
                    )}
                    {product.discount_price && (
                        <span className="absolute top-4 left-4 bg-primary text-white text-sm font-extrabold px-3 py-1.5 rounded-full shadow-lg animate-pulse">
                            🔥 Sale
                        </span>
                    )}
                </div>

                {/* Details Section */}
                <div className="space-y-6">
                    {/* Category badge */}
                    {product.category_name && (
                        <span className="inline-block bg-orange-100 text-primary text-xs uppercase font-extrabold px-3 py-1 rounded-full tracking-wider">
                            {product.category_name}
                        </span>
                    )}

                    <h1 className="font-heading font-black text-3xl md:text-4xl text-text-primary leading-tight">
                        {product.name}
                    </h1>

                    {/* Product Description Section */}
                    <div className="space-y-2">
                        <h4 className="text-xs font-bold text-text-secondary uppercase tracking-wider">Product Description</h4>
                        <p className="text-text-secondary text-sm leading-relaxed bg-gray-50 border border-gray-100 rounded-xl p-4">
                            {product.description || 'Deliciously fresh and hot, prepared with high quality ingredients.'}
                        </p>
                    </div>


                    {/* Price */}
                    <div className="flex items-baseline gap-4">
                        {product.discount_price && (
                            <span className="text-lg text-text-secondary line-through font-semibold">
                                {formatPrice(parseFloat(product.base_price) + parseFloat(selectedVariation?.price_adjustment || 0))}
                            </span>
                        )}
                        <span className="text-4xl font-heading font-black text-primary">
                            {formatPrice(price)}
                        </span>
                    </div>

                    {/* Variations */}
                    {product.variations && product.variations.length > 0 && (
                        <div className="space-y-3">
                            <p className="text-sm font-bold text-text-primary uppercase tracking-wider">Choose Size / Option</p>
                            <div className="flex flex-wrap gap-3">
                                {product.variations.map(v => (
                                    <button
                                        key={v.id}
                                        onClick={() => handleVariationChange(v)}
                                        className={`px-4 py-2 rounded-xl text-sm font-semibold border-2 transition-all duration-200 ${
                                            selectedVariation?.id === v.id
                                                ? 'border-primary bg-primary text-white shadow-lg shadow-orange-100/50'
                                                : 'border-gray-200 bg-white text-text-primary hover:border-primary hover:text-primary'
                                        }`}
                                    >
                                        {v.variation_name}
                                        {parseFloat(v.price_adjustment) !== 0 && (
                                            <span className="ml-1 text-xs opacity-75">
                                                {parseFloat(v.price_adjustment) > 0 ? '+' : ''}{v.price_adjustment} PKR
                                            </span>
                                        )}
                                    </button>
                                ))}
                            </div>
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
                                    Add to Cart
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
