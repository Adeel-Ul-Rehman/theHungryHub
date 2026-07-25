// E:\hungryHub\hungry-fast-food\website\backend\src\services\cloudinaryService.js

import crypto from 'crypto';
import dotenv from 'dotenv';

dotenv.config();

function sha1(string) {
    return crypto.createHash('sha1').update(string).digest('hex');
}

/**
 * Uploads a file buffer directly to Cloudinary using secure signed requests.
 * @param {Buffer} imageBuffer - The image data buffer
 * @param {string} [folder] - Target folder path in Cloudinary
 * @param {string} [publicId] - Optional custom public ID for the resource
 * @returns {Promise<{ secure_url: string, public_id: string }>} Secure URL and public ID
 */
export const uploadImage = async (imageBuffer, folder = '', publicId = '') => {
    try {
        const blob = new Blob([imageBuffer], { type: 'image/jpeg' });
        
        const timestamp = Math.round(new Date().getTime() / 1000).toString();
        const uploadPreset = process.env.CLOUDINARY_UPLOAD_PRESET || 'ml_default';
        const apiKey = process.env.CLOUDINARY_API_KEY || '461368631868661';
        const apiSecret = process.env.CLOUDINARY_API_SECRET || 'PA97TqEm3kmuOVKgNlvrQrzTEu0';
        const cloudName = process.env.CLOUDINARY_CLOUD_NAME || 'ourl0wez';
        
        // Sort parameters alphabetically to sign
        const params = {
            timestamp,
            upload_preset: uploadPreset
        };
        if (folder) params.folder = folder;
        if (publicId) params.public_id = publicId;

        const sortedKeys = Object.keys(params).sort();
        const stringToSign = sortedKeys.map(key => `${key}=${params[key]}`).join('&') + apiSecret;
        const signature = sha1(stringToSign);
        
        const formData = new FormData();
        formData.append('file', blob, 'image.jpg');
        formData.append('api_key', apiKey);
        formData.append('signature', signature);
        for (const key of Object.keys(params)) {
            formData.append(key, params[key]);
        }
        
        const url = `https://api.cloudinary.com/v1_1/${cloudName}/image/upload`;
        
        const response = await fetch(url, {
            method: 'POST',
            body: formData
        });
        
        const json = await response.json();
        if (response.ok) {
            return {
                secure_url: json.secure_url,
                public_id: json.public_id
            };
        } else {
            console.error('❌ Cloudinary Upload Error:', json);
            throw new Error(json.error?.message || 'Cloudinary upload failed');
        }
    } catch (error) {
        console.error('❌ Cloudinary Service Exception:', error);
        throw error;
    }
};

/**
 * Deletes an image from Cloudinary using secure signed request.
 * @param {string} publicId - The public ID of the resource to delete
 * @returns {Promise<object>} Result of the deletion
 */
export const deleteImage = async (publicId) => {
    try {
        const timestamp = Math.round(new Date().getTime() / 1000).toString();
        const apiKey = process.env.CLOUDINARY_API_KEY || '461368631868661';
        const apiSecret = process.env.CLOUDINARY_API_SECRET || 'PA97TqEm3kmuOVKgNlvrQrzTEu0';
        const cloudName = process.env.CLOUDINARY_CLOUD_NAME || 'ourl0wez';

        const stringToSign = `public_id=${publicId}&timestamp=${timestamp}${apiSecret}`;
        const signature = sha1(stringToSign);

        const formData = new FormData();
        formData.append('public_id', publicId);
        formData.append('timestamp', timestamp);
        formData.append('api_key', apiKey);
        formData.append('signature', signature);

        const url = `https://api.cloudinary.com/v1_1/${cloudName}/image/destroy`;
        const response = await fetch(url, {
            method: 'POST',
            body: formData
        });

        const json = await response.json();
        if (response.ok) {
            return json;
        } else {
            console.error('❌ Cloudinary Delete Error:', json);
            throw new Error(json.error?.message || 'Cloudinary delete failed');
        }
    } catch (error) {
        console.error('❌ Cloudinary Delete Exception:', error);
        throw error;
    }
};

/**
 * Generates delivery URL with custom transformations.
 * @param {string} publicId - The public ID of the resource
 * @param {object} [options] - Transformations config
 * @returns {string} Fully qualified delivery URL
 */
export const getImageUrl = (publicId, options = {}) => {
    const cloudName = process.env.CLOUDINARY_CLOUD_NAME || 'ourl0wez';
    const parts = [];
    if (options.width) parts.push(`w_${options.width}`);
    if (options.height) parts.push(`h_${options.height}`);
    if (options.crop) parts.push(`c_${options.crop}`);
    if (options.quality) parts.push(`q_${options.quality}`);
    if (options.fetchFormat) parts.push(`f_${options.fetchFormat}`);
    
    const transformSegment = parts.length > 0 ? parts.join(',') + '/' : '';
    return `https://res.cloudinary.com/${cloudName}/image/upload/${transformSegment}${publicId}`;
};
