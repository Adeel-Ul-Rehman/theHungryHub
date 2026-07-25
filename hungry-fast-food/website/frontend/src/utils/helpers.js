// E:\hungryHub\hungry-fast-food\website\frontend\src\utils\helpers.js

import { APP_CONFIG } from './constants';

// Format price in PKR
export const formatPrice = (amount) => {
    return `PKR ${Number(amount).toLocaleString('en-PK')}`;
};

// Format date
export const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-PK', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
    });
};

// Truncate text
export const truncateText = (text, length = 50) => {
    if (!text || text.length <= length) return text;
    return text.substring(0, length) + '...';
};

// Generate order number display
export const formatOrderNumber = (orderNumber) => {
    return orderNumber || 'N/A';
};

// Calculate distance using Haversine formula
export const calculateDistance = (lat1, lon1, lat2, lon2) => {
    const R = 6371;
    const dLat = toRadians(lat2 - lat1);
    const dLon = toRadians(lon2 - lon1);
    const a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(toRadians(lat1)) * Math.cos(toRadians(lat2)) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
};

const toRadians = (degrees) => {
    return degrees * (Math.PI / 180);
};

// Check if delivery is available
export const checkDeliveryAvailability = (distance) => {
    const maxDistance = APP_CONFIG.MAX_DELIVERY_DISTANCE;
    return distance <= maxDistance;
};

// Get delivery charge based on distance
export const getDeliveryCharge = (distance) => {
    // This should match backend logic
    if (distance <= 5) return 0;
    if (distance <= 10) return 100;
    if (distance <= 12) return 200;
    return null; // Not deliverable
};

// Generate Google Maps link for delivery location
export const generateGoogleMapsLink = (address, latitude, longitude) => {
    if (latitude && longitude) {
        return `https://www.google.com/maps/search/?api=1&query=${latitude},${longitude}`;
    }
    if (address) {
        return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`;
    }
    return null;
};

// Validate email
export const isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
};

// Validate phone number (Pakistan format)
export const isValidPhone = (phone) => {
    const phoneRegex = /^[0-9+\-\s]{7,20}$/;
    return phoneRegex.test(phone);
};

// Get status badge color
export const getStatusColor = (status) => {
    const colors = {
        pending: 'bg-yellow-100 text-yellow-800',
        confirmed: 'bg-blue-100 text-blue-800',
        preparing: 'bg-purple-100 text-purple-800',
        ready: 'bg-green-100 text-green-800',
        completed: 'bg-green-600 text-white',
        cancelled: 'bg-red-100 text-red-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
};

// Get status label
export const getStatusLabel = (status) => {
    const labels = {
        pending: 'Pending',
        confirmed: 'Confirmed',
        preparing: 'Preparing',
        ready: 'Ready',
        completed: 'Completed',
        cancelled: 'Cancelled',
    };
    return labels[status] || status;
};

// Generate random ID (for cart items)
export const generateId = () => {
    return Math.random().toString(36).substring(2, 9);
};

// Debounce function
export const debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Get error message from API response
export const getErrorMessage = (error) => {
    if (typeof error === 'string') return error;
    if (error.message) return error.message;
    return 'Something went wrong. Please try again.';
};