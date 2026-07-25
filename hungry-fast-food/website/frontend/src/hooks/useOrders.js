// E:\hungryHub\hungry-fast-food\website\frontend\src\hooks\useOrders.js

import useSWR from 'swr';
import { api } from '../services/api';

const fetcher = (url) => api.swrFetcher(url, { includeAuth: true });

export function useOrders() {
    const config = {
        refreshInterval: 30000, // Refresh user orders list every 30 seconds
        revalidateOnFocus: true,
        revalidateOnReconnect: true,
        keepPreviousData: true
    };

    const { data: orders, error, mutate } = useSWR('/orders/my-orders', fetcher, {
        ...config,
        fallbackData: (() => {
            try {
                return JSON.parse(localStorage.getItem('cached_user_orders') || '[]');
            } catch {
                return [];
            }
        })(),
        onSuccess: (data) => {
            localStorage.setItem('cached_user_orders', JSON.stringify(data));
        }
    });

    return {
        orders: orders || [],
        isLoading: !orders && !error,
        isError: error,
        mutate
    };
}
