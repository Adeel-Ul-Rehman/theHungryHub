import DOMPurify from 'dompurify';
import { JSDOM } from 'jsdom';

const window = new JSDOM('').window;
const purify = DOMPurify(window);

// Recursively sanitize an object or array
const sanitizeObject = (obj) => {
    if (typeof obj === 'string') {
        return purify.sanitize(obj);
    }
    
    if (Array.isArray(obj)) {
        return obj.map(item => sanitizeObject(item));
    }
    
    if (obj !== null && typeof obj === 'object') {
        const sanitized = {};
        for (const key in obj) {
            sanitized[key] = sanitizeObject(obj[key]);
        }
        return sanitized;
    }
    
    return obj;
};

// Express middleware
export const xssSanitizer = (req, res, next) => {
    if (req.body) req.body = sanitizeObject(req.body);
    
    if (req.query) {
        for (const key in req.query) {
            req.query[key] = sanitizeObject(req.query[key]);
        }
    }
    
    if (req.params) {
        for (const key in req.params) {
            req.params[key] = sanitizeObject(req.params[key]);
        }
    }
    
    next();
};
