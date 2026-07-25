// E:\hungryHub\hungry-fast-food\website\frontend\src\components\cart\Checkout.jsx

import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';
import { api } from '../../services/api';
import ToastNotification from '../common/ToastNotification';
import { formatPrice } from '../../utils/helpers';

export default function Checkout() {
    const { cart, cartTotal, clearCart, taxRate, isRestaurantOpen, closedMessage } = useCart();
    const { user } = useAuth();
    const navigate = useNavigate();

    const [orderType, setOrderType] = useState('delivery');
    const [customerName, setCustomerName] = useState('');
    const [customerPhone, setCustomerPhone] = useState('');
    const [customerEmail, setCustomerEmail] = useState('');
    const [deliveryAddress, setDeliveryAddress] = useState('');
    const [latitude, setLatitude] = useState(null);
    const [longitude, setLongitude] = useState(null);
    const [paymentMethod, setPaymentMethod] = useState('cod');
    const [adminNotes, setAdminNotes] = useState('');

    const [deliveryCharge, setDeliveryCharge] = useState(0);
    const [isCheckingDelivery, setIsCheckingDelivery] = useState(false);
    const [deliveryError, setDeliveryError] = useState(null);
    const [isPlacingOrder, setIsPlacingOrder] = useState(false);
    const [isDetectingLocation, setIsDetectingLocation] = useState(false);
    const [locationDetected, setLocationDetected] = useState(false);
    const [toast, setToast] = useState(null);
    const [minOrderRequired, setMinOrderRequired] = useState(0);
    const [zoneMinOrder, setZoneMinOrder] = useState(0);
    const [zoneName, setZoneName] = useState(null);
    const [globalMinOrder, setGlobalMinOrder] = useState(0);
    const [isPaidZone, setIsPaidZone] = useState(false);
    const [deliveryMessage, setDeliveryMessage] = useState(null);


    // Auto-detect location on mount if allowed
    useEffect(() => {
        if (orderType === 'delivery') {
            navigator.geolocation?.getCurrentPosition(
                (pos) => {
                    setLatitude(pos.coords.latitude);
                    setLongitude(pos.coords.longitude);
                    setLocationDetected(true);
                    verifyDeliveryZone(pos.coords.latitude, pos.coords.longitude);
                },
                () => {
                    // silently fail — user can click the button
                }
            );
        }
    }, [orderType]);

    // Check delivery eligibility when coordinates change
    useEffect(() => {
        if (orderType === 'delivery' && latitude && longitude) {
            verifyDeliveryZone(latitude, longitude);
        }
    }, [latitude, longitude, orderType]);

    const verifyDeliveryZone = async (lat, lng) => {
        try {
            setIsCheckingDelivery(true);
            setDeliveryError(null);
            const response = await api.get(`/orders/check-delivery?lat=${lat}&lng=${lng}`);

            if (response.success && response.data) {
                if (response.data.allowed) {
                    setDeliveryCharge(response.data.charge);
                    setMinOrderRequired(response.data.minOrder || 0);
                    setZoneMinOrder(response.data.zoneMinOrder || 0);
                    setGlobalMinOrder(response.data.globalMinOrder || 0);
                    setZoneName(response.data.zoneName || null);
                    setIsPaidZone(response.data.isPaidZone || false);
                    setDeliveryMessage(response.data.message || null);
                    setDeliveryError(null);
                    setToast({ type: 'success', message: `📍 Location verified! ${response.data.message}` });
                } else {
                    setDeliveryCharge(0);
                    setMinOrderRequired(0);
                    setZoneMinOrder(0);
                    setGlobalMinOrder(0);
                    setZoneName(null);
                    setIsPaidZone(false);
                    setDeliveryError(response.data.message);
                    setDeliveryMessage(null);
                    setToast({ type: 'error', message: response.data.message });
                }
            }
        } catch (error) {
            setDeliveryError('Failed to verify delivery zone');
        } finally {
            setIsCheckingDelivery(false);
        }
    };

    const handleDetectLocation = () => {
        if (!navigator.geolocation) {
            setToast({ type: 'error', message: 'Location services are not supported by your browser.' });
            return;
        }

        setIsDetectingLocation(true);
        navigator.geolocation.getCurrentPosition(
            (pos) => {
                setLatitude(pos.coords.latitude);
                setLongitude(pos.coords.longitude);
                setLocationDetected(true);
                setIsDetectingLocation(false);
                setToast({ type: 'success', message: '📍 Location detected successfully!' });
            },
            (err) => {
                setIsDetectingLocation(false);
                let msg = 'Unable to detect location.';
                if (err.code === 1) msg = 'Location permission denied. Please allow location access in your browser settings.';
                else if (err.code === 2) msg = 'Location unavailable. Please enter your address manually.';
                else if (err.code === 3) msg = 'Location request timed out. Please try again.';
                setToast({ type: 'error', message: msg });
            },
            { timeout: 10000, maximumAge: 60000 }
        );
    };

    const handlePlaceOrder = async (e) => {
        e.preventDefault();

        if (!customerName.trim()) {
            setToast({ type: 'error', message: 'Please enter your name' });
            return;
        }
        if (!customerPhone.trim()) {
            setToast({ type: 'error', message: 'Please enter your phone number' });
            return;
        }
        if (orderType === 'delivery' && !deliveryAddress.trim()) {
            setToast({ type: 'error', message: 'Please enter your delivery address' });
            return;
        }
        if (orderType === 'delivery' && deliveryError) {
            setToast({ type: 'error', message: 'We cannot deliver to your selected location' });
            return;
        }

        // Block order if below the required minimum order for the location/zone
        if (orderType === 'delivery' && minOrderRequired > 0 && cartTotal < minOrderRequired) {
            setToast({
                type: 'error',
                message: `Minimum order for ${zoneName || 'this delivery zone'} is ${formatPrice(minOrderRequired)}. Your subtotal is ${formatPrice(cartTotal)}. Please add more items.`
            });
            return;
        }

        try {
            setIsPlacingOrder(true);

            const itemsData = cart.map(item => ({
                product_id: item.is_deal ? undefined : item.product_id || item.id,
                deal_id: item.is_deal ? item.deal_id : undefined,
                product_name: item.name,
                variation_id: item.variation_id || undefined,
                variation_name: item.variation_name || undefined,
                quantity: item.quantity,
                unit_price: item.price
            }));

            const orderData = {
                order_type: orderType,
                customer_name: customerName.trim(),
                customer_phone: customerPhone.trim(),
                customer_email: customerEmail.trim() || undefined,
                delivery_address: orderType === 'delivery' ? deliveryAddress.trim() : undefined,
                delivery_latitude: orderType === 'delivery' && latitude ? parseFloat(latitude) : undefined,
                delivery_longitude: orderType === 'delivery' && longitude ? parseFloat(longitude) : undefined,
                items: itemsData,
                payment_method: paymentMethod,
                admin_notes: adminNotes.trim() || undefined
            };

            const response = await api.post('/orders', orderData);

            if (response.success && response.data) {
                // Check if payment method is JazzCash
                if (paymentMethod === 'jazzcash') {
                    setToast({ type: 'success', message: 'Redirecting to JazzCash...' });
                    try {
                        const checkoutRes = await api.post('/orders/jazzcash/initiate', { orderId: response.data.order.id });
                        if (checkoutRes.success && checkoutRes.postUrl) {
                            const form = document.createElement('form');
                            form.method = 'POST';
                            form.action = checkoutRes.postUrl;

                            // Append parameters as hidden inputs
                            Object.keys(checkoutRes.params).forEach(key => {
                                const input = document.createElement('input');
                                input.type = 'hidden';
                                input.name = key;
                                input.value = checkoutRes.params[key];
                                form.appendChild(input);
                            });

                            document.body.appendChild(form);
                            localStorage.removeItem('guest_checkout');
                            form.submit();
                            return;
                        } else {
                            setToast({ type: 'error', message: checkoutRes.message || 'Failed to initiate payment' });
                            return;
                        }
                    } catch (err) {
                        setToast({ type: 'error', message: err.message || 'Failed to initiate payment' });
                        return;
                    }
                }

                // Default COD flow
                setToast({ type: 'success', message: '🎉 Order placed successfully!' });
                localStorage.removeItem('guest_checkout');
                clearCart();
                setTimeout(() => {
                    navigate(`/orders/${response.data.orderNumber}`);
                }, 1500);
            } else {
                setToast({ type: 'error', message: response.message || 'Failed to place order' });
            }
        } catch (error) {
            setToast({ type: 'error', message: error.message || 'Failed to place order' });
        } finally {
            setIsPlacingOrder(false);
        }
    };

    const tax = cartTotal * (taxRate / 100);
    const finalTotal = cartTotal + tax + (orderType === 'delivery' ? deliveryCharge : 0);

    const orderTypes = [
        { id: 'delivery', label: '🛵 Delivery', desc: 'Delivered to your door' },
        { id: 'takeaway', label: '🛍️ Takeaway', desc: 'Pick up at the counter' },
    ];

    const paymentMethods = [
        {
            id: 'cod',
            label: 'Cash on Delivery',
            icon: '💵',
            desc: 'Pay when your order arrives'
        },
        {
            id: 'online',
            label: 'Pay Online',
            icon: '💳',
            desc: 'JazzCash, EasyPaisa, or Card'
        }
    ];

    return (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 py-4">
            {/* Details Form Column */}
            <form onSubmit={handlePlaceOrder} className="lg:col-span-2 space-y-6">
                <div className="border-b border-gray-100 pb-4">
                    <h1 className="font-heading font-black text-2xl md:text-3xl text-text-primary">
                        Checkout
                    </h1>
                    <p className="text-text-secondary text-sm">Fill in your details to place your order</p>
                </div>

                {/* Order Type */}
                <div className="bg-white rounded-2xl border border-gray-100 p-5 md:p-6 shadow-md space-y-4">
                    <h3 className="font-heading font-bold text-lg text-text-primary">Order Type</h3>
                    <div className="grid grid-cols-2 gap-3">
                        {orderTypes.map((type) => (
                            <button
                                key={type.id}
                                type="button"
                                onClick={() => setOrderType(type.id)}
                                className={`py-4 px-5 rounded-xl font-bold text-sm border-2 transition-all duration-300 flex flex-col items-center gap-1 ${
                                    orderType === type.id
                                        ? 'bg-primary text-white border-primary shadow-md shadow-orange-100/50'
                                        : 'bg-white text-text-primary hover:bg-gray-50 border-gray-200 hover:border-primary/40'
                                }`}
                            >
                                <span className="text-xl">{type.label.split(' ')[0]}</span>
                                <span>{type.label.split(' ')[1]}</span>
                                <span className={`text-[10px] font-normal ${orderType === type.id ? 'text-orange-100' : 'text-text-secondary'}`}>
                                    {type.desc}
                                </span>
                            </button>
                        ))}
                    </div>
                </div>

                {/* Customer Info */}
                <div className="bg-white rounded-2xl border border-gray-100 p-5 md:p-6 shadow-md space-y-4">
                    <h3 className="font-heading font-bold text-lg text-text-primary">Contact Details</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="space-y-1.5">
                            <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Full Name *</label>
                            <input
                                type="text"
                                placeholder="e.g. Ahmed Ali"
                                value={customerName}
                                onChange={(e) => setCustomerName(e.target.value)}
                                className="input-field"
                                required
                            />
                        </div>
                        <div className="space-y-1.5">
                            <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Phone Number *</label>
                            <input
                                type="tel"
                                placeholder="e.g. 03001234567"
                                value={customerPhone}
                                onChange={(e) => setCustomerPhone(e.target.value)}
                                className="input-field"
                                required
                            />
                        </div>
                        <div className="space-y-1.5 md:col-span-2">
                            <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Email Address <span className="text-text-secondary font-normal normal-case">(Optional – for order confirmation)</span></label>
                            <input
                                type="email"
                                placeholder="e.g. ahmed@email.com"
                                value={customerEmail}
                                onChange={(e) => setCustomerEmail(e.target.value)}
                                className="input-field"
                            />
                            {!customerEmail.trim() && (
                                <div className="text-xs text-amber-600 bg-amber-50 border border-amber-200 rounded-xl p-3 font-semibold mt-2 leading-relaxed">
                                    ⚠️ Notice: You will not receive any order confirmation or status update email messages because the email address is left blank.
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Delivery Address */}
                {orderType === 'delivery' && (
                    <div className="bg-white rounded-2xl border border-gray-100 p-5 md:p-6 shadow-md space-y-4 animate-slide-up">
                        <div className="flex items-center justify-between flex-wrap gap-3">
                            <h3 className="font-heading font-bold text-lg text-text-primary">Delivery Address</h3>

                            {/* Detect My Location Button */}
                            <button
                                type="button"
                                onClick={handleDetectLocation}
                                disabled={isDetectingLocation}
                                className={`flex items-center gap-2 text-sm font-bold px-4 py-2 rounded-xl border-2 transition-all duration-200 ${
                                    locationDetected
                                        ? 'border-green-500 text-green-600 bg-green-50 hover:bg-green-100'
                                        : 'border-primary text-primary hover:bg-orange-50 active:scale-95'
                                } ${isDetectingLocation ? 'opacity-70 cursor-wait' : ''}`}
                            >
                                {isDetectingLocation ? (
                                    <>
                                        <svg className="animate-spin w-4 h-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                                        </svg>
                                        Detecting...
                                    </>
                                ) : locationDetected ? (
                                    <>✅ Location Detected</>
                                ) : (
                                    <>📍 Detect My Location</>
                                )}
                            </button>
                        </div>

                        <div className="space-y-4">
                            <div className="space-y-1.5">
                                <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Complete Address *</label>
                                <textarea
                                    rows="3"
                                    placeholder="e.g. House 12, Street 4, Sector G-9/2, Islamabad"
                                    value={deliveryAddress}
                                    onChange={(e) => setDeliveryAddress(e.target.value)}
                                    className="input-field resize-none"
                                    required={orderType === 'delivery'}
                                />
                            </div>

                            {/* Location status indicator */}
                            {isCheckingDelivery && (
                                <div className="flex items-center gap-2 text-xs text-primary animate-pulse">
                                    <svg className="animate-spin w-4 h-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                                    </svg>
                                    Verifying delivery zone...
                                </div>
                            )}

                            {locationDetected && !isCheckingDelivery && !deliveryError && (
                                <div className="space-y-2">
                                    <div className="flex items-center gap-2 bg-green-50 border border-green-200 rounded-xl p-3 text-xs text-green-700 font-semibold">
                                        <span>✅</span>
                                        <span>Location verified — we deliver to your area!</span>
                                        {deliveryCharge > 0 && <span className="ml-auto font-bold">Delivery fee: {formatPrice(deliveryCharge)}</span>}
                                    </div>

                                    {/* Paid delivery zone warning */}
                                    {isPaidZone && (
                                        <div className="flex items-start gap-2 bg-amber-50 border border-amber-300 rounded-xl p-3 text-xs text-amber-800 font-semibold leading-relaxed">
                                            <span className="text-base">⚠️</span>
                                            <span>
                                                <strong>Heads up!</strong> You are outside the free delivery zone. A delivery fee of{' '}
                                                <strong>{formatPrice(deliveryCharge)}</strong> will be added because your location is in the{' '}
                                                <strong>{zoneName || 'paid delivery zone'}</strong>.
                                                {minOrderRequired > 0 && (
                                                    <> You must reach a minimum order of <strong>{formatPrice(minOrderRequired)}</strong> to place this order.</>
                                                )}
                                            </span>
                                        </div>
                                    )}

                                    {/* Minimum order not met warning */}
                                    {minOrderRequired > 0 && cartTotal < minOrderRequired && (
                                        <div className="flex items-start gap-2 bg-red-50 border border-red-300 rounded-xl p-3 text-xs text-red-700 font-semibold leading-relaxed">
                                            <span className="text-base">🚫</span>
                                            <span>
                                                Your current subtotal is <strong>{formatPrice(cartTotal)}</strong>, which is below the required minimum order of{' '}
                                                <strong>{formatPrice(minOrderRequired)}</strong> for this zone. Please add more items before placing the order.
                                            </span>
                                        </div>
                                    )}
                                </div>
                            )}

                            {deliveryError && (
                                <div className="bg-amber-50 text-amber-800 border border-amber-200 rounded-xl p-3 text-xs font-semibold leading-relaxed">
                                    ⚠️ {deliveryError}
                                </div>
                            )}
                        </div>
                    </div>
                )}

                {/* Payment Method */}
                <div className="bg-white rounded-2xl border border-gray-100 p-5 md:p-6 shadow-md space-y-4">
                    <h3 className="font-heading font-bold text-lg text-text-primary">Payment Method</h3>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                        {paymentMethods.map((method) => (
                            <button
                                key={method.id}
                                type="button"
                                onClick={() => setPaymentMethod(method.id)}
                                className={`py-4 px-5 rounded-xl font-bold text-sm border-2 text-left transition-all duration-300 flex items-center gap-4 ${
                                    paymentMethod === method.id
                                        ? 'bg-primary text-white border-primary shadow-md shadow-orange-100/50'
                                        : 'bg-white text-text-primary hover:bg-gray-50 border-gray-200 hover:border-primary/40'
                                }`}
                            >
                                <span className="text-2xl">{method.icon}</span>
                                <div>
                                    <div className="font-extrabold">{method.label}</div>
                                    <div className={`text-[11px] font-normal ${paymentMethod === method.id ? 'text-orange-100' : 'text-text-secondary'}`}>
                                        {method.desc}
                                    </div>
                                </div>
                                {paymentMethod === method.id && (
                                    <span className="ml-auto text-lg">✓</span>
                                )}
                            </button>
                        ))}
                    </div>
                    {paymentMethod === 'online' && (
                        <div className="bg-blue-50 border border-blue-100 rounded-xl p-3 text-xs text-blue-700 font-semibold flex items-center gap-2">
                            💳 You will be prompted to complete the payment after placing the order.
                        </div>
                    )}
                </div>

                {/* Additional instructions */}
                <div className="bg-white rounded-2xl border border-gray-100 p-5 md:p-6 shadow-md space-y-4">
                    <h3 className="font-heading font-bold text-lg text-text-primary">Special Instructions</h3>
                    <div className="space-y-1.5">
                        <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Chef / Rider Notes <span className="font-normal normal-case">(Optional)</span></label>
                        <input
                            type="text"
                            placeholder="e.g. No onions please, extra ketchup, leave at gate"
                            value={adminNotes}
                            onChange={(e) => setAdminNotes(e.target.value)}
                            className="input-field"
                        />
                    </div>
                </div>
            </form>

            {/* Order Summary Column */}
            <div className="space-y-6">
                <h2 className="font-heading font-bold text-xl md:text-2xl text-text-primary border-b border-gray-100 pb-4">
                    Order Summary
                </h2>

                <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-md space-y-5">
                    {/* Items list */}
                    <div className="space-y-3.5 border-b border-gray-100 pb-5 max-h-52 overflow-y-auto">
                        <h4 className="font-bold text-xs uppercase tracking-wider text-text-secondary mb-2">Your Items</h4>
                        {cart.map((item, idx) => (
                            <div key={idx} className="flex justify-between text-xs text-text-primary font-semibold">
                                <span className="line-clamp-1 flex-1 pr-2">{item.quantity}x {item.name}{item.variation_name ? ` (${item.variation_name})` : ''}</span>
                                <span className="shrink-0">{formatPrice(item.price * item.quantity)}</span>
                            </div>
                        ))}
                    </div>

                    {/* Breakdown */}
                    <div className="space-y-3.5 border-b border-gray-100 pb-5 text-sm">
                        <div className="flex justify-between text-text-secondary">
                            <span>Subtotal</span>
                            <span className="font-semibold text-text-primary">{formatPrice(cartTotal)}</span>
                        </div>
                        <div className="flex justify-between text-text-secondary">
                            <span>Tax ({taxRate}% GST)</span>
                            <span className="font-semibold text-text-primary">{formatPrice(tax)}</span>
                        </div>
                        {orderType === 'delivery' && (
                            <div className="flex justify-between text-text-secondary">
                                <span>Delivery Fee</span>
                                <span className="font-semibold text-text-primary">
                                    {deliveryCharge > 0 ? formatPrice(deliveryCharge) : locationDetected ? formatPrice(0) : '—'}
                                </span>
                            </div>
                        )}
                    </div>

                    <div className="flex justify-between items-baseline pt-1">
                        <span className="font-bold text-text-primary">Total</span>
                        <span className="text-2xl font-heading font-black text-primary">
                            {formatPrice(finalTotal)}
                        </span>
                    </div>

                    {/* Order type and payment summary */}
                    <div className="bg-gray-50 rounded-xl p-3 space-y-1.5 text-xs text-text-secondary">
                        <div className="flex justify-between">
                            <span>Order Type</span>
                            <span className="font-bold text-text-primary capitalize">{orderType}</span>
                        </div>
                        <div className="flex justify-between">
                            <span>Payment</span>
                            <span className="font-bold text-text-primary">
                                {paymentMethod === 'cod' ? 'Cash on Delivery' : 'Online Payment'}
                            </span>
                        </div>
                    </div>

                    {!isRestaurantOpen && (
                        <div className="bg-red-100 text-red-700 p-3 rounded-xl text-sm font-medium mb-4 flex items-center gap-2">
                            <span className="text-lg">🚫</span>
                            {closedMessage || "The restaurant is currently closed."}
                        </div>
                    )}
                    <div className="pt-1">
                        <button
                            onClick={handlePlaceOrder}
                            disabled={isPlacingOrder || isCheckingDelivery || !isRestaurantOpen}
                            className="btn-primary w-full text-center font-bold shadow-lg shadow-orange-100/50 py-4 flex items-center justify-center gap-2 hover:shadow-orange-200/50 text-base disabled:opacity-70 disabled:cursor-not-allowed"
                        >
                            {isPlacingOrder ? (
                                <>
                                    <svg className="animate-spin w-5 h-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                                    </svg>
                                    Placing Order...
                                </>
                            ) : '🛒 Place Order'}
                        </button>
                    </div>
                </div>
            </div>

            {toast && (
                <ToastNotification
                    message={toast.message}
                    type={toast.type}
                    onClose={() => setToast(null)}
                />
            )}
        </div>
    );
}
