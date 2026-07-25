// E:\hungryHub\hungry-fast-food\website\backend\src\middleware\validation.js

import joi from 'joi';

// Validation schemas
export const schemas = {
    // Auth schemas
    signup: joi.object({
        email: joi.string().email().required(),
        password: joi.string().min(6).required(),
        full_name: joi.string().min(2).max(100).required(),
        phone: joi.string().pattern(/^[0-9+\-\s]{7,20}$/).optional()
    }),

    login: joi.object({
        email: joi.string().email().required(),
        password: joi.string().required()
    }),

    googleLogin: joi.object({
        idToken: joi.string().required()
    }),

    forgotPassword: joi.object({
        email: joi.string().email().required()
    }),

    resetPassword: joi.object({
        email: joi.string().email().required(),
        otp: joi.string().length(6).pattern(/^[0-9]+$/).required(),
        newPassword: joi.string().min(6).required()
    }),

    verifyOTP: joi.object({
        email: joi.string().email().required(),
        otp: joi.string().length(6).pattern(/^[0-9]+$/).required(),
        purpose: joi.string().valid('signup', 'login', 'forgot_password').required()
    }),

    resendOTP: joi.object({
        email: joi.string().email().required(),
        purpose: joi.string().valid('signup', 'login', 'forgot_password').required()
    }),

    // Order schemas
    createOrder: joi.object({
        order_type: joi.string().valid('dining', 'delivery', 'takeaway').required(),
        customer_name: joi.string().min(2).max(100).required(),
        customer_phone: joi.string().pattern(/^[0-9+\-\s]{7,20}$/).optional(),
        customer_email: joi.string().email().optional(),
        delivery_address: joi.string().when('order_type', {
            is: 'delivery',
            then: joi.string().required(),
            otherwise: joi.string().optional()
        }),
        delivery_latitude: joi.number().when('order_type', {
            is: 'delivery',
            then: joi.number().min(-90).max(90).required(),
            otherwise: joi.number().optional()
        }),
        delivery_longitude: joi.number().when('order_type', {
            is: 'delivery',
            then: joi.number().min(-180).max(180).required(),
            otherwise: joi.number().optional()
        }),
        items: joi.array().items(
            joi.object({
                product_id: joi.string().uuid().optional(),
                deal_id: joi.string().uuid().optional(),
                product_name: joi.string().required(),
                variation_id: joi.string().uuid().optional(),
                variation_name: joi.string().optional(),
                quantity: joi.number().integer().min(1).required(),
                unit_price: joi.number().min(0).required()
            })
        ).min(1).required(),
        payment_method: joi.string().valid('jazzcash', 'cod', 'cash').required(),
        admin_notes: joi.string().max(500).optional()
    }),

    updateOrderStatus: joi.object({
        status: joi.string().valid('pending', 'confirmed', 'preparing', 'ready', 'completed', 'cancelled').required(),
        admin_email: joi.string().email().optional()
    }),

    cancelOrder: joi.object({
        reason: joi.string().max(500).optional(),
        admin_email: joi.string().email().required()
    }),

    // Menu schemas
    createCategory: joi.object({
        name: joi.string().min(2).max(100).required(),
        slug: joi.string().min(2).max(100).required(),
        display_order: joi.number().integer().min(0).optional(),
        is_active: joi.boolean().optional()
    }),

    createProduct: joi.object({
        category_id: joi.string().uuid().required(),
        name: joi.string().min(2).max(200).required(),
        slug: joi.string().min(2).max(200).required(),
        description: joi.string().max(1000).optional(),
        base_price: joi.number().min(0).required(),
        discount_price: joi.number().min(0).optional(),
        has_variations: joi.boolean().optional(),
        is_active: joi.boolean().optional(),
        is_deal: joi.boolean().optional(),
        image_url: joi.string().uri().optional(),
        display_order: joi.number().integer().min(0).optional(),
        variations: joi.array().items(
            joi.object({
                variation_type: joi.string().valid('size', 'flavor', 'option').required(),
                variation_name: joi.string().required(),
                price_adjustment: joi.number().default(0),
                is_default: joi.boolean().default(false)
            })
        ).optional()
    }),

    createDeal: joi.object({
        name: joi.string().min(2).max(200).required(),
        slug: joi.string().min(2).max(200).required(),
        description: joi.string().max(1000).optional(),
        total_price: joi.number().min(0).required(),
        discount_price: joi.number().min(0).optional(),
        is_active: joi.boolean().optional(),
        is_featured: joi.boolean().optional(),
        image_url: joi.string().uri().optional(),
        items: joi.array().items(
            joi.object({
                product_id: joi.string().uuid().required(),
                variation_id: joi.string().uuid().optional(),
                quantity: joi.number().integer().min(1).default(1),
                unit_price: joi.number().min(0).optional()
            })
        ).min(1).required()
    }),

    // Admin schemas
    updateSettings: joi.object({
        setting_key: joi.string().required(),
        setting_value: joi.string().required()
    }),

    deliveryZones: joi.object({
        zones: joi.array().items(
            joi.object({
                maxDistance: joi.number().min(0).required(),
                charge: joi.number().min(0).required(),
                minOrder: joi.number().min(0).optional()
            })
        ).required()
    })
};

// Validate middleware
export const validate = (schema) => {
    return (req, res, next) => {
        const { error } = schema.validate(req.body, {
            abortEarly: false,
            stripUnknown: true
        });

        if (error) {
            const errors = error.details.map(detail => ({
                field: detail.path.join('.'),
                message: detail.message
            }));

            return res.status(400).json({
                success: false,
                message: 'Validation failed',
                errors
            });
        }

        next();
    };
};

// Sanitize input (basic)
export const sanitize = (obj) => {
    if (typeof obj !== 'object' || obj === null) return obj;

    const sanitized = {};
    for (const [key, value] of Object.entries(obj)) {
        if (typeof value === 'string') {
            // Remove potential XSS
            sanitized[key] = value
                .replace(/<[^>]*>/g, '') // Remove HTML tags
                .replace(/[&<>"]/g, (match) => {
                    const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' };
                    return map[match];
                });
        } else if (typeof value === 'object' && value !== null) {
            sanitized[key] = sanitize(value);
        } else {
            sanitized[key] = value;
        }
    }
    return sanitized;
};