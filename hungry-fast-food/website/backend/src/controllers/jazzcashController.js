import crypto from 'crypto';
import pool from '../config/database.js';

// Compute pp_SecureHash
export function calculateSecureHash(params, integritySalt) {
    // Sort keys alphabetically
    const sortedKeys = Object.keys(params)
        .filter(key => key !== 'pp_SecureHash' && params[key] !== null && params[key] !== undefined && params[key] !== '')
        .sort();

    // Create the signature string
    let signatureString = integritySalt;
    for (const key of sortedKeys) {
        signatureString += `&${params[key]}`;
    }

    // Hash using HMAC-SHA256
    return crypto
        .createHmac('sha256', integritySalt)
        .update(signatureString)
        .digest('hex')
        .toUpperCase();
}

export const initiateJazzCashPayment = async (req, res) => {
    const { orderId } = req.body;
    try {
        if (!orderId) {
            return res.status(400).json({ success: false, message: 'Order ID is required' });
        }

        // Fetch order details
        const orderResult = await pool.query('SELECT * FROM orders WHERE id = $1', [orderId]);
        if (orderResult.rows.length === 0) {
            return res.status(404).json({ success: false, message: 'Order not found' });
        }
        const order = orderResult.rows[0];

        // Format amount in Paise (Rupees * 100)
        const amountPaise = Math.round(parseFloat(order.total) * 100);

        // Get credentials
        const merchantId = process.env.JAZZCASH_MERCHANT_ID || 'MC12345';
        const password = process.env.JAZZCASH_PASSWORD || 'pass123';
        const integritySalt = process.env.JAZZCASH_INTEGRITY_SALT || 'salt123';
        const returnUrl = process.env.JAZZCASH_RETURN_URL || `${req.protocol}://${req.get('host')}/api/orders/jazzcash/callback`;

        // Format date/times
        const now = new Date();
        const expiry = new Date(now.getTime() + 60 * 60 * 1000); // 1 hour expiry

        const formatDate = (date) => {
            const pad = (n) => n.toString().padStart(2, '0');
            return `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}${pad(date.getHours())}${pad(date.getMinutes())}${pad(date.getSeconds())}`;
        };

        const txnRefNo = `T${formatDate(now)}`;

        // Transaction parameters
        const params = {
            pp_Version: '1.1',
            pp_TxnType: '', // Empty for hosted checkout portal selection
            pp_Language: 'EN',
            pp_MerchantID: merchantId,
            pp_SubMerchantID: '',
            pp_Password: password,
            pp_TxnRefNo: txnRefNo,
            pp_Amount: amountPaise.toString(),
            pp_TxnCurrency: 'PKR',
            pp_TxnDateTime: formatDate(now),
            pp_BillReference: orderId.toString(),
            pp_Description: `Payment for Order #${order.order_number || order.id}`,
            pp_TxnExpiryDateTime: formatDate(expiry),
            pp_ReturnURL: returnUrl,
            pp_MPay: '',
            pp_SecureHash: ''
        };

        // Calculate secure hash
        params.pp_SecureHash = calculateSecureHash(params, integritySalt);

        // Determine JazzCash Portal Url
        const isLive = process.env.JAZZCASH_ENVIRONMENT === 'live';
        const postUrl = isLive
            ? 'https://transaction.jazzcash.com.pk/CustomerPortal/transactionmanagement/merchantcardpayment'
            : 'https://sandbox.jazzcash.com.pk/CustomerPortal/transactionmanagement/merchantcardpayment';

        res.status(200).json({
            success: true,
            postUrl,
            params
        });

    } catch (error) {
        console.error('❌ [JAZZCASH] Initiate payment error:', error);
        res.status(500).json({ success: false, message: 'Internal server error' });
    }
};

export const handleJazzCashCallback = async (req, res) => {
    console.log('📬 [JAZZCASH] Callback received:', req.body);
    const params = req.body;
    const integritySalt = process.env.JAZZCASH_INTEGRITY_SALT || 'salt123';

    try {
        const orderId = params.pp_BillReference;
        const responseCode = params.pp_ResponseCode;
        const responseMsg = params.pp_ResponseMessage || 'Payment failed';

        // 1. Verify Secure Hash
        const calculatedHash = calculateSecureHash(params, integritySalt);
        if (calculatedHash !== params.pp_SecureHash) {
            console.error('❌ [JAZZCASH] Signature mismatch! Security Alert.');
            return res.redirect(`${process.env.FRONTEND_URL || 'https://www.thehungryhub.shop'}/order-failed?reason=security_mismatch`);
        }

        // 2. Update DB depending on response code
        if (responseCode === '000') {
            console.log(`✅ [JAZZCASH] Payment succeeded for order ID: ${orderId}`);
            
            // Mark payment as completed and status as pending (waiting admin confirmation)
            await pool.query(
                `UPDATE orders SET payment_status = $1, status = $2, updated_at = CURRENT_TIMESTAMP WHERE id = $3`,
                ['completed', 'pending', orderId]
            );

            res.redirect(`${process.env.FRONTEND_URL || 'https://www.thehungryhub.shop'}/order-success?id=${orderId}`);
        } else {
            console.warn(`❌ [JAZZCASH] Payment failed (code: ${responseCode}): ${responseMsg}`);
            
            await pool.query(
                `UPDATE orders SET payment_status = $1, updated_at = CURRENT_TIMESTAMP WHERE id = $2`,
                ['failed', orderId]
            );

            res.redirect(`${process.env.FRONTEND_URL || 'https://www.thehungryhub.shop'}/order-failed?reason=${encodeURIComponent(responseMsg)}&id=${orderId}`);
        }

    } catch (error) {
        console.error('❌ [JAZZCASH] Callback handler error:', error);
        res.redirect(`${process.env.FRONTEND_URL || 'https://www.thehungryhub.shop'}/order-failed?reason=internal_error`);
    }
};
