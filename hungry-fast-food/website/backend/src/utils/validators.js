// E:\hungryHub\hungry-fast-food\website\backend\src\utils\validators.js

import bcrypt from 'bcrypt';
import crypto from 'crypto';
import dotenv from 'dotenv';

dotenv.config();

// ============================================
// PASSWORD HELPERS
// ============================================

// Hash password
export const hashPassword = async (password) => {
    const saltRounds = parseInt(process.env.BCRYPT_SALT_ROUNDS) || 12;
    return bcrypt.hash(password, saltRounds);
};

// Compare password
export const comparePassword = async (password, hash) => {
    return bcrypt.compare(password, hash);
};

// ============================================
// OTP HELPERS
// ============================================

// Generate 6-digit OTP
export const generateOTP = () => {
    return crypto.randomInt(100000, 999999).toString();
};

// ============================================
// ORDER NUMBER HELPERS
// ============================================

// Generate order number
export const generateOrderNumber = (type) => {
    const prefix = type === 'dining' ? 'D' : type === 'delivery' ? 'DL' : 'TA';
    const date = new Date();
    const year = date.getFullYear().toString().slice(-2);
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const random = String(crypto.randomInt(1000, 9999));

    return `${prefix}${year}${month}${day}-${random}`;
};

// ============================================
// DISTANCE HELPERS
// ============================================

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

// ============================================
// DELIVERY ZONE HELPERS
// ============================================

// Cached delivery zones
let _cachedZones = null;
let _lastZoneFetch = 0;
const ZONE_CACHE_TTL = 120000; // 2 minute cache — frequent enough for admin changes

/**
 * Load delivery zones from database using the existing query helper.
 * This is called only when zone data isn't cached.
 */
async function loadDeliveryZones() {
    try {
        // Dynamic import to avoid circular dependency at module load time
        const { query } = await import('../config/database.js');
        
        // Try the primary key first
        const result = await query(
            `SELECT setting_value FROM system_settings WHERE setting_key = 'delivery_zones'`
        );
        if (result.rows.length > 0 && result.rows[0].setting_value) {
            const val = result.rows[0].setting_value;
            if (val && val !== 'null' && val !== 'undefined') {
                const parsed = JSON.parse(val);
                if (Array.isArray(parsed) && parsed.length > 0) {
                    return parsed;
                }
            }
        }
        
        // Fallback to hardcoded defaults
        console.log('⚠️ No delivery zones found in DB, using defaults');
        return getDefaultZones();
    } catch (dbError) {
        console.warn('⚠️ Failed to load delivery zones from DB:', dbError.message);
        return getDefaultZones();
    }
}

/**
 * Returns reasonable default zones if nothing is configured
 */
function getDefaultZones() {
    return [
        { name: "Free Delivery Zone", maxDistance: 10, charge: 0, minOrder: 0 },
        { name: "Charged Delivery Zone", maxDistance: 25, charge: 200, minOrder: 0 }
    ];
}

/**
 * Get zones with caching
 */
export async function getDeliveryZones() {
    const now = Date.now();
    if (_cachedZones && (now - _lastZoneFetch) < ZONE_CACHE_TTL) {
        return _cachedZones;
    }
    
    try {
        _cachedZones = await loadDeliveryZones();
        _lastZoneFetch = now;
    } catch (err) {
        console.error('Zone load error:', err);
        _cachedZones = getDefaultZones();
    }
    return _cachedZones;
}

/**
 * Force zone cache refresh (exported for admin API to call)
 */
export function clearDeliveryZoneCache() {
    _cachedZones = null;
    _lastZoneFetch = 0;
}

/**
 * Check if delivery is available for a given distance.
 * Matches the admin-defined zones dynamically.
 */
export async function checkDeliveryZone(distance) {
    const zones = await getDeliveryZones();

    // Normalize zone property names (accept both camelCase and PascalCase from C#)
    const normalizedZones = zones.map(z => ({
        name: z.name || z.Name || 'Unknown Zone',
        maxDistance: parseFloat(z.maxDistance || z.MaxDistance || 0),
        charge: parseFloat(z.charge || z.Charge || 0),
        minOrder: parseFloat(z.minOrder || z.MinOrder || 0)
    }));

    // Sort by maxDistance ascending
    normalizedZones.sort((a, b) => a.maxDistance - b.maxDistance);

    // Find applicable zone
    for (const zone of normalizedZones) {
        if (distance <= zone.maxDistance) {
            const chargeMsg = zone.charge > 0
                ? `Delivery fee: ${zone.charge} PKR applies`
                : 'Free delivery';
            const minOrderMsg = zone.minOrder > 0
                ? ` Minimum order for this zone: ${zone.minOrder} PKR.`
                : '';
            return {
                allowed: true,
                charge: zone.charge,
                maxDistance: zone.maxDistance,
                minOrder: zone.minOrder,
                zoneName: zone.name,
                message: `✅ We deliver to your location! ${chargeMsg}.${minOrderMsg}`
            };
        }
    }

    // Out of all zones — delivery not available
    const maxDist = normalizedZones.length > 0
        ? normalizedZones[normalizedZones.length - 1].maxDistance
        : 25;

    return {
        allowed: false,
        charge: 0,
        maxDistance: maxDist,
        minOrder: 0,
        message: `⚠️ We cannot deliver to your selected location (${distance.toFixed(1)} km). Maximum delivery distance is ${maxDist} km.`
    };
}

// ============================================
// SLUG HELPERS
// ============================================

export const generateSlug = (text) => {
    return text
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '');
};

// ============================================
// PHONE NUMBER HELPERS
// ============================================

export const formatPhone = (phone) => {
    if (!phone) return phone;
    return phone.replace(/[^0-9+]/g, '');
};

// ============================================
// EMAIL HELPERS
// ============================================

export const isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
};

// ============================================
// GOOGLE MAPS LINK GENERATION
// ============================================

export function generateGoogleMapsLink(address, latitude, longitude) {
    if (latitude && longitude) {
        return `https://www.google.com/maps/search/?api=1&query=${latitude},${longitude}`;
    }
    if (address) {
        return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`;
    }
    return null;
}

// ============================================
// STRING HELPERS
// ============================================

export const truncate = (text, length = 50) => {
    if (!text || text.length <= length) return text;
    return text.substring(0, length) + '...';
};

export const capitalize = (text) => {
    if (!text) return text;
    return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();
};