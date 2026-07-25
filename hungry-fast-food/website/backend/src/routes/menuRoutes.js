// E:\hungryHub\hungry-fast-food\website\backend\src\routes\menuRoutes.js

import express from 'express';
import {
    getCategories,
    getCategoryById,
    getProducts,
    getProductById,
    getDeals,
    getFeaturedDeal,
    getDealById,
    getSystemSettings
} from '../controllers/menuController.js';

const router = express.Router();

// Public routes
router.get('/settings', getSystemSettings);
router.get('/categories', getCategories);
router.get('/categories/:id', getCategoryById);
router.get('/products', getProducts);
router.get('/products/:id', getProductById);
router.get('/deals', getDeals);
router.get('/deals/featured', getFeaturedDeal);
router.get('/deals/:id', getDealById);

export default router;