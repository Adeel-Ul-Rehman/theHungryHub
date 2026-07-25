// E:\hungryHub\hungry-fast-food\website\backend\src\routes\orderRoutes.js

import express from 'express';
import {
    createOrder,
    getOrderById,
    getOrderByNumber,
    getUserOrders,
    checkDelivery
} from '../controllers/orderController.js';
import { verifyToken, optionalAuth } from '../middleware/auth.js';
import { validate } from '../middleware/validation.js';
import { schemas } from '../middleware/validation.js';
import { orderLimiter } from '../middleware/rateLimiter.js';

const router = express.Router();

// Public routes
router.get('/check-delivery', checkDelivery);
router.post('/', optionalAuth, orderLimiter, validate(schemas.createOrder), createOrder);
router.get('/track/:orderNumber', getOrderByNumber);

// Protected routes (user)
router.get('/my-orders', verifyToken, getUserOrders);
router.get('/:id', optionalAuth, getOrderById);

export default router;