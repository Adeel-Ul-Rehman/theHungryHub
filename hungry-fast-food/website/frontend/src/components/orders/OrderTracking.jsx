// E:\hungryHub\hungry-fast-food\website\frontend\src\components\orders\OrderTracking.jsx

import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { usePolling } from '../../hooks/usePolling';
import { api } from '../../services/api';
import LoadingSpinner from '../common/LoadingSpinner';
import { formatPrice, formatDate } from '../../utils/helpers';
import { STATUS_COLORS, STATUS_LABELS } from '../../utils/constants';

export default function OrderTracking() {
    const { orderNumber } = useParams();

    // Poll order status every 10 seconds
    const { data: orderResponse, loading, error } = usePolling(
        () => api.get(`/orders/track/${orderNumber}`),
        10000,
        [orderNumber]
    );

    if (loading) return <LoadingSpinner />;

    if (error || !orderResponse?.success || !orderResponse?.data) {
        return (
            <div className="max-w-md mx-auto text-center py-16 px-6 bg-white rounded-2xl border border-gray-100 shadow-md">
                <span className="text-6xl block mb-6">⚠️</span>
                <h2 className="font-heading font-extrabold text-2xl text-text-primary mb-2">
                    Failed to Load Status
                </h2>
                <p className="text-text-secondary text-sm mb-8 leading-relaxed">
                    We couldn't retrieve tracking details for order <strong>#{orderNumber}</strong>. Please check your order number.
                </p>
                <Link to="/orders" className="btn-primary inline-block font-bold px-8 shadow-lg shadow-orange-100/50">
                    Back to History
                </Link>
            </div>
        );
    }

    const order = orderResponse.data;

    const steps = ['pending', 'confirmed', 'preparing', 'ready', 'completed'];
    const currentStepIndex = steps.indexOf(order.status);

    return (
        <div className="max-w-3xl mx-auto py-4 space-y-8 animate-fade-in">
            {/* Header info */}
            <div className="bg-white rounded-2xl border border-gray-100 p-6 md:p-8 shadow-md flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div className="space-y-1">
                    <span className="text-xs text-text-secondary font-bold uppercase tracking-wider">
                        Order Tracker
                    </span>
                    <h1 className="font-heading font-black text-2xl md:text-3xl text-text-primary">
                        Order #{order.order_number}
                    </h1>
                    <p className="text-text-secondary text-xs">
                        Placed on {formatDate(order.created_at)}
                    </p>
                </div>
                <span className={`px-4 py-2 rounded-full font-black text-sm uppercase ${STATUS_COLORS[order.status]}`}>
                    {STATUS_LABELS[order.status]}
                </span>
            </div>

            {/* Timeline graphics */}
            <div className="bg-white rounded-2xl border border-gray-100 p-6 md:p-8 shadow-md">
                <h3 className="font-heading font-bold text-lg text-text-primary mb-8 text-center md:text-left">
                    Status Timeline
                </h3>
                {order.status === 'cancelled' ? (
                    <div className="bg-red-50 text-red-700 border border-red-100 p-4 rounded-xl flex items-center gap-3 font-semibold text-sm">
                        <span>❌</span>
                        <span>This order was cancelled. Please contact customer support if you have any questions.</span>
                    </div>
                ) : (
                    <div className="flex flex-col md:flex-row justify-between items-center md:items-start gap-8 md:gap-4 relative">
                        {/* Connecting Line (desktop only) */}
                        <div className="hidden md:block absolute top-6 left-10 right-10 h-1 bg-gray-200 -z-10">
                            <div
                                className="h-full bg-primary transition-all duration-1000"
                                style={{ width: `${(currentStepIndex / (steps.length - 1)) * 100}%` }}
                            ></div>
                        </div>

                        {steps.map((step, index) => {
                            const isCompleted = index <= currentStepIndex;
                            const isCurrent = index === currentStepIndex;
                            
                            const stepIcons = {
                                pending: '📝',
                                confirmed: '👍',
                                preparing: '🍳',
                                ready: '📦',
                                completed: '🎉'
                            };

                            const stepLabels = {
                                pending: 'Pending',
                                confirmed: 'Confirmed',
                                preparing: 'Cooking',
                                ready: 'Ready',
                                completed: 'Delivered'
                            };

                            return (
                                <div key={step} className="flex md:flex-col items-center gap-4 md:gap-3 md:w-32 text-center relative z-10 flex-grow">
                                    <div
                                        className={`w-12 h-12 flex items-center justify-center rounded-full text-xl shadow border-2 transition-all duration-500 ${
                                            isCompleted
                                                ? 'bg-primary text-white border-primary scale-110'
                                                : 'bg-white text-gray-400 border-gray-200'
                                        } ${isCurrent ? 'ring-4 ring-red-100 animate-pulse' : ''}`}
                                    >
                                        {stepIcons[step]}
                                    </div>
                                    <div className="text-left md:text-center">
                                        <h4 className={`text-sm font-extrabold ${isCompleted ? 'text-text-primary' : 'text-gray-400'}`}>
                                            {stepLabels[step]}
                                        </h4>
                                        <p className="text-[10px] text-text-secondary">
                                            {isCompleted ? 'Completed' : 'Upcoming'}
                                        </p>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}
            </div>

            {/* Order Items & Summary */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {/* Items */}
                <div className="md:col-span-2 bg-white rounded-2xl border border-gray-100 p-6 shadow-md space-y-4">
                    <h3 className="font-heading font-bold text-lg text-text-primary border-b border-gray-100 pb-3">
                        Items Ordered
                    </h3>
                    <div className="divide-y divide-gray-100">
                        {order.items && order.items.map((item, idx) => (
                            <div key={idx} className="flex justify-between items-center py-3.5 text-sm font-semibold">
                                <div className="space-y-0.5">
                                    <p className="text-text-primary">{item.quantity}x {item.product_name}</p>
                                    {item.variation_name && (
                                        <span className="text-[10px] text-text-secondary bg-gray-100 px-2 py-0.5 rounded border border-gray-200">
                                            {item.variation_name}
                                        </span>
                                    )}
                                </div>
                                <span className="text-text-primary font-bold">{formatPrice(item.total_price)}</span>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Pricing Summary */}
                <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-md space-y-4 flex flex-col justify-between">
                    <div className="space-y-4">
                        <h3 className="font-heading font-bold text-lg text-text-primary border-b border-gray-100 pb-3">
                            Payment Details
                        </h3>
                        <div className="space-y-2.5 text-xs font-semibold text-text-secondary">
                            <div className="flex justify-between">
                                <span>Subtotal</span>
                                <span className="text-text-primary">{formatPrice(order.subtotal)}</span>
                            </div>
                            <div className="flex justify-between">
                                <span>Tax</span>
                                <span className="text-text-primary">{formatPrice(order.tax)}</span>
                            </div>
                            <div className="flex justify-between">
                                <span>Delivery Fee</span>
                                <span className="text-text-primary">{formatPrice(order.delivery_charge)}</span>
                            </div>
                            <hr className="border-gray-100 my-2" />
                            <div className="flex justify-between text-sm pt-1">
                                <span className="font-bold text-text-primary">Total Paid</span>
                                <span className="font-black text-primary text-base">{formatPrice(order.total)}</span>
                            </div>
                        </div>
                    </div>
                    <div className="text-[11px] text-text-secondary bg-gray-50 border border-gray-200 p-3 rounded-lg leading-relaxed mt-4 font-semibold">
                        <p><strong>Payment Method:</strong> {order.payment_method?.toUpperCase()}</p>
                        <p><strong>Status:</strong> {order.payment_status?.toUpperCase()}</p>
                        {order.delivery_address && (
                            <p className="mt-1"><strong>Deliver to:</strong> {order.delivery_address}</p>
                        )}
                        {order.order_type === 'delivery' && (order.maps_url || (order.delivery_latitude && order.delivery_longitude)) && (() => {
                            const locUrl = order.maps_url ||
                                `https://www.google.com/maps/search/?api=1&query=${order.delivery_latitude},${order.delivery_longitude}`;
                            return (
                                <div className="mt-3 p-3 bg-blue-50 border border-blue-100 rounded-lg space-y-2">
                                    <p className="text-blue-700 font-bold text-[11px]">📍 Delivery Location (QR)</p>
                                    <a href={locUrl} target="_blank" rel="noopener noreferrer">
                                        <img
                                            src={`https://api.qrserver.com/v1/create-qr-code/?size=120x120&data=${encodeURIComponent(locUrl)}`}
                                            alt="Location QR Code"
                                            className="w-28 h-28 rounded border border-blue-200 bg-white"
                                        />
                                    </a>
                                    <a
                                        href={locUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="block text-blue-600 underline text-[11px] break-all"
                                    >
                                        Open location in Google Maps
                                    </a>
                                </div>
                            );
                        })()}
                    </div>
                </div>
            </div>
        </div>
    );
}
