// E:\hungryHub\hungry-fast-food\website\backend\src\routes\adminRoutes.js

import express from 'express';
import { 
    pushSyncItems, 
    pullSyncOrders,
    syncCategory,
    updateCategorySync,
    deleteCategorySync,
    syncProduct,
    updateProductSync,
    deleteProductSync,
    updateProductAvailabilitySync,
    getNewOrdersSync,
    updateOrderStatusSync,
    syncOrder,
    uploadImageSync,
    fullSync,
    syncStatus
} from '../controllers/syncController.js';
import {
    getDashboardStats,
    getAdminProfile,
    updateAdminProfile,
    changeAdminPassword,
    getUsers,
    getSettings,
    updateSetting,
    getDeliveryZones,
    updateDeliveryZones,
    getSuspiciousOrders,
    getAdminLogs,
    syncCategory as syncCategoryAdmin,
    syncProduct as syncProductAdmin,
    updateProductStatus as updateProductStatusAdmin,
    getNewOrders as getNewOrdersAdmin,
    syncOrder as syncOrderAdmin,
    deleteCategory as deleteCategoryAdmin,
    deleteProduct as deleteProductAdmin
} from '../controllers/adminController.js';
import {
    createCategory,
    updateCategory,
    deleteCategory,
    createProduct,
    updateProduct,
    deleteProduct,
    addVariation,
    removeVariation,
    createDeal,
    updateDeal,
    updateDealItems,
    deleteDeal
} from '../controllers/menuController.js';
import {
    getOrders,
    getOrderById,
    updateOrderStatus,
    cancelOrder,
    markSuspicious,
    getOrderStats,
    getDailyReport
} from '../controllers/orderController.js';
import { verifyAdmin, verifyAdminApiKey } from '../middleware/auth.js';
import { validate } from '../middleware/validation.js';
import { schemas } from '../middleware/validation.js';
import { adminLimiter } from '../middleware/rateLimiter.js';
import { auditLogger } from '../middleware/auditLogger.js';
import { verifyAdminPassword } from '../controllers/adminController.js';

const router = express.Router();

// ── Sync routes: API-key only (machine-to-machine, no user JWT needed) ────────
router.post('/sync/push', verifyAdminApiKey, pushSyncItems);
router.post('/sync/full', verifyAdminApiKey, fullSync);
router.get('/sync/pull', verifyAdminApiKey, pullSyncOrders);
router.get('/sync/status', verifyAdminApiKey, syncStatus);

// Categories Sync Endpoints
router.post('/categories/sync', verifyAdminApiKey, syncCategoryAdmin);
router.put('/categories/:id', verifyAdminApiKey, updateCategorySync);
router.delete('/categories/:id', verifyAdminApiKey, deleteCategoryAdmin);

// Products Sync Endpoints
router.post('/products/sync', verifyAdminApiKey, syncProductAdmin);
router.put('/products/:id', verifyAdminApiKey, updateProductSync);
router.delete('/products/:id', verifyAdminApiKey, deleteProductAdmin);
router.patch('/products/:id/status', verifyAdminApiKey, updateProductStatusAdmin);

// Orders Sync Endpoints
router.get('/orders/new', verifyAdminApiKey, getNewOrdersAdmin);
router.patch('/orders/:id/status', verifyAdminApiKey, updateOrderStatusSync);
router.post('/orders/sync', verifyAdminApiKey, syncOrderAdmin);

// Image Upload Endpoint (Base64)
router.post('/upload/image', verifyAdminApiKey, uploadImageSync);

// Apply full admin authentication (API key + JWT) to all other routes
router.use(verifyAdminApiKey);
router.use(verifyAdmin);
router.use(adminLimiter);

// Dashboard
router.get('/dashboard/stats', getDashboardStats);

// Verify admin password
router.post('/verify-password', verifyAdminPassword);

// Admin Profile & Security
router.get('/profile', getAdminProfile);
router.put('/profile', updateAdminProfile);
router.put('/change-password', changeAdminPassword);

// Users
router.get('/users', getUsers);

// Settings
router.get('/settings', getSettings);
router.post('/settings', validate(schemas.updateSettings), updateSetting);

// Delivery Zones
router.get('/delivery-zones', getDeliveryZones);
router.put('/delivery-zones', validate(schemas.deliveryZones), updateDeliveryZones);

// Orders
router.get('/orders', getOrders);
router.get('/orders/:id', getOrderById);
router.patch('/orders/:id/status', validate(schemas.updateOrderStatus), updateOrderStatus);
router.post('/orders/:id/cancel', validate(schemas.cancelOrder), cancelOrder);
router.post('/orders/:id/suspicious', markSuspicious);
router.get('/orders/stats', getOrderStats);
router.get('/orders/report/daily', getDailyReport);

// Suspicious Orders
router.get('/suspicious-orders', getSuspiciousOrders);

// Categories (Admin)
router.post('/categories', validate(schemas.createCategory), createCategory);
router.put('/categories/:id', validate(schemas.createCategory), updateCategory);
router.delete('/categories/:id', deleteCategory);

// Products (Admin)
router.post('/products', validate(schemas.createProduct), createProduct);
router.put('/products/:id', validate(schemas.createProduct), updateProduct);
router.delete('/products/:id', deleteProduct);

// Product Variations (Admin)
router.post('/products/:productId/variations', addVariation);
router.delete('/variations/:variationId', removeVariation);

// Deals (Admin)
router.post('/deals', validate(schemas.createDeal), auditLogger('create_deal'), createDeal);
router.put('/deals/:id', validate(schemas.createDeal), auditLogger('update_deal'), updateDeal);
router.put('/deals/:id/items', auditLogger('update_deal_items'), updateDealItems);
router.delete('/deals/:id', auditLogger('delete_deal'), deleteDeal);

// Admin Logs
router.get('/logs', getAdminLogs);

export default router;