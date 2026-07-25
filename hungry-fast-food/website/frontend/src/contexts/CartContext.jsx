// E:\hungryHub\hungry-fast-food\website\frontend\src\contexts\CartContext.jsx

import React, { createContext, useContext, useState, useEffect } from 'react';
import { useLocalStorage } from '../hooks/useLocalStorage';
import ToastNotification from '../components/common/ToastNotification';
import { subscribeToSettingsUpdate } from '../services/pusherService';
import { api } from '../services/api';

const CartContext = createContext();

export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error('useCart must be used within a CartProvider');
    }
    return context;
};

export const CartProvider = ({ children }) => {
    const [cart, setCart] = useLocalStorage('cart', []);
    const [cartTotal, setCartTotal] = useState(0);
    const [cartCount, setCartCount] = useState(0);
    const [toast, setToast] = useState(null);
    const [taxRate, setTaxRate] = useState(5);
    const [isRestaurantOpen, setIsRestaurantOpen] = useState(true);
    const [closedMessage, setClosedMessage] = useState('');

    const fetchSettings = async () => {
        try {
            const response = await api.get('/menu/settings');
            if (response.success && response.data) {
                setTaxRate(response.data.tax_rate);
                if (response.data.is_currently_open !== undefined) setIsRestaurantOpen(response.data.is_currently_open);
                if (response.data.closed_message) setClosedMessage(response.data.closed_message);
            }
        } catch (err) {
            console.error("Failed to load settings:", err);
        }
    };

    useEffect(() => {
        fetchSettings();

        const unsubscribe = subscribeToSettingsUpdate(() => {
            fetchSettings();
        });
        
        return () => {
            unsubscribe();
        };
    }, []);

    useEffect(() => {
        updateTotals();
    }, [cart]);

    const updateTotals = () => {
        const count = cart.reduce((sum, item) => sum + item.quantity, 0);
        const total = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        setCartCount(count);
        setCartTotal(total);
    };

    const showToast = (message, type = 'success') => {
        setToast({ message, type });
    };

    const addToCart = (item) => {
        setCart(prev => {
            const existingIndex = prev.findIndex(i => i.id === item.id);

            if (existingIndex >= 0) {
                const updated = [...prev];
                updated[existingIndex].quantity += item.quantity || 1;
                return updated;
            }

            return [...prev, { ...item, quantity: item.quantity || 1 }];
        });
        showToast(`🛒 "${item.name}" added to cart!`, 'success');
    };

    const removeFromCart = (itemId) => {
        const item = cart.find(i => i.id === itemId);
        if (item) {
            showToast(`🗑️ "${item.name}" removed from cart.`, 'success');
        }
        setCart(prev => prev.filter(item => item.id !== itemId));
    };

    const updateQuantity = (itemId, quantity) => {
        if (quantity <= 0) {
            removeFromCart(itemId);
            return;
        }

        setCart(prev =>
            prev.map(item =>
                item.id === itemId ? { ...item, quantity } : item
            )
        );
    };

    const clearCart = () => {
        setCart([]);
    };

    const getCartSummary = () => {
        return {
            items: cart,
            total: cartTotal,
            count: cartCount,
            subtotal: cartTotal,
        };
    };

    const value = {
        cart,
        cartTotal,
        cartCount,
        taxRate,
        isRestaurantOpen,
        closedMessage,
        addToCart,
        removeFromCart,
        updateQuantity,
        clearCart,
        getCartSummary,
        showToast,
    };

    return (
        <CartContext.Provider value={value}>
            {children}
            {toast && (
                <ToastNotification
                    message={toast.message}
                    type={toast.type}
                    onClose={() => setToast(null)}
                />
            )}
        </CartContext.Provider>
    );
};