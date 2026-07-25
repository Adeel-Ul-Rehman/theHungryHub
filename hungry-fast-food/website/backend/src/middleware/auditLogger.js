import pool from '../config/database.js';

export const auditLogger = (action) => {
    return async (req, res, next) => {
        // Save the original send function to intercept the response
        const originalSend = res.send;
        
        res.send = function (data) {
            // Restore original send
            res.send = originalSend;
            
            // Only log successful modifications
            if (res.statusCode >= 200 && res.statusCode < 300) {
                const adminId = req.user ? req.user.id : null;
                const ipAddress = req.ip || req.headers['x-forwarded-for'] || 'unknown';
                const targetUrl = req.originalUrl;
                
                // Don't await, let it run in background
                pool.query(
                    'INSERT INTO audit_logs (admin_id, action, target, details, ip_address) VALUES ($1, $2, $3, $4, $5)',
                    [adminId, action, targetUrl, JSON.stringify(req.body || {}), ipAddress]
                ).catch(err => console.error('Audit log failed:', err));
            }
            
            // Send the response
            return res.send(data);
        };
        
        next();
    };
};
