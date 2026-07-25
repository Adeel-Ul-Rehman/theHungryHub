// E:\hungryHub\hungry-fast-food\website\backend\src\controllers\menuController.js

import Category from '../models/Category.js';
import Product from '../models/Product.js';
import Deal from '../models/Deal.js';
import { emitSocketEvent } from '../../services/socketService.js';

// ============================================
// CATEGORY CONTROLLERS
// ============================================

// Get all categories
export const getCategories = async (req, res) => {
    try {
        const includeInactive = req.query.include_inactive === 'true';
        const categories = await Category.getAll(includeInactive);

        res.set('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
        res.status(200).json({
            success: true,
            data: categories
        });
    } catch (error) {
        console.error('Get categories error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get categories',
            error: error.message
        });
    }
};

// Get category by ID
export const getCategoryById = async (req, res) => {
    try {
        const { id } = req.params;
        const category = await Category.findById(id);

        if (!category) {
            return res.status(404).json({
                success: false,
                message: 'Category not found'
            });
        }

        res.status(200).json({
            success: true,
            data: category
        });
    } catch (error) {
        console.error('Get category error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get category',
            error: error.message
        });
    }
};

// Create category (Admin only)
export const createCategory = async (req, res) => {
    try {
        if (req.body.name) {
            const existing = await Category.findByName(req.body.name);
            if (existing) {
                return res.status(400).json({
                    success: false,
                    message: 'A category with this name already exists'
                });
            }
        }

        const category = await Category.create(req.body);

        emitSocketEvent('category_added', {
            id: category.id,
            name: category.name,
            slug: category.slug,
            display_order: category.display_order,
            is_active: category.is_active
        });

        res.status(201).json({
            success: true,
            message: 'Category created successfully',
            data: category
        });
    } catch (error) {
        console.error('Create category error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to create category',
            error: error.message
        });
    }
};

// Update category (Admin only)
export const updateCategory = async (req, res) => {
    try {
        const { id } = req.params;

        if (req.body.name) {
            const existing = await Category.findByName(req.body.name, id);
            if (existing) {
                return res.status(400).json({
                    success: false,
                    message: 'A category with this name already exists'
                });
            }
        }

        if (req.body.is_active === false || req.body.is_active === 'false') {
            const activeProducts = await Category.countActiveProducts(id);
            if (activeProducts > 0) {
                return res.status(400).json({
                    success: false,
                    message: 'Cannot mark category as inactive because it contains active products.'
                });
            }
        }

        const category = await Category.update(id, req.body);

        if (!category) {
            return res.status(404).json({
                success: false,
                message: 'Category not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Category updated successfully',
            data: category
        });
    } catch (error) {
        console.error('Update category error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update category',
            error: error.message
        });
    }
};

// Delete category (Admin only)
export const deleteCategory = async (req, res) => {
    try {
        const { id } = req.params;

        const productCount = await Category.countProducts(id);
        if (productCount > 0) {
            return res.status(400).json({
                success: false,
                message: 'Cannot delete category because it contains products.'
            });
        }

        const category = await Category.delete(id);

        if (!category) {
            return res.status(404).json({
                success: false,
                message: 'Category not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Category deleted successfully'
        });
    } catch (error) {
        console.error('Delete category error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to delete category',
            error: error.message
        });
    }
};

// ============================================
// PRODUCT CONTROLLERS
// ============================================

// Get all products
export const getProducts = async (req, res) => {
    try {
        const {
            category_id,
            is_active,
            is_deal,
            search,
            limit = 50,
            offset = 0
        } = req.query;

        const products = await Product.getProducts({
            category_id,
            is_active: is_active === 'all' ? undefined : (is_active !== undefined ? is_active === 'true' : true),
            is_deal: is_deal !== undefined ? is_deal === 'true' : undefined,
            search,
            limit: parseInt(limit),
            offset: parseInt(offset)
        });

        res.set('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
        res.status(200).json({
            success: true,
            data: products,
            pagination: {
                limit: parseInt(limit),
                offset: parseInt(offset)
            }
        });
    } catch (error) {
        console.error('Get products error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get products',
            error: error.message
        });
    }
};

// Get product by ID
export const getProductById = async (req, res) => {
    try {
        const { id } = req.params;
        const product = await Product.findById(id);

        if (!product) {
            return res.status(404).json({
                success: false,
                message: 'Product not found'
            });
        }

        res.status(200).json({
            success: true,
            data: product
        });
    } catch (error) {
        console.error('Get product error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get product',
            error: error.message
        });
    }
};

// Create product (Admin only)
export const createProduct = async (req, res) => {
    try {
        if (req.body.name) {
            const existing = await Product.findByName(req.body.name);
            if (existing) {
                return res.status(400).json({
                    success: false,
                    message: 'A product with this name already exists'
                });
            }
        }

        const { variations, ...productData } = req.body;

        const product = await Product.create(productData);

        // Add variations if provided
        if (variations && variations.length > 0) {
            for (const variation of variations) {
                await Product.addVariation(product.id, variation);
            }
        }

        emitSocketEvent('product_added', {
            id: product.id,
            name: product.name,
            category_id: product.category_id,
            price: product.price,
            image_url: product.image_url,
            is_active: product.is_active
        });

        res.status(201).json({
            success: true,
            message: 'Product created successfully',
            data: product
        });
    } catch (error) {
        console.error('Create product error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to create product',
            error: error.message
        });
    }
};

// Update product (Admin only)
export const updateProduct = async (req, res) => {
    try {
        const { id } = req.params;

        if (req.body.name) {
            const existing = await Product.findByName(req.body.name, id);
            if (existing) {
                return res.status(400).json({
                    success: false,
                    message: 'A product with this name already exists'
                });
            }
        }

        const product = await Product.update(id, req.body);

        if (!product) {
            return res.status(404).json({
                success: false,
                message: 'Product not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Product updated successfully',
            data: product
        });
    } catch (error) {
        console.error('Update product error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update product',
            error: error.message
        });
    }
};

// Delete product (Admin only)
export const deleteProduct = async (req, res) => {
    try {
        const { id } = req.params;
        const product = await Product.delete(id);

        if (!product) {
            return res.status(404).json({
                success: false,
                message: 'Product not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Product deleted successfully'
        });
    } catch (error) {
        console.error('Delete product error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to delete product',
            error: error.message
        });
    }
};

// ============================================
// PRODUCT VARIATION CONTROLLERS
// ============================================

// Add variation to product
export const addVariation = async (req, res) => {
    try {
        const { productId } = req.params;
        const variation = await Product.addVariation(productId, req.body);

        res.status(201).json({
            success: true,
            message: 'Variation added successfully',
            data: variation
        });
    } catch (error) {
        console.error('Add variation error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to add variation',
            error: error.message
        });
    }
};

// Remove variation
export const removeVariation = async (req, res) => {
    try {
        const { variationId } = req.params;
        await Product.removeVariation(variationId);

        res.status(200).json({
            success: true,
            message: 'Variation removed successfully'
        });
    } catch (error) {
        console.error('Remove variation error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to remove variation',
            error: error.message
        });
    }
};

// ============================================
// DEAL CONTROLLERS
// ============================================

// Get all deals
export const getDeals = async (req, res) => {
    try {
        const { is_active, is_featured } = req.query;

        const deals = await Deal.getDeals({
            is_active: is_active !== undefined ? is_active === 'true' : undefined,
            is_featured: is_featured !== undefined ? is_featured === 'true' : undefined
        });

        res.set('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
        res.status(200).json({
            success: true,
            data: deals
        });
    } catch (error) {
        console.error('Get deals error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get deals',
            error: error.message
        });
    }
};

// Get featured deal
export const getFeaturedDeal = async (req, res) => {
    try {
        const deal = await Deal.getFeatured();

        if (!deal) {
            return res.status(200).json({
                success: true,
                data: null,
                message: 'No featured deal available'
            });
        }

        res.status(200).json({
            success: true,
            data: deal
        });
    } catch (error) {
        console.error('Get featured deal error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get featured deal',
            error: error.message
        });
    }
};

// Get deal by ID
export const getDealById = async (req, res) => {
    try {
        const { id } = req.params;
        const deal = await Deal.findById(id);

        if (!deal) {
            return res.status(404).json({
                success: false,
                message: 'Deal not found'
            });
        }

        res.status(200).json({
            success: true,
            data: deal
        });
    } catch (error) {
        console.error('Get deal error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get deal',
            error: error.message
        });
    }
};

// Create deal (Admin only)
export const createDeal = async (req, res) => {
    try {
        const { items, ...dealData } = req.body;

        const deal = await Deal.create(dealData, items);

        res.status(201).json({
            success: true,
            message: 'Deal created successfully',
            data: deal
        });
    } catch (error) {
        console.error('Create deal error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to create deal',
            error: error.message
        });
    }
};

// Update deal (Admin only)
export const updateDeal = async (req, res) => {
    try {
        const { id } = req.params;
        const deal = await Deal.update(id, req.body);

        if (!deal) {
            return res.status(404).json({
                success: false,
                message: 'Deal not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Deal updated successfully',
            data: deal
        });
    } catch (error) {
        console.error('Update deal error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update deal',
            error: error.message
        });
    }
};

// Update deal items (Admin only)
export const updateDealItems = async (req, res) => {
    try {
        const { id } = req.params;
        const { items } = req.body;

        const deal = await Deal.updateItems(id, items);

        if (!deal) {
            return res.status(404).json({
                success: false,
                message: 'Deal not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Deal items updated successfully',
            data: deal
        });
    } catch (error) {
        console.error('Update deal items error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to update deal items',
            error: error.message
        });
    }
};

// Delete deal (Admin only)
export const deleteDeal = async (req, res) => {
    try {
        const { id } = req.params;
        const deal = await Deal.delete(id);

        if (!deal) {
            return res.status(404).json({
                success: false,
                message: 'Deal not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Deal deleted successfully'
        });
    } catch (error) {
        console.error('Delete deal error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to delete deal',
            error: error.message
        });
    }
};
// ============================================
// SYSTEM SETTINGS
// ============================================
export const getSystemSettings = async (req, res) => {
    try {
        const { query } = await import('../config/database.js');
        const result = await query('SELECT setting_key, setting_value FROM system_settings');
        
        const settings = {};
        result.rows.forEach(row => {
            settings[row.setting_key] = row.setting_value;
        });

        const isManualOpen = String(settings.accept_website_orders).toLowerCase() !== 'false'; // default true
        const useAutoTiming = String(settings.use_auto_timing).toLowerCase() === 'true';
        const openingTime = settings.opening_time || '10:00';
        const closingTime = settings.closing_time || '23:00';
        
        let is_currently_open = true;
        if (!useAutoTiming) {
            is_currently_open = isManualOpen;
        } else {
            const tzDate = new Date(new Date().toLocaleString('en-US', { timeZone: 'Asia/Karachi' }));
            const currentTotalMins = tzDate.getHours() * 60 + tzDate.getMinutes();
            
            const [openH, openM] = openingTime.split(':').map(Number);
            const [closeH, closeM] = closingTime.split(':').map(Number);
            
            const openTotalMins = openH * 60 + openM;
            const closeTotalMins = closeH * 60 + closeM;
            
            if (closeTotalMins > openTotalMins) {
                is_currently_open = currentTotalMins >= openTotalMins && currentTotalMins <= closeTotalMins;
            } else {
                // Crosses midnight
                is_currently_open = currentTotalMins >= openTotalMins || currentTotalMins <= closeTotalMins;
            }
        }

        res.status(200).json({
            success: true,
            data: {
                tax_rate: parseFloat(settings.tax_rate) || 0,
                baking_duration_minutes: parseInt(settings.baking_duration_minutes) || 15,
                delivery_duration_minutes: parseInt(settings.delivery_duration_minutes) || 20,
                min_order: parseFloat(settings.min_order) || 500,
                is_currently_open,
                opening_time: openingTime,
                closing_time: closingTime,
                closed_message: `The restaurant is closed right now. You can place your order after ${openingTime}`
            }
        });
    } catch (error) {
        console.error('Get system settings error:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get system settings',
            error: error.message
        });
    }
};

