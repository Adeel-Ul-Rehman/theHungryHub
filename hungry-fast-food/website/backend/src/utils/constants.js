// E:\hungryHub\hungry-fast-food\website\backend\src\utils\constants.js

// Order statuses
export const ORDER_STATUSES = {
    PENDING: 'pending',
    CONFIRMED: 'confirmed',
    PREPARING: 'preparing',
    READY: 'ready',
    COMPLETED: 'completed',
    CANCELLED: 'cancelled'
};

// Order types
export const ORDER_TYPES = {
    DINING: 'dining',
    DELIVERY: 'delivery',
    TAKEAWAY: 'takeaway'
};

// Payment methods
export const PAYMENT_METHODS = {
    JAZZCASH: 'jazzcash',
    COD: 'cod',
    CASH: 'cash'
};

// Payment statuses
export const PAYMENT_STATUSES = {
    PENDING: 'pending',
    COMPLETED: 'completed',
    FAILED: 'failed',
    REFUNDED: 'refunded'
};

// OTP purposes
export const OTP_PURPOSES = {
    SIGNUP: 'signup',
    LOGIN: 'login',
    FORGOT_PASSWORD: 'forgot_password'
};

// Variation types
export const VARIATION_TYPES = {
    SIZE: 'size',
    FLAVOR: 'flavor',
    OPTION: 'option'
};

// HTTP status codes
export const HTTP_STATUS = {
    OK: 200,
    CREATED: 201,
    BAD_REQUEST: 400,
    UNAUTHORIZED: 401,
    FORBIDDEN: 403,
    NOT_FOUND: 404,
    CONFLICT: 409,
    TOO_MANY_REQUESTS: 429,
    INTERNAL_SERVER_ERROR: 500
};

// Error messages
export const ERROR_MESSAGES = {
    // Auth errors
    USER_NOT_FOUND: 'User not found',
    INVALID_CREDENTIALS: 'Invalid credentials',
    EMAIL_ALREADY_EXISTS: 'Email already exists',
    EMAIL_NOT_VERIFIED: 'Email not verified',
    TOKEN_EXPIRED: 'Token expired',
    INVALID_TOKEN: 'Invalid token',
    UNAUTHORIZED: 'Unauthorized access',
    FORBIDDEN: 'Access forbidden',
    RATE_LIMIT_EXCEEDED: 'Too many requests, please try again later',

    // Order errors
    ORDER_NOT_FOUND: 'Order not found',
    ORDER_STATUS_INVALID: 'Invalid order status transition',
    ORDER_CANNOT_BE_CANCELLED: 'Order cannot be cancelled',
    DELIVERY_NOT_AVAILABLE: 'Delivery not available for this location',

    // Validation errors
    VALIDATION_FAILED: 'Validation failed',
    MISSING_FIELDS: 'Missing required fields',
    INVALID_EMAIL: 'Invalid email format',
    INVALID_PHONE: 'Invalid phone number format',
    INVALID_OTP: 'Invalid or expired OTP',
    PASSWORD_TOO_WEAK: 'Password too weak',

    // Database errors
    DUPLICATE_ENTRY: 'Duplicate entry',
    FOREIGN_KEY_VIOLATION: 'Foreign key constraint violation',

    // Server errors
    INTERNAL_SERVER_ERROR: 'Internal server error',
    SERVICE_UNAVAILABLE: 'Service unavailable'
};

// Success messages
export const SUCCESS_MESSAGES = {
    USER_CREATED: 'User created successfully',
    USER_LOGIN: 'Login successful',
    USER_LOGOUT: 'Logout successful',
    OTP_SENT: 'OTP sent successfully',
    OTP_VERIFIED: 'OTP verified successfully',
    PASSWORD_RESET: 'Password reset successfully',
    EMAIL_VERIFIED: 'Email verified successfully',
    ORDER_CREATED: 'Order created successfully',
    ORDER_UPDATED: 'Order updated successfully',
    ORDER_CANCELLED: 'Order cancelled successfully',
    DELETED: 'Deleted successfully',
    UPDATED: 'Updated successfully',
    CREATED: 'Created successfully'
};

// API response messages
export const API_MESSAGES = {
    HEALTH_CHECK: 'API is healthy',
    VERSION: '1.0.0',
    ROUTE_NOT_FOUND: 'Route not found'
};