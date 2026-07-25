// E:\hungryHub\hungry-fast-food\website\frontend\src\components\orders\OrderHistory.jsx

import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../../services/api';
import LoadingSpinner from '../common/LoadingSpinner';
import { formatPrice, formatDate } from '../../utils/helpers';
import { STATUS_COLORS, STATUS_LABELS } from '../../utils/constants';

export default function OrderHistory() {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchOrders = async () => {
            try {
                const response = await api.get('/orders/my-orders');
                if (response.success && response.data) {
                    setOrders(response.data);
                }
            } catch (error) {
                console.error('Failed to fetch orders:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchOrders();
    }, []);

    if (loading) return <LoadingSpinner />;

    return (
        <div className="max-w-4xl mx-auto py-4 space-y-6">
            <div className="border-b border-gray-100 pb-4">
                <h1 className="font-heading font-black text-2xl md:text-3xl text-text-primary">
                    Order History
                </h1>
                <p className="text-text-secondary text-sm">View details and status of your past orders</p>
            </div>

            {orders.length === 0 ? (
                <div className="text-center py-16 bg-white rounded-2xl border border-gray-100 p-8 shadow-sm">
                    <span className="text-6xl block mb-6 select-none">🍔</span>
                    <h3 className="font-heading font-extrabold text-xl text-text-primary mb-1">
                        No orders placed yet
                    </h3>
                    <p className="text-text-secondary text-sm mb-6">
                        Looks like you haven't ordered anything yet. Check out our menu to place your first order.
                    </p>
                    <Link to="/menu" className="btn-primary inline-block font-bold px-8 shadow-lg shadow-orange-100/50">
                        View Menu
                    </Link>
                </div>
            ) : (
                <div className="space-y-4">
                    {orders.map((order) => (
                        <div
                            key={order.id}
                            className="card bg-white border border-gray-100 p-5 md:p-6 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-6 hover:border-orange-150"
                        >
                            {/* Order Details */}
                            <div className="space-y-3 flex-grow">
                                <div className="flex flex-wrap items-center gap-3">
                                    <h3 className="font-heading font-extrabold text-lg text-text-primary">
                                        Order #{order.order_number}
                                    </h3>
                                    <span className={`px-2.5 py-0.5 rounded-full text-xs font-black uppercase ${STATUS_COLORS[order.status]}`}>
                                        {STATUS_LABELS[order.status]}
                                    </span>
                                </div>
                                <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-text-secondary font-semibold">
                                    <span>Placed on: {formatDate(order.created_at)}</span>
                                    <span className="hidden sm:inline">&bull;</span>
                                    <span>Type: {order.order_type?.toUpperCase()}</span>
                                    <span className="hidden sm:inline">&bull;</span>
                                    <span>Items: {order.items ? order.items.length : 0}</span>
                                </div>
                                <p className="text-xs text-text-secondary leading-relaxed max-w-lg">
                                    {order.items && order.items.map((item) => `${item.quantity}x ${item.product_name}`).join(', ')}
                                </p>
                            </div>

                            {/* Total price & Actions */}
                            <div className="flex md:flex-col items-center md:items-end justify-between md:justify-center gap-4 flex-shrink-0 border-t md:border-t-0 border-gray-100 pt-4 md:pt-0">
                                <div className="text-left md:text-right">
                                    <span className="text-xs text-text-secondary font-bold uppercase tracking-wider block">
                                        Total Amount
                                    </span>
                                    <span className="text-xl font-heading font-black text-primary">
                                        {formatPrice(order.total)}
                                    </span>
                                </div>
                                <Link
                                    to={`/orders/${order.order_number}`}
                                    className="btn-outline px-5 py-2 text-xs font-bold text-center block w-full md:w-auto"
                                >
                                    Track Order
                                </Link>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}
