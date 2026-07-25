import Joi from 'joi';

// User Registration Schema
export const registerSchema = Joi.object({
    email: Joi.string().email().required().messages({
        'string.email': 'Please provide a valid email address.',
        'any.required': 'Email is required.'
    }),
    password: Joi.string().min(8).required().messages({
        'string.min': 'Password must be at least 8 characters long.',
        'any.required': 'Password is required.'
    }),
    full_name: Joi.string().min(2).max(100).required().messages({
        'string.min': 'Full name must be at least 2 characters.',
        'string.max': 'Full name cannot exceed 100 characters.',
        'any.required': 'Full name is required.'
    }),
    phone: Joi.string().pattern(/^[0-9+ ]{10,15}$/).optional().messages({
        'string.pattern.base': 'Phone number must be between 10-15 digits and can include a plus sign.'
    })
});

// User Login Schema
export const loginSchema = Joi.object({
    email: Joi.string().email().required(),
    password: Joi.string().required()
});

// Order Placement Schema (Simplified)
export const orderSchema = Joi.object({
    items: Joi.array().min(1).required(),
    delivery_type: Joi.string().valid('delivery', 'takeaway', 'dining').required(),
    total_amount: Joi.number().min(0).required(),
    delivery_fee: Joi.number().min(0).optional(),
    tax_amount: Joi.number().min(0).optional(),
    latitude: Joi.number().optional().allow(null),
    longitude: Joi.number().optional().allow(null),
    address: Joi.string().optional().allow('', null)
}).unknown(true); // Allow other fields for now to prevent breaking existing code
