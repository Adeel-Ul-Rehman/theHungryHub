// E:\hungryHub\hungry-fast-food\website\frontend\src\utils\constants.js

export const ORDER_STATUSES = {
    PENDING: 'pending',
    CONFIRMED: 'confirmed',
    PREPARING: 'preparing',
    READY: 'ready',
    COMPLETED: 'completed',
    CANCELLED: 'cancelled',
};

export const ORDER_TYPES = {
    DINING: 'dining',
    DELIVERY: 'delivery',
    TAKEAWAY: 'takeaway',
};

export const PAYMENT_METHODS = {
    JAZZCASH: 'jazzcash',
    COD: 'cod',
    CASH: 'cash',
};

export const STATUS_COLORS = {
    pending: 'bg-yellow-100 text-yellow-800',
    confirmed: 'bg-blue-100 text-blue-800',
    preparing: 'bg-purple-100 text-purple-800',
    ready: 'bg-green-100 text-green-800',
    completed: 'bg-green-600 text-white',
    cancelled: 'bg-red-100 text-red-800',
};

export const STATUS_LABELS = {
    pending: 'Pending',
    confirmed: 'Confirmed',
    preparing: 'Preparing',
    ready: 'Ready',
    completed: 'Completed',
    cancelled: 'Cancelled',
};

export const ORDER_TYPE_LABELS = {
    dining: 'Dine In',
    delivery: 'Delivery',
    takeaway: 'Take Away',
};

export const API_ENDPOINTS = {
    AUTH: {
        LOGIN: '/auth/login',
        REGISTER: '/auth/register',
        VERIFY_OTP: '/auth/verify-otp',
        RESEND_OTP: '/auth/resend-otp',
        FORGOT_PASSWORD: '/auth/forgot-password',
        RESET_PASSWORD: '/auth/reset-password',
        GOOGLE_LOGIN: '/auth/google-login',
        REFRESH_TOKEN: '/auth/refresh-token',
        LOGOUT: '/auth/logout',
        ME: '/auth/me',
    },
    MENU: {
        CATEGORIES: '/menu/categories',
        PRODUCTS: '/menu/products',
        DEALS: '/menu/deals',
        FEATURED_DEAL: '/menu/deals/featured',
    },
    ORDERS: {
        CREATE: '/orders',
        TRACK: '/orders/track',
        CHECK_DELIVERY: '/orders/check-delivery',
        MY_ORDERS: '/orders/my-orders',
    },
    ADMIN: {
        ORDERS: '/admin/orders',
        DASHBOARD: '/admin/dashboard/stats',
        USERS: '/admin/users',
        SETTINGS: '/admin/settings',
        DELIVERY_ZONES: '/admin/delivery-zones',
        SUSPICIOUS_ORDERS: '/admin/suspicious-orders',
        LOGS: '/admin/logs',
    },
};

export const APP_CONFIG = {
    APP_NAME: import.meta.env.VITE_APP_NAME || 'Hungry Fast Food',
    CURRENCY: import.meta.env.VITE_CURRENCY || 'PKR',
    MAX_DELIVERY_DISTANCE: parseFloat(import.meta.env.VITE_MAX_DELIVERY_DISTANCE) || 12,
    RESTAURANT_LATITUDE: parseFloat(import.meta.env.VITE_RESTAURANT_LATITUDE) || 24.8607,
    RESTAURANT_LONGITUDE: parseFloat(import.meta.env.VITE_RESTAURANT_LONGITUDE) || 67.0011,
};